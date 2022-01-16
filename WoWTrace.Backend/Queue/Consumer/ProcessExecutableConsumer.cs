using DotNetWorkQueue;
using DotNetWorkQueue.Transport.SQLite.Basic;
using System;
using System.Threading;
using System.Threading.Tasks;
using WoWTrace.Backend.Queue.Message;

namespace WoWTrace.Backend.Queue.Consumer
{
    public class ProcessExecutableConsumer : IConsumer
    {
        public int BuildId;

        public void Listen()
        {
            using (var schedulerContainer = new SchedulerContainer())
            {
                using (var scheduler = schedulerContainer.CreateTaskScheduler())
                {
                    var factory = schedulerContainer.CreateTaskFactory(scheduler);
                    factory.Scheduler.Configuration.MaximumThreads = 4;
                    factory.Scheduler.Start();
                    using (var queueContainer = new QueueContainer<SqLiteMessageQueueInit>())
                    {
                        using (var queue = queueContainer.CreateConsumerQueueScheduler(QueueManager.Instance.FastlineQueue, factory))
                        {
                            queue.Start<ProcessExecutableMessage>(HandleMessages);
                            Thread.Sleep(Timeout.Infinite);
                        }
                    }
                }
            }
        }

        private void HandleMessages(IReceivedMessage<ProcessExecutableMessage> message, IWorkerNotification notifications)
        {
            Console.WriteLine("HANDLE BuildId: " + message.Body.BuildId);
        }
    }
}
