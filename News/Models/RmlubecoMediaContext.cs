using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace News.Models;

public partial class RmlubecoMediaContext : DbContext
{
    public RmlubecoMediaContext()
    {
    }

    public RmlubecoMediaContext(DbContextOptions<RmlubecoMediaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Advantage> Advantages { get; set; }

    public virtual DbSet<AggregatedCounter> AggregatedCounters { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<CategoryForNews> CategoryForNews { get; set; }

    public virtual DbSet<Contact> Contacts { get; set; }

    public virtual DbSet<ContactFormSubmission> ContactFormSubmissions { get; set; }

    public virtual DbSet<Counter> Counters { get; set; }

    public virtual DbSet<Hash> Hashes { get; set; }

    public virtual DbSet<Job> Jobs { get; set; }

    public virtual DbSet<JobParameter> JobParameters { get; set; }

    public virtual DbSet<JobQueue> JobQueues { get; set; }

    public virtual DbSet<List> Lists { get; set; }

    public virtual DbSet<Photo> Photos { get; set; }

    public virtual DbSet<Schema> Schemas { get; set; }

    public virtual DbSet<Server> Servers { get; set; }

    public virtual DbSet<Set> Sets { get; set; }

    public virtual DbSet<State> States { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<Subscribe> Subscribes { get; set; }

    public virtual DbSet<TitleColor> TitleColors { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Xeberler> Xeberlers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=wdb4.my-hosting-panel.com;Database=rmlubeco_media;User Id=rmlubeco_media;Password=MediaTimes@123;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("rmlubeco_media");

        modelBuilder.Entity<Advantage>(entity =>
        {
            entity.HasKey(e => e.AdvantageId).HasName("PK__Advantag__4E5FB0DE432AC29C");

            entity.ToTable("Advantage");

            entity.Property(e => e.AdvantageName).HasMaxLength(50);
        });

        modelBuilder.Entity<AggregatedCounter>(entity =>
        {
            entity.HasKey(e => e.Key).HasName("PK_HangFire_CounterAggregated");

            entity.ToTable("AggregatedCounter", "HangFire");

            entity.HasIndex(e => e.ExpireAt, "IX_HangFire_AggregatedCounter_ExpireAt").HasFilter("([ExpireAt] IS NOT NULL)");

            entity.Property(e => e.Key).HasMaxLength(100);
            entity.Property(e => e.ExpireAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Category__19093A0BF1797EF0");

            entity.ToTable("Category");

            entity.Property(e => e.CategoryNameAz).HasMaxLength(100);
            entity.Property(e => e.CategoryNameEn).HasMaxLength(100);
            entity.Property(e => e.CategoryNameRu).HasMaxLength(100);
        });

        modelBuilder.Entity<CategoryForNews>(entity =>
        {
            entity.HasKey(e => e.CategoryForNewsId).HasName("PK__Category__D490C5344B1B457C");

            entity.HasOne(d => d.CategoryForNewsCategory).WithMany(p => p.CategoryForNews)
                .HasForeignKey(d => d.CategoryForNewsCategoryId)
                .HasConstraintName("FK__CategoryF__Categ__2B3F6F97");

            entity.HasOne(d => d.CategoryForNewsNews).WithMany(p => p.CategoryForNews)
                .HasForeignKey(d => d.CategoryForNewsNewsId)
                .HasConstraintName("FK__CategoryF__Categ__2C3393D0");
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.ContactId).HasName("PK__Contact__5C66259BD4BED124");

            entity.ToTable("Contact");

            entity.Property(e => e.ContactEmail).HasMaxLength(50);
            entity.Property(e => e.ContactName).HasMaxLength(50);
        });

        modelBuilder.Entity<ContactFormSubmission>(entity =>
        {
            entity.HasKey(e => e.ContactId).HasName("PK__ContactF__5C66259BE3CB10D7");

            entity.Property(e => e.ContactEmail).HasMaxLength(100);
            entity.Property(e => e.ContactName).HasMaxLength(100);
            entity.Property(e => e.ContactPhone).HasMaxLength(20);
            entity.Property(e => e.ContactSubmissionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<Counter>(entity =>
        {
            entity.HasKey(e => new { e.Key, e.Id }).HasName("PK_HangFire_Counter");

            entity.ToTable("Counter", "HangFire");

            entity.Property(e => e.Key).HasMaxLength(100);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ExpireAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Hash>(entity =>
        {
            entity.HasKey(e => new { e.Key, e.Field }).HasName("PK_HangFire_Hash");

            entity.ToTable("Hash", "HangFire");

            entity.HasIndex(e => e.ExpireAt, "IX_HangFire_Hash_ExpireAt").HasFilter("([ExpireAt] IS NOT NULL)");

            entity.Property(e => e.Key).HasMaxLength(100);
            entity.Property(e => e.Field).HasMaxLength(100);
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_HangFire_Job");

            entity.ToTable("Job", "HangFire");

            entity.HasIndex(e => e.ExpireAt, "IX_HangFire_Job_ExpireAt").HasFilter("([ExpireAt] IS NOT NULL)");

            entity.HasIndex(e => e.StateName, "IX_HangFire_Job_StateName").HasFilter("([StateName] IS NOT NULL)");

            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ExpireAt).HasColumnType("datetime");
            entity.Property(e => e.StateName).HasMaxLength(20);
        });

        modelBuilder.Entity<JobParameter>(entity =>
        {
            entity.HasKey(e => new { e.JobId, e.Name }).HasName("PK_HangFire_JobParameter");

            entity.ToTable("JobParameter", "HangFire");

            entity.Property(e => e.Name).HasMaxLength(40);

            entity.HasOne(d => d.Job).WithMany(p => p.JobParameters)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK_HangFire_JobParameter_Job");
        });

        modelBuilder.Entity<JobQueue>(entity =>
        {
            entity.HasKey(e => new { e.Queue, e.Id }).HasName("PK_HangFire_JobQueue");

            entity.ToTable("JobQueue", "HangFire");

            entity.Property(e => e.Queue).HasMaxLength(50);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.FetchedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<List>(entity =>
        {
            entity.HasKey(e => new { e.Key, e.Id }).HasName("PK_HangFire_List");

            entity.ToTable("List", "HangFire");

            entity.HasIndex(e => e.ExpireAt, "IX_HangFire_List_ExpireAt").HasFilter("([ExpireAt] IS NOT NULL)");

            entity.Property(e => e.Key).HasMaxLength(100);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ExpireAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Photo>(entity =>
        {
            entity.HasKey(e => e.PhotoId).HasName("PK__Photo__21B7B5E235484A69");

            entity.ToTable("Photo");

            entity.Property(e => e.PhotoUrl).HasMaxLength(100);

            entity.HasOne(d => d.PhotoNews).WithMany(p => p.Photos)
                .HasForeignKey(d => d.PhotoNewsId)
                .HasConstraintName("FK__Photo__PhotoNews__267ABA7A");
        });

        modelBuilder.Entity<Schema>(entity =>
        {
            entity.HasKey(e => e.Version).HasName("PK_HangFire_Schema");

            entity.ToTable("Schema", "HangFire");

            entity.Property(e => e.Version).ValueGeneratedNever();
        });

        modelBuilder.Entity<Server>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_HangFire_Server");

            entity.ToTable("Server", "HangFire");

            entity.HasIndex(e => e.LastHeartbeat, "IX_HangFire_Server_LastHeartbeat");

            entity.Property(e => e.Id).HasMaxLength(200);
            entity.Property(e => e.LastHeartbeat).HasColumnType("datetime");
        });

        modelBuilder.Entity<Set>(entity =>
        {
            entity.HasKey(e => new { e.Key, e.Value }).HasName("PK_HangFire_Set");

            entity.ToTable("Set", "HangFire");

            entity.HasIndex(e => e.ExpireAt, "IX_HangFire_Set_ExpireAt").HasFilter("([ExpireAt] IS NOT NULL)");

            entity.HasIndex(e => new { e.Key, e.Score }, "IX_HangFire_Set_Score");

            entity.Property(e => e.Key).HasMaxLength(100);
            entity.Property(e => e.Value).HasMaxLength(256);
            entity.Property(e => e.ExpireAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<State>(entity =>
        {
            entity.HasKey(e => new { e.JobId, e.Id }).HasName("PK_HangFire_State");

            entity.ToTable("State", "HangFire");

            entity.HasIndex(e => e.CreatedAt, "IX_HangFire_State_CreatedAt");

            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(20);
            entity.Property(e => e.Reason).HasMaxLength(100);

            entity.HasOne(d => d.Job).WithMany(p => p.States)
                .HasForeignKey(d => d.JobId)
                .HasConstraintName("FK_HangFire_State_Job");
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__Status__C8EE206314BD3346");

            entity.ToTable("Status");

            entity.Property(e => e.StatusName).HasMaxLength(25);
        });

        modelBuilder.Entity<Subscribe>(entity =>
        {
            entity.HasKey(e => e.SubscribeId).HasName("PK__Subscrib__CAEA2952B710375C");

            entity.Property(e => e.SubscribeDate).HasColumnType("datetime");
            entity.Property(e => e.SubscribeEmail).HasMaxLength(100);
        });

        modelBuilder.Entity<TitleColor>(entity =>
        {
            entity.HasKey(e => e.TitleColorId).HasName("PK__TitleCol__03F01F8DBF4EEAB3");

            entity.ToTable("TitleColor");

            entity.Property(e => e.TitleColorId).HasColumnName("TitleColorID");
            entity.Property(e => e.TitleColorCode).HasMaxLength(20);
            entity.Property(e => e.TitleColorName).HasMaxLength(20);

            entity.HasOne(d => d.TitleColorNews).WithMany(p => p.TitleColors)
                .HasForeignKey(d => d.TitleColorNewsId)
                .HasConstraintName("FK__TitleColo__Title__797309D9");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CAADB4DA0");

            entity.Property(e => e.UserName).HasMaxLength(25);
            entity.Property(e => e.UserPassword)
                .HasMaxLength(60)
                .IsUnicode(false);

            entity.HasOne(d => d.UserStatus).WithMany(p => p.Users)
                .HasForeignKey(d => d.UserStatusId)
                .HasConstraintName("FK__Users__UserStatu__71D1E811");
        });

        modelBuilder.Entity<Xeberler>(entity =>
        {
            entity.HasKey(e => e.NewsId).HasName("PK__Xeberler__954EBDF33E50236A");

            entity.ToTable("Xeberler");

            entity.Property(e => e.NewsColor).HasMaxLength(10);
            entity.Property(e => e.NewsDate).HasColumnType("datetime");
            entity.Property(e => e.NewsLangSupport)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.NewsPhoto).HasMaxLength(255);
            entity.Property(e => e.NewsTitleAz).HasMaxLength(100);
            entity.Property(e => e.NewsTitleEn).HasMaxLength(100);
            entity.Property(e => e.XeberTwitter).HasMaxLength(500);
            entity.Property(e => e.XeberlerFutureDate).HasColumnType("datetime");
            entity.Property(e => e.XeberlerVideoUrl)
                .HasMaxLength(500)
                .HasColumnName("XeberlerVideoURL");

            entity.HasOne(d => d.XeberlerAdvantage).WithMany(p => p.Xeberlers)
                .HasForeignKey(d => d.XeberlerAdvantageId)
                .HasConstraintName("FK__Xeberler__Xeberl__38996AB5");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
