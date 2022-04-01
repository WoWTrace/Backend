using System;
using System.Collections.Generic;

namespace WoWTrace.Backend.EntityFramework.Database.Models
{
    public partial class Product
    {
        public Product()
        {
            Builds = new HashSet<Build>();
        }

        public ulong Id { get; set; }
        public string Product1 { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string BadgeText { get; set; } = null!;
        public string BadgeType { get; set; } = null!;
        public bool Encrypted { get; set; }
        public string? LastVersion { get; set; }
        public string? LastBuildConfig { get; set; }
        public DateTime? Detected { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<Build> Builds { get; set; }
    }
}
