namespace WoWTrace.Backend.Exceptions
{
    public class EncryptedBuildConfigException : Exception
    {
        public EncryptedBuildConfigException() : base()
        {
        }

        public EncryptedBuildConfigException(string? message) : base(message)
        {
        }
    }
}
