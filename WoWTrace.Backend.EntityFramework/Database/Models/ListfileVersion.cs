using System;
using System.Collections.Generic;

namespace WoWTrace.Backend.EntityFramework.Database.Models
{
    public partial class ListfileVersion
    {
        public ulong Id { get; set; }
        public string ContentHash { get; set; } = null!;
        public bool Encrypted { get; set; }
        public uint? FileSize { get; set; }
        /// <summary>
        /// List of process class names which processed this build
        /// </summary>
        public string ProcessedBy { get; set; } = null!;
        public ulong FirstBuildId { get; set; }
        public uint ClientBuild { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual Build FirstBuild { get; set; } = null!;
        public virtual Listfile IdNavigation { get; set; } = null!;
    }
}
