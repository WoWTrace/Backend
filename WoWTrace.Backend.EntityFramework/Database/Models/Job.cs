using System;
using System.Collections.Generic;

namespace WoWTrace.Backend.EntityFramework.Database.Models
{
    public partial class Job
    {
        public ulong Id { get; set; }
        public string Queue { get; set; } = null!;
        public string Payload { get; set; } = null!;
        public byte Attempts { get; set; }
        public uint? ReservedAt { get; set; }
        public uint AvailableAt { get; set; }
        public uint CreatedAt { get; set; }
    }
}
