using System;

namespace WoWTrace.Backend.Queue.Attribute
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageType : System.Attribute
    {
        public const string TypeBuild = "build";

        public MessageType(string type)
        {
            Type = type;
        }

        public string Type { get; }
    }
}
