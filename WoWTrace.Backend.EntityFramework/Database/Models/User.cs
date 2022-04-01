using System;
using System.Collections.Generic;

namespace WoWTrace.Backend.EntityFramework.Database.Models
{
    public partial class User
    {
        public User()
        {
            ListfileSuggestionReviewerUsers = new HashSet<ListfileSuggestion>();
            ListfileSuggestionUsers = new HashSet<ListfileSuggestion>();
            Listfiles = new HashSet<Listfile>();
            Roles = new HashSet<Role>();
        }

        public ulong Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTime? EmailVerifiedAt { get; set; }
        public string Password { get; set; } = null!;
        public string? RememberToken { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? Permissions { get; set; }

        public virtual ICollection<ListfileSuggestion> ListfileSuggestionReviewerUsers { get; set; }
        public virtual ICollection<ListfileSuggestion> ListfileSuggestionUsers { get; set; }
        public virtual ICollection<Listfile> Listfiles { get; set; }

        public virtual ICollection<Role> Roles { get; set; }
    }
}
