using System;
using System.Collections.Generic;

namespace WoWTrace.Backend.EntityFramework.Database.Models
{
    public partial class ListfileSuggestion
    {
        public string SuggestionKey { get; set; } = null!;
        public ulong Id { get; set; }
        public ulong UserId { get; set; }
        public string Path { get; set; } = null!;
        public string? Type { get; set; }
        public bool? Accepted { get; set; }
        public ulong? ReviewerUserId { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual User? ReviewerUser { get; set; }
        public virtual User User { get; set; } = null!;
    }
}
