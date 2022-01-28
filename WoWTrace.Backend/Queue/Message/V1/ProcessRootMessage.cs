using DotNetWorkQueue.Configuration;

namespace WoWTrace.Backend.Queue.Message.V1
{
    public class ProcessRootMessage
    {
        public ulong BuildId;
        public bool Force = false;

        public static void Publish(ulong buildId, bool force = false)
        {
            QueueManager.Instance.Publish(new ProcessRootMessage() { BuildId = buildId, Force = force }, QueueManager.Instance.RootV1Queue);
        }
    }
}
