using Microsoft.EntityFrameworkCore;

namespace PgBlazorSamples.Web.Data;

/// <summary>Webプロジェクト内に置く最小のEF Core 10 DbContext。</summary>
public sealed class LearningDbContext(DbContextOptions<LearningDbContext> options) : DbContext(options)
{
    public DbSet<LearningTask> Tasks => Set<LearningTask>();
    public DbSet<LearningCategory> Categories => Set<LearningCategory>();
    public DbSet<ExistingUserAccessRow> ExistingUserAccess => Set<ExistingUserAccessRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var task = modelBuilder.Entity<LearningTask>();
        task.ToTable("learning_task");
        task.HasKey(x => x.Id);
        task.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        task.Property(x => x.Title).HasColumnName("title").HasMaxLength(100).IsRequired();
        task.Property(x => x.DueDate).HasColumnName("due_date").HasColumnType("date");
        task.Property(x => x.StartTime).HasColumnName("start_time").HasColumnType("time without time zone");
        // daterangeはNpgsqlRange<DateTime>として保存する。DateOnlyとは別の型。
        task.Property(x => x.AvailableDates).HasColumnName("available_dates").HasColumnType("daterange");
        task.Property(x => x.IsDone).HasColumnName("is_done");
        task.Property(x => x.Version).IsRowVersion(); // PostgreSQLのxminにマッピング
        task.Property(x => x.CategoryId).HasColumnName("category_id");
        task.HasOne(x => x.Category).WithMany().HasForeignKey(x => x.CategoryId);
        task.HasIndex(x => x.DueDate);
        var category = modelBuilder.Entity<LearningCategory>();
        category.ToTable("learning_category");
        category.HasKey(x => x.Id);
        category.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        category.Property(x => x.Name).HasColumnName("name").HasMaxLength(60).IsRequired();
        category.HasIndex(x => x.Name).IsUnique();

        // 既存台帳の統一ビュー。新規テーブルを業務アプリから作成・更新しない。
        var externalAccess = modelBuilder.Entity<ExistingUserAccessRow>();
        externalAccess.HasNoKey();
        externalAccess.ToView("learning_user_access_v");
        externalAccess.Property(x => x.LoginName).HasColumnName("login_name");
        externalAccess.Property(x => x.DisplayName).HasColumnName("display_name");
        externalAccess.Property(x => x.IsActive).HasColumnName("is_active");
        externalAccess.Property(x => x.CanRead).HasColumnName("can_read");
        externalAccess.Property(x => x.CanEdit).HasColumnName("can_edit");
        externalAccess.Property(x => x.UnitCode).HasColumnName("unit_code");
        externalAccess.Property(x => x.AuthorizationVersion).HasColumnName("authorization_version");
    }
}
