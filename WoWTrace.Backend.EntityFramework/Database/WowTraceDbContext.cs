using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using WoWTrace.Backend.EntityFramework.Database.Models;

namespace WoWTrace.Backend.Database
{
    public partial class WowTraceDbContext : DbContext
    {
        public WowTraceDbContext()
        {
        }

        public WowTraceDbContext(DbContextOptions<WowTraceDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Attachment> Attachments { get; set; } = null!;
        public virtual DbSet<Attachmentable> Attachmentables { get; set; } = null!;
        public virtual DbSet<Build> Builds { get; set; } = null!;
        public virtual DbSet<FailedJob> FailedJobs { get; set; } = null!;
        public virtual DbSet<Job> Jobs { get; set; } = null!;
        public virtual DbSet<Listfile> Listfiles { get; set; } = null!;
        public virtual DbSet<ListfileSuggestion> ListfileSuggestions { get; set; } = null!;
        public virtual DbSet<ListfileVersion> ListfileVersions { get; set; } = null!;
        public virtual DbSet<Migration> Migrations { get; set; } = null!;
        public virtual DbSet<Notification> Notifications { get; set; } = null!;
        public virtual DbSet<PasswordReset> PasswordResets { get; set; } = null!;
        public virtual DbSet<PersonalAccessToken> PersonalAccessTokens { get; set; } = null!;
        public virtual DbSet<Product> Products { get; set; } = null!;
        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("utf8mb4_general_ci")
                .HasCharSet("utf8mb4");

            modelBuilder.Entity<Attachment>(entity =>
            {
                entity.ToTable("attachments");

                entity.HasCharSet("utf8")
                    .UseCollation("utf8_unicode_ci");

                entity.Property(e => e.Id)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("id");

                entity.Property(e => e.Alt)
                    .HasColumnType("text")
                    .HasColumnName("alt");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("created_at");

                entity.Property(e => e.Description)
                    .HasColumnType("text")
                    .HasColumnName("description");

                entity.Property(e => e.Disk)
                    .HasMaxLength(255)
                    .HasColumnName("disk")
                    .HasDefaultValueSql("'public'");

                entity.Property(e => e.Extension)
                    .HasMaxLength(255)
                    .HasColumnName("extension");

                entity.Property(e => e.Group)
                    .HasMaxLength(255)
                    .HasColumnName("group");

                entity.Property(e => e.Hash)
                    .HasColumnType("text")
                    .HasColumnName("hash");

                entity.Property(e => e.Mime)
                    .HasMaxLength(255)
                    .HasColumnName("mime");

                entity.Property(e => e.Name)
                    .HasColumnType("text")
                    .HasColumnName("name");

                entity.Property(e => e.OriginalName)
                    .HasColumnType("text")
                    .HasColumnName("original_name");

                entity.Property(e => e.Path)
                    .HasColumnType("text")
                    .HasColumnName("path");

                entity.Property(e => e.Size)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("size");

                entity.Property(e => e.Sort)
                    .HasColumnType("int(11)")
                    .HasColumnName("sort");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("updated_at");

                entity.Property(e => e.UserId)
                    .HasColumnType("bigint(20) unsigned")
                    .HasColumnName("user_id");
            });

            modelBuilder.Entity<Attachmentable>(entity =>
            {
                entity.ToTable("attachmentable");

                entity.HasCharSet("utf8")
                    .UseCollation("utf8_unicode_ci");

                entity.HasIndex(e => e.AttachmentId, "attachmentable_attachment_id_foreign");

                entity.HasIndex(e => new { e.AttachmentableType, e.AttachmentableId }, "attachmentable_attachmentable_type_attachmentable_id_index");

                entity.Property(e => e.Id)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("id");

                entity.Property(e => e.AttachmentId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("attachment_id");

                entity.Property(e => e.AttachmentableId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("attachmentable_id");

                entity.Property(e => e.AttachmentableType).HasColumnName("attachmentable_type");

                entity.HasOne(d => d.Attachment)
                    .WithMany(p => p.Attachmentables)
                    .HasForeignKey(d => d.AttachmentId)
                    .HasConstraintName("attachmentable_attachment_id_foreign");
            });

            modelBuilder.Entity<Build>(entity =>
            {
                entity.ToTable("build");

                entity.HasCharSet("utf8")
                    .UseCollation("utf8_unicode_ci");

                entity.HasIndex(e => e.BuildConfig, "build_buildconfig_unique")
                    .IsUnique();

                entity.HasIndex(e => e.CdnConfig, "build_cdnconfig_index");

                entity.HasIndex(e => e.ProductKey, "build_product_foreign");

                entity.HasIndex(e => e.RootCdnHash, "build_rootcdnhash_index");

                entity.Property(e => e.Id)
                    .HasColumnType("bigint(20) unsigned")
                    .HasColumnName("id");

                entity.Property(e => e.BuildConfig)
                    .HasMaxLength(32)
                    .HasColumnName("buildConfig")
                    .IsFixedLength();

                entity.Property(e => e.CdnConfig)
                    .HasMaxLength(32)
                    .HasColumnName("cdnConfig")
                    .IsFixedLength();

                entity.Property(e => e.ClientBuild)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("clientBuild");

                entity.Property(e => e.CompiledAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("compiledAt")
                    .HasComment("Compile time of the Wow.exe");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("created_at");

                entity.Property(e => e.Custom)
                    .HasColumnName("custom")
                    .HasComment("Builds which contains custom generated configs");

                entity.Property(e => e.DownloadCdnHash)
                    .HasMaxLength(32)
                    .HasColumnName("downloadCdnHash")
                    .IsFixedLength();

                entity.Property(e => e.DownloadContentHash)
                    .HasMaxLength(32)
                    .HasColumnName("downloadContentHash")
                    .IsFixedLength();

                entity.Property(e => e.EncodingCdnHash)
                    .HasMaxLength(32)
                    .HasColumnName("encodingCdnHash")
                    .IsFixedLength();

                entity.Property(e => e.EncodingContentHash)
                    .HasMaxLength(32)
                    .HasColumnName("encodingContentHash")
                    .IsFixedLength();

                entity.Property(e => e.Expansion)
                    .HasMaxLength(4)
                    .HasColumnName("expansion");

                entity.Property(e => e.InstallCdnHash)
                    .HasMaxLength(32)
                    .HasColumnName("installCdnHash")
                    .IsFixedLength();

                entity.Property(e => e.InstallContentHash)
                    .HasMaxLength(32)
                    .HasColumnName("installContentHash")
                    .IsFixedLength();

                entity.Property(e => e.Major)
                    .HasMaxLength(4)
                    .HasColumnName("major");

                entity.Property(e => e.Minor)
                    .HasMaxLength(4)
                    .HasColumnName("minor");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.Patch)
                    .HasMaxLength(14)
                    .HasColumnName("patch")
                    .HasComputedColumnSql("concat(`expansion`,'.',`major`,'.',`minor`)", false);

                entity.Property(e => e.PatchConfig)
                    .HasMaxLength(32)
                    .HasColumnName("patchConfig")
                    .IsFixedLength();

                entity.Property(e => e.ProcessedBy)
                    .HasColumnType("json")
                    .HasColumnName("processedBy")
                    .HasComment("List of process class names which processed this build");

                entity.Property(e => e.ProductConfig)
                    .HasMaxLength(32)
                    .HasColumnName("productConfig")
                    .IsFixedLength();

                entity.Property(e => e.ProductKey)
                    .HasMaxLength(32)
                    .HasColumnName("productKey");

                entity.Property(e => e.RootCdnHash)
                    .HasMaxLength(32)
                    .HasColumnName("rootCdnHash")
                    .IsFixedLength();

                entity.Property(e => e.RootContentHash)
                    .HasMaxLength(32)
                    .HasColumnName("rootContentHash")
                    .IsFixedLength();

                entity.Property(e => e.SizeCdnHash)
                    .HasMaxLength(32)
                    .HasColumnName("sizeCdnHash")
                    .IsFixedLength();

                entity.Property(e => e.SizeContentHash)
                    .HasMaxLength(32)
                    .HasColumnName("sizeContentHash")
                    .IsFixedLength();

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("updated_at");

                entity.HasOne(d => d.ProductKeyNavigation)
                    .WithMany(p => p.Builds)
                    .HasPrincipalKey(p => p.Product1)
                    .HasForeignKey(d => d.ProductKey)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("build_product_foreign");
            });

            modelBuilder.Entity<FailedJob>(entity =>
            {
                entity.ToTable("failed_jobs");

                entity.HasCharSet("utf8")
                    .UseCollation("utf8_unicode_ci");

                entity.HasIndex(e => e.Uuid, "failed_jobs_uuid_unique")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("bigint(20) unsigned")
                    .HasColumnName("id");

                entity.Property(e => e.Connection)
                    .HasColumnType("text")
                    .HasColumnName("connection");

                entity.Property(e => e.Exception).HasColumnName("exception");

                entity.Property(e => e.FailedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("failed_at")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.Payload).HasColumnName("payload");

                entity.Property(e => e.Queue)
                    .HasColumnType("text")
                    .HasColumnName("queue");

                entity.Property(e => e.Uuid).HasColumnName("uuid");
            });

            modelBuilder.Entity<Job>(entity =>
            {
                entity.ToTable("jobs");

                entity.HasCharSet("utf8")
                    .UseCollation("utf8_unicode_ci");

                entity.HasIndex(e => e.Queue, "jobs_queue_index");

                entity.Property(e => e.Id)
                    .HasColumnType("bigint(20) unsigned")
                    .HasColumnName("id");

                entity.Property(e => e.Attempts)
                    .HasColumnType("tinyint(3) unsigned")
                    .HasColumnName("attempts");

                entity.Property(e => e.AvailableAt)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("available_at");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("created_at");

                entity.Property(e => e.Payload).HasColumnName("payload");

                entity.Property(e => e.Queue).HasColumnName("queue");

                entity.Property(e => e.ReservedAt)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("reserved_at");
            });

            modelBuilder.Entity<Listfile>(entity =>
            {
                entity.ToTable("listfile");

                entity.HasCharSet("utf8")
                    .UseCollation("utf8_unicode_ci");

                entity.HasIndex(e => e.Lookup, "listfile_lookup_unique")
                    .IsUnique();

                entity.HasIndex(e => e.Path, "listfile_path_fulltext")
                    .IsUnique()
                    .HasAnnotation("MySql:FullTextIndex", true);

                entity.HasIndex(e => e.PathDiscovery, "listfile_pathdiscovery_index");

                entity.HasIndex(e => e.Type, "listfile_type_index");

                entity.HasIndex(e => e.UserId, "listfile_user_id_foreign");

                entity.Property(e => e.Id)
                    .HasColumnType("bigint(20) unsigned")
                    .HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("created_at");

                entity.Property(e => e.Lookup)
                    .HasMaxLength(16)
                    .HasColumnName("lookup")
                    .IsFixedLength();

                entity.Property(e => e.Path).HasColumnName("path");

                entity.Property(e => e.PathDiscovery)
                    .HasColumnType("timestamp")
                    .HasColumnName("pathDiscovery");

                entity.Property(e => e.Type)
                    .HasMaxLength(20)
                    .HasColumnName("type");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("updated_at");

                entity.Property(e => e.UserId)
                    .HasColumnType("bigint(20) unsigned")
                    .HasColumnName("userId");

                entity.Property(e => e.Verified).HasColumnName("verified");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Listfiles)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("listfile_user_id_foreign");

                entity.HasMany(d => d.Builds)
                    .WithMany(p => p.Ids)
                    .UsingEntity<Dictionary<string, object>>(
                        "ListfileBuild",
                        l => l.HasOne<Build>().WithMany().HasForeignKey("BuildId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("listfile_builds_buildid_foreign"),
                        r => r.HasOne<Listfile>().WithMany().HasForeignKey("Id").HasConstraintName("listfile_builds_id_foreign"),
                        j =>
                        {
                            j.HasKey("Id", "BuildId").HasName("PRIMARY").HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                            j.ToTable("listfile_builds").HasCharSet("utf8").UseCollation("utf8_unicode_ci");

                            j.HasIndex(new[] { "BuildId" }, "listfile_builds_buildid_foreign");

                            j.IndexerProperty<ulong>("Id").HasColumnType("bigint(20) unsigned").HasColumnName("id");

                            j.IndexerProperty<ulong>("BuildId").HasColumnType("bigint(20) unsigned").HasColumnName("buildId");
                        });
            });

            modelBuilder.Entity<ListfileSuggestion>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.UserId, e.Path })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0, 0 });

                entity.ToTable("listfile_suggestion");

                entity.HasCharSet("utf8")
                    .UseCollation("utf8_unicode_ci");

                entity.HasIndex(e => e.Accepted, "listfile_suggestion_accepted_index");

                entity.HasIndex(e => e.ReviewerUserId, "listfile_suggestion_revieweruserid_foreign");

                entity.HasIndex(e => e.SuggestionKey, "listfile_suggestion_suggestionkey_index");

                entity.HasIndex(e => e.UserId, "listfile_suggestion_userid_foreign");

                entity.Property(e => e.Id)
                    .HasColumnType("bigint(20) unsigned")
                    .HasColumnName("id");

                entity.Property(e => e.UserId)
                    .HasColumnType("bigint(20) unsigned")
                    .HasColumnName("userId");

                entity.Property(e => e.Path).HasColumnName("path");

                entity.Property(e => e.Accepted).HasColumnName("accepted");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("created_at");

                entity.Property(e => e.ReviewedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("reviewedAt");

                entity.Property(e => e.ReviewerUserId)
                    .HasColumnType("bigint(20) unsigned")
                    .HasColumnName("reviewerUserId");

                entity.Property(e => e.SuggestionKey).HasColumnName("suggestionKey");

                entity.Property(e => e.Type)
                    .HasMaxLength(20)
                    .HasColumnName("type");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("updated_at");

                entity.HasOne(d => d.ReviewerUser)
                    .WithMany(p => p.ListfileSuggestionReviewerUsers)
                    .HasForeignKey(d => d.ReviewerUserId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("listfile_suggestion_revieweruserid_foreign");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.ListfileSuggestionUsers)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("listfile_suggestion_userid_foreign");
            });

            modelBuilder.Entity<ListfileVersion>(entity =>
            {
                entity.HasKey(e => new { e.Id, e.ContentHash })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.ToTable("listfile_version");

                entity.HasCharSet("utf8")
                    .UseCollation("utf8_unicode_ci");

                entity.HasIndex(e => e.FirstBuildId, "listfile_version_buildid_foreign");

                entity.HasIndex(e => e.ClientBuild, "listfile_version_clientbuild_index");

                entity.Property(e => e.Id)
                    .HasColumnType("bigint(20) unsigned")
                    .HasColumnName("id");

                entity.Property(e => e.ContentHash)
                    .HasMaxLength(32)
                    .HasColumnName("contentHash")
                    .IsFixedLength();

                entity.Property(e => e.ClientBuild)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("clientBuild");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("created_at");

                entity.Property(e => e.Encrypted).HasColumnName("encrypted");

                entity.Property(e => e.FileSize)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("fileSize");

                entity.Property(e => e.FirstBuildId)
                    .HasColumnType("bigint(20) unsigned")
                    .HasColumnName("firstBuildId");

                entity.Property(e => e.ProcessedBy)
                    .HasColumnType("json")
                    .HasColumnName("processedBy")
                    .HasComment("List of process class names which processed this build");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("updated_at");

                entity.HasOne(d => d.FirstBuild)
                    .WithMany(p => p.ListfileVersions)
                    .HasForeignKey(d => d.FirstBuildId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("listfile_version_buildid_foreign");

                entity.HasOne(d => d.IdNavigation)
                    .WithMany(p => p.ListfileVersions)
                    .HasForeignKey(d => d.Id)
                    .HasConstraintName("listfile_version_id_foreign");
            });

            modelBuilder.Entity<Migration>(entity =>
            {
                entity.ToTable("migrations");

                entity.HasCharSet("utf8")
                    .UseCollation("utf8_unicode_ci");

                entity.Property(e => e.Id)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("id");

                entity.Property(e => e.Batch)
                    .HasColumnType("int(11)")
                    .HasColumnName("batch");

                entity.Property(e => e.Migration1)
                    .HasMaxLength(255)
                    .HasColumnName("migration");
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("notifications");

                entity.HasCharSet("utf8")
                    .UseCollation("utf8_unicode_ci");

                entity.HasIndex(e => new { e.NotifiableType, e.NotifiableId }, "notifications_notifiable_type_notifiable_id_index");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("created_at");

                entity.Property(e => e.Data)
                    .HasColumnType("text")
                    .HasColumnName("data");

                entity.Property(e => e.NotifiableId)
                    .HasColumnType("bigint(20) unsigned")
                    .HasColumnName("notifiable_id");

                entity.Property(e => e.NotifiableType).HasColumnName("notifiable_type");

                entity.Property(e => e.ReadAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("read_at");

                entity.Property(e => e.Type)
                    .HasMaxLength(255)
                    .HasColumnName("type");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<PasswordReset>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("password_resets");

                entity.HasCharSet("utf8")
                    .UseCollation("utf8_unicode_ci");

                entity.HasIndex(e => e.Email, "password_resets_email_index");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("created_at");

                entity.Property(e => e.Email).HasColumnName("email");

                entity.Property(e => e.Token)
                    .HasMaxLength(255)
                    .HasColumnName("token");
            });

            modelBuilder.Entity<PersonalAccessToken>(entity =>
            {
                entity.ToTable("personal_access_tokens");

                entity.HasCharSet("utf8")
                    .UseCollation("utf8_unicode_ci");

                entity.HasIndex(e => e.Token, "personal_access_tokens_token_unique")
                    .IsUnique();

                entity.HasIndex(e => new { e.TokenableType, e.TokenableId }, "personal_access_tokens_tokenable_type_tokenable_id_index");

                entity.Property(e => e.Id)
                    .HasColumnType("bigint(20) unsigned")
                    .HasColumnName("id");

                entity.Property(e => e.Abilities)
                    .HasColumnType("text")
                    .HasColumnName("abilities");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("created_at");

                entity.Property(e => e.LastUsedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("last_used_at");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.Token)
                    .HasMaxLength(64)
                    .HasColumnName("token");

                entity.Property(e => e.TokenableId)
                    .HasColumnType("bigint(20) unsigned")
                    .HasColumnName("tokenable_id");

                entity.Property(e => e.TokenableType).HasColumnName("tokenable_type");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("product");

                entity.HasCharSet("utf8")
                    .UseCollation("utf8_unicode_ci");

                entity.HasIndex(e => e.Product1, "product_product_unique")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("bigint(20) unsigned")
                    .HasColumnName("id");

                entity.Property(e => e.BadgeText)
                    .HasMaxLength(255)
                    .HasColumnName("badgeText");

                entity.Property(e => e.BadgeType)
                    .HasMaxLength(255)
                    .HasColumnName("badgeType");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("created_at");

                entity.Property(e => e.Detected)
                    .HasColumnType("timestamp")
                    .HasColumnName("detected");

                entity.Property(e => e.Encrypted).HasColumnName("encrypted");

                entity.Property(e => e.LastBuildConfig)
                    .HasMaxLength(32)
                    .HasColumnName("lastBuildConfig");

                entity.Property(e => e.LastVersion)
                    .HasMaxLength(30)
                    .HasColumnName("lastVersion");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.Product1)
                    .HasMaxLength(32)
                    .HasColumnName("product");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("roles");

                entity.HasCharSet("utf8")
                    .UseCollation("utf8_unicode_ci");

                entity.HasIndex(e => e.Slug, "roles_slug_unique")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("created_at");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.Permissions)
                    .HasColumnType("json")
                    .HasColumnName("permissions");

                entity.Property(e => e.Slug).HasColumnName("slug");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("updated_at");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.HasCharSet("utf8")
                    .UseCollation("utf8_unicode_ci");

                entity.HasIndex(e => e.Email, "users_email_unique")
                    .IsUnique();

                entity.Property(e => e.Id)
                    .HasColumnType("bigint(20) unsigned")
                    .HasColumnName("id");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("created_at");

                entity.Property(e => e.Email).HasColumnName("email");

                entity.Property(e => e.EmailVerifiedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("email_verified_at");

                entity.Property(e => e.Name)
                    .HasMaxLength(255)
                    .HasColumnName("name");

                entity.Property(e => e.Password)
                    .HasMaxLength(255)
                    .HasColumnName("password");

                entity.Property(e => e.Permissions)
                    .HasColumnType("json")
                    .HasColumnName("permissions");

                entity.Property(e => e.RememberToken)
                    .HasMaxLength(100)
                    .HasColumnName("remember_token");

                entity.Property(e => e.UpdatedAt)
                    .HasColumnType("timestamp")
                    .HasColumnName("updated_at");

                entity.HasMany(d => d.Roles)
                    .WithMany(p => p.Users)
                    .UsingEntity<Dictionary<string, object>>(
                        "RoleUser",
                        l => l.HasOne<Role>().WithMany().HasForeignKey("RoleId").HasConstraintName("role_users_role_id_foreign"),
                        r => r.HasOne<User>().WithMany().HasForeignKey("UserId").HasConstraintName("role_users_user_id_foreign"),
                        j =>
                        {
                            j.HasKey("UserId", "RoleId").HasName("PRIMARY").HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                            j.ToTable("role_users").HasCharSet("utf8").UseCollation("utf8_unicode_ci");

                            j.HasIndex(new[] { "RoleId" }, "role_users_role_id_foreign");

                            j.IndexerProperty<ulong>("UserId").HasColumnType("bigint(20) unsigned").HasColumnName("user_id");

                            j.IndexerProperty<uint>("RoleId").HasColumnType("int(10) unsigned").HasColumnName("role_id");
                        });
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
