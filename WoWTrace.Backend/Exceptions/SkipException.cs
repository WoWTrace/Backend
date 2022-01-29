using System;

namespace WoWTrace.Backend.Exceptions
{
    public class SkipException : Exception
    {
        public SkipException() : base()
        {
        }

        public SkipException(string? message) : base(message)
        {
        }
    }
}
