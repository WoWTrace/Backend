using System;
using System.Collections.Generic;

namespace WoWTrace.Backend.EntityFramework.Database.Models
{
    public partial class Build
    {
        public Build()
        {
            ListfileVersions = new HashSet<ListfileVersion>();
            Ids = new HashSet<Listfile>();
        }

        public ulong Id { get; set; }
        public string BuildConfig { get; set; } = null!;
        public string CdnConfig { get; set; } = null!;
        public string? PatchConfig { get; set; }
        public string? ProductConfig { get; set; }
        public string ProductKey { get; set; } = null!;
        public string Expansion { get; set; } = null!;
        public string Major { get; set; } = null!;
        public string Minor { get; set; } = null!;
        public uint ClientBuild { get; set; }
        public string? Patch { get; set; }
        public string Name { get; set; } = null!;
        public string EncodingContentHash { get; set; } = null!;
        public string EncodingCdnHash { get; set; } = null!;
        public string RootContentHash { get; set; } = null!;
        public string RootCdnHash { get; set; } = null!;
        public string InstallContentHash { get; set; } = null!;
        public string InstallCdnHash { get; set; } = null!;
        public string DownloadContentHash { get; set; } = null!;
        public string DownloadCdnHash { get; set; } = null!;
        public string? SizeContentHash { get; set; }
        public string? SizeCdnHash { get; set; }
        /// <summary>
        /// Builds which contains custom generated configs
        /// </summary>
        public bool Custom { get; set; }
        /// <summary>
        /// Compile time of the Wow.exe
        /// </summary>
        public DateTime? CompiledAt { get; set; }
        /// <summary>
        /// List of process class names which processed this build
        /// </summary>
        public string ProcessedBy { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual Product ProductKeyNavigation { get; set; } = null!;
        public virtual ICollection<ListfileVersion> ListfileVersions { get; set; }

        public virtual ICollection<Listfile> Ids { get; set; }
    }
}
