using DotNetWorkQueue;
using DotNetWorkQueue.Configuration;
using DotNetWorkQueue.Transport.SQLite.Basic;
using System;
using System.Linq;
using System.Threading;
using WoWTrace.Backend.Queue.Consumer;
using WoWTrace.Backend.Queue.Message;

namespace WoWTrace.Backend.Queue
{
    public sealed class QueueManager
    {
        public QueueConnection MainlineQueue;
        public QueueConnection FastlineQueue;
        private static Lazy<QueueManager> lazy = new Lazy<QueueManager>(() => new QueueManager(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static QueueManager Instance { get { return lazy.Value; } }

        private QueueManager()
        {
            MainlineQueue = CreateQueue("mainlineQueue", Settings.Instance.QueueConnectionString);
            FastlineQueue = CreateQueue("fastlineQueue", Settings.Instance.QueueConnectionString);

            InitializeConsumer();
        }

        protected QueueConnection CreateQueue(string queueName, string connectionString)
        {
            var queueConnection = new QueueConnection(queueName, connectionString);
            using (var createQueueContainer = new QueueCreationContainer<SqLiteMessageQueueInit>())
            {
                using (var createQueue = createQueueContainer.GetQueueCreation<SqLiteMessageQueueCreation>(queueConnection))
                {
                    if (!createQueue.QueueExists)
                        createQueue.CreateQueue();
                }
            }

            return queueConnection;
        }

        protected void InitializeConsumer()
        {
            IConsumer[] comsumer = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(comsumers => comsumers.GetTypes())
                .Where(typeof(IConsumer).IsAssignableFrom)
                .Where(comsumerType => !comsumerType.IsInterface)
                .Select(comsumerInstance => (IConsumer)Activator.CreateInstance(comsumerInstance))
                .ToArray();

            foreach (IConsumer consumer in comsumer)
                (new Thread(() => { consumer.Listen(); })).Start();


            (new Thread(() => { 
                Thread.Sleep(5000);
                

            })).Start();

            

            var a = 2;
        }

        public void Publish<T>(T message, QueueConnection queueConnection = null) where T : class
        {
            using (var queueContainer = new QueueContainer<SqLiteMessageQueueInit>())
            using (var queue = queueContainer.CreateProducer<T>(queueConnection ?? Instance.MainlineQueue))
            {
                queue.Send(message);
            }
        }

        public void Initialize()
        {
            // Initialize in constructor
        }
    }
}
