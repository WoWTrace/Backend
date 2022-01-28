using DotNetWorkQueue.Configuration;

namespace WoWTrace.Backend.Queue.Message.V1
{
    public class ProcessExecutableMessage
    {
        public ulong BuildId;
        public bool Force = false;

        public static void Publish(ulong buildId, bool force = false)
        {
            QueueManager.Instance.Publish(new ProcessExecutableMessage() { BuildId = buildId, Force = force }, QueueManager.Instance.ExecutableV1Queue);
        }
    }
}
