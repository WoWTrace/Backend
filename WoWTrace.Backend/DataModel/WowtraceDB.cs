//---------------------------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated by T4Model template for T4 (https://github.com/linq2db/linq2db).
//    Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
// </auto-generated>
//---------------------------------------------------------------------------------------------------

#pragma warning disable 1591

using System;
using System.Collections.Generic;
using System.Linq;

using LinqToDB;
using LinqToDB.Configuration;
using LinqToDB.Mapping;

namespace WoWTrace.Backend.DataModels
{
	/// <summary>
	/// Database       : wowtrace
	/// Data Source    : 127.0.0.1
	/// Server Version : 5.5.5-10.3.32-MariaDB-0ubuntu0.20.04.1
	/// </summary>
	public partial class WowtraceDB : LinqToDB.Data.DataConnection
	{
		public ITable<Attachment>          Attachments          { get { return this.GetTable<Attachment>(); } }
		public ITable<Attachmentable>      Attachmentables      { get { return this.GetTable<Attachmentable>(); } }
		public ITable<Build>               Builds               { get { return this.GetTable<Build>(); } }
		public ITable<FailedJob>           FailedJobs           { get { return this.GetTable<FailedJob>(); } }
		public ITable<Job>                 Jobs                 { get { return this.GetTable<Job>(); } }
		public ITable<Listfile>            Listfiles            { get { return this.GetTable<Listfile>(); } }
		public ITable<ListfileBuild>       ListfileBuilds       { get { return this.GetTable<ListfileBuild>(); } }
		public ITable<ListfileSuggestion>  ListfileSuggestions  { get { return this.GetTable<ListfileSuggestion>(); } }
		public ITable<ListfileVersion>     ListfileVersions     { get { return this.GetTable<ListfileVersion>(); } }
		public ITable<Migration>           Migrations           { get { return this.GetTable<Migration>(); } }
		public ITable<Notification>        Notifications        { get { return this.GetTable<Notification>(); } }
		public ITable<PasswordReset>       PasswordResets       { get { return this.GetTable<PasswordReset>(); } }
		public ITable<PersonalAccessToken> PersonalAccessTokens { get { return this.GetTable<PersonalAccessToken>(); } }
		public ITable<Product>             Products             { get { return this.GetTable<Product>(); } }
		public ITable<Role>                Roles                { get { return this.GetTable<Role>(); } }
		public ITable<RoleUser>            RoleUsers            { get { return this.GetTable<RoleUser>(); } }
		public ITable<User>                Users                { get { return this.GetTable<User>(); } }

		public WowtraceDB()
		{
			InitDataContext();
			InitMappingSchema();
		}

		public WowtraceDB(string configuration)
			: base(configuration)
		{
			InitDataContext();
			InitMappingSchema();
		}

		public WowtraceDB(LinqToDbConnectionOptions options)
			: base(options)
		{
			InitDataContext();
			InitMappingSchema();
		}

		public WowtraceDB(LinqToDbConnectionOptions<WowtraceDB> options)
			: base(options)
		{
			InitDataContext();
			InitMappingSchema();
		}

		partial void InitDataContext  ();
		partial void InitMappingSchema();
	}

	[Table("attachments")]
	public partial class Attachment
	{
		[Column("id"),            PrimaryKey,  Identity] public uint      Id           { get; set; } // int(10) unsigned
		[Column("name"),          NotNull              ] public string    Name         { get; set; } // text
		[Column("original_name"), NotNull              ] public string    OriginalName { get; set; } // text
		[Column("mime"),          NotNull              ] public string    Mime         { get; set; } // varchar(255)
		[Column("extension"),        Nullable          ] public string    Extension    { get; set; } // varchar(255)
		[Column("size"),          NotNull              ] public long      Size         { get; set; } // bigint(20)
		[Column("sort"),          NotNull              ] public int       Sort         { get; set; } // int(11)
		[Column("path"),          NotNull              ] public string    Path         { get; set; } // text
		[Column("description"),      Nullable          ] public string    Description  { get; set; } // text
		[Column("alt"),              Nullable          ] public string    Alt          { get; set; } // text
		[Column("hash"),             Nullable          ] public string    Hash         { get; set; } // text
		[Column("disk"),          NotNull              ] public string    Disk         { get; set; } // varchar(255)
		[Column("user_id"),          Nullable          ] public ulong?    UserId       { get; set; } // bigint(20) unsigned
		[Column("group"),            Nullable          ] public string    Group        { get; set; } // varchar(255)
		[Column("created_at"),       Nullable          ] public DateTime? CreatedAt    { get; set; } // timestamp
		[Column("updated_at"),       Nullable          ] public DateTime? UpdatedAt    { get; set; } // timestamp

		#region Associations

		/// <summary>
		/// attachmentable_attachment_id_foreign_BackReference
		/// </summary>
		[Association(ThisKey="Id", OtherKey="AttachmentId", CanBeNull=true, Relationship=LinqToDB.Mapping.Relationship.OneToMany, IsBackReference=true)]
		public IEnumerable<Attachmentable> Attachmentableattachmentidforeigns { get; set; }

		#endregion
	}

	[Table("attachmentable")]
	public partial class Attachmentable
	{
		[Column("id"),                  PrimaryKey, Identity] public uint   Id                 { get; set; } // int(10) unsigned
		[Column("attachmentable_type"), NotNull             ] public string AttachmentableType { get; set; } // varchar(255)
		[Column("attachmentable_id"),   NotNull             ] public uint   AttachmentableId   { get; set; } // int(10) unsigned
		[Column("attachment_id"),       NotNull             ] public uint   AttachmentId       { get; set; } // int(10) unsigned

		#region Associations

		/// <summary>
		/// attachmentable_attachment_id_foreign
		/// </summary>
		[Association(ThisKey="AttachmentId", OtherKey="Id", CanBeNull=false, Relationship=LinqToDB.Mapping.Relationship.ManyToOne, KeyName="attachmentable_attachment_id_foreign", BackReferenceName="Attachmentableattachmentidforeigns")]
		public Attachment Attachment { get; set; }

		#endregion
	}

	[Table("build")]
	public partial class Build
	{
		[Column("id"),                                                        PrimaryKey,  Identity] public ulong     Id                  { get; set; } // bigint(20) unsigned
		[Column("buildConfig"),                                               NotNull              ] public string    BuildConfig         { get; set; } // char(32)
		[Column("cdnConfig"),                                                 NotNull              ] public string    CdnConfig           { get; set; } // char(32)
		[Column("patchConfig"),                                                  Nullable          ] public string    PatchConfig         { get; set; } // char(32)
		[Column("productConfig"),                                                Nullable          ] public string    ProductConfig       { get; set; } // char(32)
		[Column("productKey"),                                                NotNull              ] public string    ProductKey          { get; set; } // varchar(32)
		[Column("expansion"),                                                 NotNull              ] public string    Expansion           { get; set; } // varchar(4)
		[Column("major"),                                                     NotNull              ] public string    Major               { get; set; } // varchar(4)
		[Column("minor"),                                                     NotNull              ] public string    Minor               { get; set; } // varchar(4)
		[Column("clientBuild"),                                               NotNull              ] public uint      ClientBuild         { get; set; } // int(10) unsigned
		[Column("patch",               SkipOnInsert=true, SkipOnUpdate=true),    Nullable          ] public string    Patch               { get; set; } // varchar(14)
		[Column("name"),                                                      NotNull              ] public string    Name                { get; set; } // varchar(255)
		[Column("encodingContentHash"),                                       NotNull              ] public string    EncodingContentHash { get; set; } // char(32)
		[Column("encodingCdnHash"),                                           NotNull              ] public string    EncodingCdnHash     { get; set; } // char(32)
		[Column("rootContentHash"),                                           NotNull              ] public string    RootContentHash     { get; set; } // char(32)
		[Column("rootCdnHash"),                                               NotNull              ] public string    RootCdnHash         { get; set; } // char(32)
		[Column("installContentHash"),                                        NotNull              ] public string    InstallContentHash  { get; set; } // char(32)
		[Column("installCdnHash"),                                            NotNull              ] public string    InstallCdnHash      { get; set; } // char(32)
		[Column("downloadContentHash"),                                       NotNull              ] public string    DownloadContentHash { get; set; } // char(32)
		[Column("downloadCdnHash"),                                           NotNull              ] public string    DownloadCdnHash     { get; set; } // char(32)
		[Column("sizeContentHash"),                                              Nullable          ] public string    SizeContentHash     { get; set; } // char(32)
		[Column("sizeCdnHash"),                                                  Nullable          ] public string    SizeCdnHash         { get; set; } // char(32)
		/// <summary>
		/// Builds which contains custom generated configs
		/// </summary>
		[Column("custom"),                                                    NotNull              ] public bool      Custom              { get; set; } // tinyint(1)
		/// <summary>
		/// Compile time of the Wow.exe
		/// </summary>
		[Column("compiledAt"),                                                   Nullable          ] public DateTime? CompiledAt          { get; set; } // timestamp
		/// <summary>
		/// List of process class names which processed this build
		/// </summary>
		[Column("processedBy"),                                               NotNull              ] public string    ProcessedBy         { get; set; } // longtext
		[Column("created_at"),                                                   Nullable          ] public DateTime? CreatedAt           { get; set; } // timestamp
		[Column("updated_at"),                                                   Nullable          ] public DateTime? UpdatedAt           { get; set; } // timestamp

		#region Associations

		/// <summary>
		/// listfile_builds_buildid_foreign_BackReference
		/// </summary>
		[Association(ThisKey="Id", OtherKey="BuildId", CanBeNull=true, Relationship=LinqToDB.Mapping.Relationship.OneToMany, IsBackReference=true)]
		public IEnumerable<ListfileBuild> Listfilebuildsbuildidforeigns { get; set; }

		/// <summary>
		/// listfile_version_buildid_foreign_BackReference
		/// </summary>
		[Association(ThisKey="Id", OtherKey="FirstBuildId", CanBeNull=true, Relationship=LinqToDB.Mapping.Relationship.OneToMany, IsBackReference=true)]
		public IEnumerable<ListfileVersion> Listfileversionbuildidforeigns { get; set; }

		/// <summary>
		/// build_product_foreign
		/// </summary>
		[Association(ThisKey="ProductKey", OtherKey="ProductColumn", CanBeNull=false, Relationship=LinqToDB.Mapping.Relationship.ManyToOne, KeyName="build_product_foreign", BackReferenceName="Buildforeigns")]
		public Product Productforeign { get; set; }

		#endregion
	}

	[Table("failed_jobs")]
	public partial class FailedJob
	{
		[Column("id"),         PrimaryKey, Identity] public ulong    Id         { get; set; } // bigint(20) unsigned
		[Column("uuid"),       NotNull             ] public string   Uuid       { get; set; } // varchar(255)
		[Column("connection"), NotNull             ] public string   Connection { get; set; } // text
		[Column("queue"),      NotNull             ] public string   Queue      { get; set; } // text
		[Column("payload"),    NotNull             ] public string   Payload    { get; set; } // longtext
		[Column("exception"),  NotNull             ] public string   Exception  { get; set; } // longtext
		[Column("failed_at"),  NotNull             ] public DateTime FailedAt   { get; set; } // timestamp
	}

	[Table("jobs")]
	public partial class Job
	{
		[Column("id"),           PrimaryKey,  Identity] public ulong  Id          { get; set; } // bigint(20) unsigned
		[Column("queue"),        NotNull              ] public string Queue       { get; set; } // varchar(255)
		[Column("payload"),      NotNull              ] public string Payload     { get; set; } // longtext
		[Column("attempts"),     NotNull              ] public byte   Attempts    { get; set; } // tinyint(3) unsigned
		[Column("reserved_at"),     Nullable          ] public uint?  ReservedAt  { get; set; } // int(10) unsigned
		[Column("available_at"), NotNull              ] public uint   AvailableAt { get; set; } // int(10) unsigned
		[Column("created_at"),   NotNull              ] public uint   CreatedAt   { get; set; } // int(10) unsigned
	}

	[Table("listfile")]
	public partial class Listfile
	{
		[Column("id"),            PrimaryKey,  Identity] public ulong     Id            { get; set; } // bigint(20) unsigned
		[Column("path"),             Nullable          ] public string    Path          { get; set; } // varchar(255)
		[Column("type"),             Nullable          ] public string    Type          { get; set; } // varchar(20)
		[Column("userId"),           Nullable          ] public ulong?    UserId        { get; set; } // bigint(20) unsigned
		[Column("lookup"),           Nullable          ] public string    Lookup        { get; set; } // char(16)
		[Column("verified"),      NotNull              ] public bool      Verified      { get; set; } // tinyint(1)
		[Column("pathDiscovery"),    Nullable          ] public DateTime? PathDiscovery { get; set; } // timestamp
		[Column("created_at"),       Nullable          ] public DateTime? CreatedAt     { get; set; } // timestamp
		[Column("updated_at"),       Nullable          ] public DateTime? UpdatedAt     { get; set; } // timestamp

		#region Associations

		/// <summary>
		/// listfile_builds_id_foreign_BackReference
		/// </summary>
		[Association(ThisKey="Id", OtherKey="Id", CanBeNull=true, Relationship=LinqToDB.Mapping.Relationship.OneToMany, IsBackReference=true)]
		public IEnumerable<ListfileBuild> Buildsidforeigns { get; set; }

		/// <summary>
		/// listfile_user_id_foreign
		/// </summary>
		[Association(ThisKey="UserId", OtherKey="Id", CanBeNull=true, Relationship=LinqToDB.Mapping.Relationship.ManyToOne, KeyName="listfile_user_id_foreign", BackReferenceName="Listfileuseridforeigns")]
		public User User { get; set; }

		/// <summary>
		/// listfile_version_id_foreign_BackReference
		/// </summary>
		[Association(ThisKey="Id", OtherKey="Id", CanBeNull=true, Relationship=LinqToDB.Mapping.Relationship.OneToMany, IsBackReference=true)]
		public IEnumerable<ListfileVersion> Versionidforeigns { get; set; }

		#endregion
	}

	[Table("listfile_builds")]
	public partial class ListfileBuild
	{
		[Column("id"),      PrimaryKey(1), NotNull] public ulong Id      { get; set; } // bigint(20) unsigned
		[Column("buildId"), PrimaryKey(2), NotNull] public ulong BuildId { get; set; } // bigint(20) unsigned

		#region Associations

		/// <summary>
		/// listfile_builds_buildid_foreign
		/// </summary>
		[Association(ThisKey="BuildId", OtherKey="Id", CanBeNull=false, Relationship=LinqToDB.Mapping.Relationship.ManyToOne, KeyName="listfile_builds_buildid_foreign", BackReferenceName="Listfilebuildsbuildidforeigns")]
		public Build Build { get; set; }

		/// <summary>
		/// listfile_builds_id_foreign
		/// </summary>
		[Association(ThisKey="Id", OtherKey="Id", CanBeNull=false, Relationship=LinqToDB.Mapping.Relationship.ManyToOne, KeyName="listfile_builds_id_foreign", BackReferenceName="Buildsidforeigns")]
		public Listfile Listfile { get; set; }

		#endregion
	}

	[Table("listfile_suggestion")]
	public partial class ListfileSuggestion
	{
		[Column("suggestionKey"),                 NotNull] public string    SuggestionKey  { get; set; } // varchar(255)
		[Column("id"),             PrimaryKey(1), NotNull] public ulong     Id             { get; set; } // bigint(20) unsigned
		[Column("userId"),         PrimaryKey(2), NotNull] public ulong     UserId         { get; set; } // bigint(20) unsigned
		[Column("path"),           PrimaryKey(3), NotNull] public string    Path           { get; set; } // varchar(255)
		[Column("type"),              Nullable           ] public string    Type           { get; set; } // varchar(20)
		[Column("accepted"),          Nullable           ] public bool?     Accepted       { get; set; } // tinyint(1)
		[Column("reviewerUserId"),    Nullable           ] public ulong?    ReviewerUserId { get; set; } // bigint(20) unsigned
		[Column("reviewedAt"),        Nullable           ] public DateTime? ReviewedAt     { get; set; } // timestamp
		[Column("created_at"),        Nullable           ] public DateTime? CreatedAt      { get; set; } // timestamp
		[Column("updated_at"),        Nullable           ] public DateTime? UpdatedAt      { get; set; } // timestamp

		#region Associations

		/// <summary>
		/// listfile_suggestion_revieweruserid_foreign
		/// </summary>
		[Association(ThisKey="ReviewerUserId", OtherKey="Id", CanBeNull=true, Relationship=LinqToDB.Mapping.Relationship.ManyToOne, KeyName="listfile_suggestion_revieweruserid_foreign", BackReferenceName="Listfilesuggestionrevieweruseridforeigns")]
		public User ReviewerUser { get; set; }

		/// <summary>
		/// listfile_suggestion_userid_foreign
		/// </summary>
		[Association(ThisKey="UserId", OtherKey="Id", CanBeNull=false, Relationship=LinqToDB.Mapping.Relationship.ManyToOne, KeyName="listfile_suggestion_userid_foreign", BackReferenceName="Listfilesuggestionuseridforeigns")]
		public User User { get; set; }

		#endregion
	}

	[Table("listfile_version")]
	public partial class ListfileVersion
	{
		[Column("id"),           PrimaryKey(1), NotNull] public ulong     Id           { get; set; } // bigint(20) unsigned
		[Column("contentHash"),  PrimaryKey(2), NotNull] public string    ContentHash  { get; set; } // char(32)
		[Column("encrypted"),                   NotNull] public bool      Encrypted    { get; set; } // tinyint(1)
		[Column("fileSize"),        Nullable           ] public uint?     FileSize     { get; set; } // int(10) unsigned
		[Column("processed"),                   NotNull] public bool      Processed    { get; set; } // tinyint(1)
		[Column("firstBuildId"),                NotNull] public ulong     FirstBuildId { get; set; } // bigint(20) unsigned
		[Column("clientBuild"),                 NotNull] public uint      ClientBuild  { get; set; } // int(10) unsigned
		[Column("created_at"),      Nullable           ] public DateTime? CreatedAt    { get; set; } // timestamp
		[Column("updated_at"),      Nullable           ] public DateTime? UpdatedAt    { get; set; } // timestamp

		#region Associations

		/// <summary>
		/// listfile_version_buildid_foreign
		/// </summary>
		[Association(ThisKey="FirstBuildId", OtherKey="Id", CanBeNull=false, Relationship=LinqToDB.Mapping.Relationship.ManyToOne, KeyName="listfile_version_buildid_foreign", BackReferenceName="Listfileversionbuildidforeigns")]
		public Build FirstBuild { get; set; }

		/// <summary>
		/// listfile_version_id_foreign
		/// </summary>
		[Association(ThisKey="Id", OtherKey="Id", CanBeNull=false, Relationship=LinqToDB.Mapping.Relationship.ManyToOne, KeyName="listfile_version_id_foreign", BackReferenceName="Versionidforeigns")]
		public Listfile Listfile { get; set; }

		#endregion
	}

	[Table("migrations")]
	public partial class Migration
	{
		[Column("id"),        PrimaryKey, Identity] public uint   Id              { get; set; } // int(10) unsigned
		[Column("migration"), NotNull             ] public string MigrationColumn { get; set; } // varchar(255)
		[Column("batch"),     NotNull             ] public int    Batch           { get; set; } // int(11)
	}

	[Table("notifications")]
	public partial class Notification
	{
		[Column("id"),              PrimaryKey,  NotNull] public string    Id             { get; set; } // char(36)
		[Column("type"),                         NotNull] public string    Type           { get; set; } // varchar(255)
		[Column("notifiable_type"),              NotNull] public string    NotifiableType { get; set; } // varchar(255)
		[Column("notifiable_id"),                NotNull] public ulong     NotifiableId   { get; set; } // bigint(20) unsigned
		[Column("data"),                         NotNull] public string    Data           { get; set; } // text
		[Column("read_at"),            Nullable         ] public DateTime? ReadAt         { get; set; } // timestamp
		[Column("created_at"),         Nullable         ] public DateTime? CreatedAt      { get; set; } // timestamp
		[Column("updated_at"),         Nullable         ] public DateTime? UpdatedAt      { get; set; } // timestamp
	}

	[Table("password_resets")]
	public partial class PasswordReset
	{
		[Column("email"),      NotNull    ] public string    Email     { get; set; } // varchar(255)
		[Column("token"),      NotNull    ] public string    Token     { get; set; } // varchar(255)
		[Column("created_at"),    Nullable] public DateTime? CreatedAt { get; set; } // timestamp
	}

	[Table("personal_access_tokens")]
	public partial class PersonalAccessToken
	{
		[Column("id"),             PrimaryKey,  Identity] public ulong     Id            { get; set; } // bigint(20) unsigned
		[Column("tokenable_type"), NotNull              ] public string    TokenableType { get; set; } // varchar(255)
		[Column("tokenable_id"),   NotNull              ] public ulong     TokenableId   { get; set; } // bigint(20) unsigned
		[Column("name"),           NotNull              ] public string    Name          { get; set; } // varchar(255)
		[Column("token"),          NotNull              ] public string    Token         { get; set; } // varchar(64)
		[Column("abilities"),         Nullable          ] public string    Abilities     { get; set; } // text
		[Column("last_used_at"),      Nullable          ] public DateTime? LastUsedAt    { get; set; } // timestamp
		[Column("created_at"),        Nullable          ] public DateTime? CreatedAt     { get; set; } // timestamp
		[Column("updated_at"),        Nullable          ] public DateTime? UpdatedAt     { get; set; } // timestamp
	}

	[Table("product")]
	public partial class Product
	{
		[Column("id"),              PrimaryKey,  Identity] public ulong     Id              { get; set; } // bigint(20) unsigned
		[Column("product"),         NotNull              ] public string    ProductColumn   { get; set; } // varchar(32)
		[Column("name"),            NotNull              ] public string    Name            { get; set; } // varchar(255)
		[Column("badgeText"),       NotNull              ] public string    BadgeText       { get; set; } // varchar(255)
		[Column("badgeType"),       NotNull              ] public string    BadgeType       { get; set; } // varchar(255)
		[Column("encrypted"),       NotNull              ] public bool      Encrypted       { get; set; } // tinyint(1)
		[Column("lastVersion"),        Nullable          ] public string    LastVersion     { get; set; } // varchar(30)
		[Column("lastBuildConfig"),    Nullable          ] public string    LastBuildConfig { get; set; } // varchar(32)
		[Column("detected"),           Nullable          ] public DateTime? Detected        { get; set; } // timestamp
		[Column("created_at"),         Nullable          ] public DateTime? CreatedAt       { get; set; } // timestamp
		[Column("updated_at"),         Nullable          ] public DateTime? UpdatedAt       { get; set; } // timestamp

		#region Associations

		/// <summary>
		/// build_product_foreign_BackReference
		/// </summary>
		[Association(ThisKey="ProductColumn", OtherKey="ProductKey", CanBeNull=true, Relationship=LinqToDB.Mapping.Relationship.OneToMany, IsBackReference=true)]
		public IEnumerable<Build> Buildforeigns { get; set; }

		#endregion
	}

	[Table("roles")]
	public partial class Role
	{
		[Column("id"),          PrimaryKey,  Identity] public uint      Id          { get; set; } // int(10) unsigned
		[Column("slug"),        NotNull              ] public string    Slug        { get; set; } // varchar(255)
		[Column("name"),        NotNull              ] public string    Name        { get; set; } // varchar(255)
		[Column("permissions"),    Nullable          ] public string    Permissions { get; set; } // longtext
		[Column("created_at"),     Nullable          ] public DateTime? CreatedAt   { get; set; } // timestamp
		[Column("updated_at"),     Nullable          ] public DateTime? UpdatedAt   { get; set; } // timestamp

		#region Associations

		/// <summary>
		/// role_users_role_id_foreign_BackReference
		/// </summary>
		[Association(ThisKey="Id", OtherKey="RoleId", CanBeNull=true, Relationship=LinqToDB.Mapping.Relationship.OneToMany, IsBackReference=true)]
		public IEnumerable<RoleUser> Roleusersroleidforeigns { get; set; }

		#endregion
	}

	[Table("role_users")]
	public partial class RoleUser
	{
		[Column("user_id"), PrimaryKey(1), NotNull] public ulong UserId { get; set; } // bigint(20) unsigned
		[Column("role_id"), PrimaryKey(2), NotNull] public uint  RoleId { get; set; } // int(10) unsigned

		#region Associations

		/// <summary>
		/// role_users_role_id_foreign
		/// </summary>
		[Association(ThisKey="RoleId", OtherKey="Id", CanBeNull=false, Relationship=LinqToDB.Mapping.Relationship.ManyToOne, KeyName="role_users_role_id_foreign", BackReferenceName="Roleusersroleidforeigns")]
		public Role Role { get; set; }

		/// <summary>
		/// role_users_user_id_foreign
		/// </summary>
		[Association(ThisKey="UserId", OtherKey="Id", CanBeNull=false, Relationship=LinqToDB.Mapping.Relationship.ManyToOne, KeyName="role_users_user_id_foreign", BackReferenceName="Roleuseridforeigns")]
		public User User { get; set; }

		#endregion
	}

	[Table("users")]
	public partial class User
	{
		[Column("id"),                PrimaryKey,  Identity] public ulong     Id              { get; set; } // bigint(20) unsigned
		[Column("name"),              NotNull              ] public string    Name            { get; set; } // varchar(255)
		[Column("email"),             NotNull              ] public string    Email           { get; set; } // varchar(255)
		[Column("email_verified_at"),    Nullable          ] public DateTime? EmailVerifiedAt { get; set; } // timestamp
		[Column("password"),          NotNull              ] public string    Password        { get; set; } // varchar(255)
		[Column("remember_token"),       Nullable          ] public string    RememberToken   { get; set; } // varchar(100)
		[Column("created_at"),           Nullable          ] public DateTime? CreatedAt       { get; set; } // timestamp
		[Column("updated_at"),           Nullable          ] public DateTime? UpdatedAt       { get; set; } // timestamp
		[Column("permissions"),          Nullable          ] public string    Permissions     { get; set; } // longtext

		#region Associations

		/// <summary>
		/// listfile_suggestion_revieweruserid_foreign_BackReference
		/// </summary>
		[Association(ThisKey="Id", OtherKey="ReviewerUserId", CanBeNull=true, Relationship=LinqToDB.Mapping.Relationship.OneToMany, IsBackReference=true)]
		public IEnumerable<ListfileSuggestion> Listfilesuggestionrevieweruseridforeigns { get; set; }

		/// <summary>
		/// listfile_suggestion_userid_foreign_BackReference
		/// </summary>
		[Association(ThisKey="Id", OtherKey="UserId", CanBeNull=true, Relationship=LinqToDB.Mapping.Relationship.OneToMany, IsBackReference=true)]
		public IEnumerable<ListfileSuggestion> Listfilesuggestionuseridforeigns { get; set; }

		/// <summary>
		/// listfile_user_id_foreign_BackReference
		/// </summary>
		[Association(ThisKey="Id", OtherKey="UserId", CanBeNull=true, Relationship=LinqToDB.Mapping.Relationship.OneToMany, IsBackReference=true)]
		public IEnumerable<Listfile> Listfileuseridforeigns { get; set; }

		/// <summary>
		/// role_users_user_id_foreign_BackReference
		/// </summary>
		[Association(ThisKey="Id", OtherKey="UserId", CanBeNull=true, Relationship=LinqToDB.Mapping.Relationship.OneToMany, IsBackReference=true)]
		public IEnumerable<RoleUser> Roleuseridforeigns { get; set; }

		#endregion
	}

	public static partial class TableExtensions
	{
		public static Attachment Find(this ITable<Attachment> table, uint Id)
		{
			return table.FirstOrDefault(t =>
				t.Id == Id);
		}

		public static Attachmentable Find(this ITable<Attachmentable> table, uint Id)
		{
			return table.FirstOrDefault(t =>
				t.Id == Id);
		}

		public static Build Find(this ITable<Build> table, ulong Id)
		{
			return table.FirstOrDefault(t =>
				t.Id == Id);
		}

		public static FailedJob Find(this ITable<FailedJob> table, ulong Id)
		{
			return table.FirstOrDefault(t =>
				t.Id == Id);
		}

		public static Job Find(this ITable<Job> table, ulong Id)
		{
			return table.FirstOrDefault(t =>
				t.Id == Id);
		}

		public static Listfile Find(this ITable<Listfile> table, ulong Id)
		{
			return table.FirstOrDefault(t =>
				t.Id == Id);
		}

		public static ListfileBuild Find(this ITable<ListfileBuild> table, ulong Id, ulong BuildId)
		{
			return table.FirstOrDefault(t =>
				t.Id      == Id &&
				t.BuildId == BuildId);
		}

		public static ListfileSuggestion Find(this ITable<ListfileSuggestion> table, ulong Id, ulong UserId, string Path)
		{
			return table.FirstOrDefault(t =>
				t.Id     == Id     &&
				t.UserId == UserId &&
				t.Path   == Path);
		}

		public static ListfileVersion Find(this ITable<ListfileVersion> table, ulong Id, string ContentHash)
		{
			return table.FirstOrDefault(t =>
				t.Id          == Id &&
				t.ContentHash == ContentHash);
		}

		public static Migration Find(this ITable<Migration> table, uint Id)
		{
			return table.FirstOrDefault(t =>
				t.Id == Id);
		}

		public static Notification Find(this ITable<Notification> table, string Id)
		{
			return table.FirstOrDefault(t =>
				t.Id == Id);
		}

		public static PersonalAccessToken Find(this ITable<PersonalAccessToken> table, ulong Id)
		{
			return table.FirstOrDefault(t =>
				t.Id == Id);
		}

		public static Product Find(this ITable<Product> table, ulong Id)
		{
			return table.FirstOrDefault(t =>
				t.Id == Id);
		}

		public static Role Find(this ITable<Role> table, uint Id)
		{
			return table.FirstOrDefault(t =>
				t.Id == Id);
		}

		public static RoleUser Find(this ITable<RoleUser> table, ulong UserId, uint RoleId)
		{
			return table.FirstOrDefault(t =>
				t.UserId == UserId &&
				t.RoleId == RoleId);
		}

		public static User Find(this ITable<User> table, ulong Id)
		{
			return table.FirstOrDefault(t =>
				t.Id == Id);
		}
	}
}

#pragma warning restore 1591