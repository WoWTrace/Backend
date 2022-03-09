namespace WoWTrace.Backend.Queue.Consumer
{
    public interface IConsumer
    {
        public void Listen();
        public bool AlreadyProcessedCheck(ulong buildId);
        public void MarkAsProcessed(ulong buildId);
    }
}