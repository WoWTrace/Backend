using System;
using System.Collections.Generic;

namespace WoWTrace.Backend.EntityFramework.Database.Models
{
    public partial class Attachment
    {
        public Attachment()
        {
            Attachmentables = new HashSet<Attachmentable>();
        }

        public uint Id { get; set; }
        public string Name { get; set; } = null!;
        public string OriginalName { get; set; } = null!;
        public string Mime { get; set; } = null!;
        public string? Extension { get; set; }
        public long Size { get; set; }
        public int Sort { get; set; }
        public string Path { get; set; } = null!;
        public string? Description { get; set; }
        public string? Alt { get; set; }
        public string? Hash { get; set; }
        public string Disk { get; set; } = null!;
        public ulong? UserId { get; set; }
        public string? Group { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<Attachmentable> Attachmentables { get; set; }
    }
}
