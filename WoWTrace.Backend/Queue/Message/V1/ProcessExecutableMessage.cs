using WoWTrace.Backend.Queue.Attribute;

namespace WoWTrace.Backend.Queue.Message.V1
{
    [MessageType(MessageType.TypeBuild)]
    public class ProcessExecutableMessage : IQueueMessage
    {
        public readonly ulong BuildId;
        public readonly bool Force;

        public ProcessExecutableMessage(ulong buildId, bool force = false)
        {
            BuildId = buildId;
            Force = force;
        }

        public static void Publish(ulong buildId, bool force = false)
        {
            new ProcessExecutableMessage(buildId, force).PublishMessage();
        }

        public void PublishMessage()
        {
            QueueManager.Instance.Publish(this, QueueManager.Instance.ExecutableV1Queue);
        }
    }
}