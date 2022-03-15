using DotNetWorkQueue;
using DotNetWorkQueue.Configuration;
using DotNetWorkQueue.Transport.SQLite.Basic;

using WoWTrace.Backend.Queue.Consumer;

namespace WoWTrace.Backend.Queue
{
    public sealed class QueueManager
    {
        public static QueueManager Instance => _lazy.Value;

        public readonly QueueConnection RootV1Queue;
        public readonly QueueConnection ExecutableV1Queue;

        private static readonly Lazy<QueueManager> _lazy = new(() => new(), LazyThreadSafetyMode.ExecutionAndPublication);
        private readonly List<Thread> _runningConsumers = new();

        private QueueManager()
        {
            RootV1Queue = CreateQueue("rootV1Queue", Settings.Instance.QueueConnectionString);
            ExecutableV1Queue = CreateQueue("executableV1Queue", Settings.Instance.QueueConnectionString);

            InitializeConsumer();
        }

        private QueueConnection CreateQueue(string queueName, string connectionString)
        {
            var queueConnection = new QueueConnection(queueName, connectionString);

            using (var createQueueContainer = new QueueCreationContainer<SqLiteMessageQueueInit>())
            using (var createQueue = createQueueContainer.GetQueueCreation<SqLiteMessageQueueCreation>(queueConnection))
            {
                if (!createQueue.QueueExists)
                    createQueue.CreateQueue();
            }

            return queueConnection;
        }

        private void InitializeConsumer()
        {
            var consumers = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(consumers => consumers.GetTypes())
                .Where(typeof(IConsumer).IsAssignableFrom)
                .Where(consumerType => !consumerType.IsInterface)
                .Select(consumerInstance => (IConsumer?)Activator.CreateInstance(consumerInstance))
                .ToArray();

            foreach (var consumer in consumers)
            {
                var thread =  new Thread(() =>
                {
                    consumer?.Listen();
                });
                thread.Start();

                _runningConsumers.Add(thread);
            }
        }

        public void Publish<T>(T message, QueueConnection queueConnection) where T : class
        {
            using (var queueContainer = new QueueContainer<SqLiteMessageQueueInit>())
            using (var queue = queueContainer.CreateProducer<T>(queueConnection))
            {
                queue.Send(message);
            }
        }

        public void Initialize()
        {
            // Initialize in constructor
        }

        public void Shutdown()
        {
            foreach (var consumerThread in _runningConsumers)
                consumerThread.Join();

            _runningConsumers.Clear();
        }
    }
}