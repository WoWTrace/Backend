using System;
using System.Collections.Generic;

namespace WoWTrace.Backend.EntityFramework.Database.Models
{
    public partial class Listfile
    {
        public Listfile()
        {
            ListfileVersions = new HashSet<ListfileVersion>();
            Builds = new HashSet<Build>();
        }

        public ulong Id { get; set; }
        public string? Path { get; set; }
        public string? Type { get; set; }
        public ulong? UserId { get; set; }
        public string? Lookup { get; set; }
        public bool Verified { get; set; }
        public DateTime? PathDiscovery { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual User? User { get; set; }
        public virtual ICollection<ListfileVersion> ListfileVersions { get; set; }

        public virtual ICollection<Build> Builds { get; set; }
    }
}
