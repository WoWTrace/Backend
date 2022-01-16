using DotNetWorkQueue;
using DotNetWorkQueue.Transport.SQLite.Basic;
using System;
using System.Threading;
using WoWTrace.Backend.Queue.Message.V1;

namespace WoWTrace.Backend.Queue.Consumer.V1
{
    public class ProcessExecutableConsumer : IConsumer
    {
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
