using System;
using System.Collections.Generic;

namespace WoWTrace.Backend.EntityFramework.Database.Models
{
    public partial class Attachmentable
    {
        public uint Id { get; set; }
        public string AttachmentableType { get; set; } = null!;
        public uint AttachmentableId { get; set; }
        public uint AttachmentId { get; set; }

        public virtual Attachment Attachment { get; set; } = null!;
    }
}
