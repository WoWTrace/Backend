using WoWTrace.Backend.Queue.Attribute;

namespace WoWTrace.Backend.Queue.Message.V1
{
    [MessageType(MessageType.TypeBuild)]
    public class ProcessRootMessage : IQueueMessage
    {
        public readonly ulong BuildId;
        public readonly bool Force;

        public ProcessRootMessage(ulong buildId, bool force = false)
        {
            BuildId = buildId;
            Force = force;
        }

        public static void Publish(ulong buildId, bool force = false)
        {
            QueueManager.Instance.Publish(new ProcessRootMessage(buildId, force), QueueManager.Instance.RootV1Queue);
        }

        public void PublishMessage()
        {
            QueueManager.Instance.Publish(this, QueueManager.Instance.RootV1Queue);
        }
    }
}