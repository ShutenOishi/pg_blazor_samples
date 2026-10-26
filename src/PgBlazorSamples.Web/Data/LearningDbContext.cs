using Microsoft.EntityFrameworkCore;

namespace PgBlazorSamples.Web.Data;

/// <summary>Webプロジェクト内に置く最小のEF Core 10 DbContext。</summary>
public sealed class LearningDbContext(DbContextOptions<LearningDbContext> options) : DbContext(options)
{
    public DbSet<LearningTask> Tasks => Set<LearningTask>();
    public DbSet<LearningCategory> Categories => Set<LearningCategory>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<AppRole> AppRoles => Set<AppRole>();
    public DbSet<AppPermission> AppPermissions => Set<AppPermission>();
    public DbSet<AppUserRole> AppUserRoles => Set<AppUserRole>();
    public DbSet<AppRolePermission> AppRolePermissions => Set<AppRolePermission>();

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

        // Step63〜65：ADのSIDをキーに、業務ロールと権限をDBで管理する。
        var user = modelBuilder.Entity<AppUser>();
        user.ToTable("app_user");
        user.HasKey(x => x.Id);
        user.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        user.Property(x => x.Sid).HasColumnName("sid").HasMaxLength(184).IsRequired();
        user.Property(x => x.DisplayName).HasColumnName("display_name").HasMaxLength(100).IsRequired();
        user.Property(x => x.IsActive).HasColumnName("is_active");
        user.HasIndex(x => x.Sid).IsUnique();

        var role = modelBuilder.Entity<AppRole>();
        role.ToTable("app_role");
        role.HasKey(x => x.Id);
        role.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        role.Property(x => x.Code).HasColumnName("code").HasMaxLength(80).IsRequired();
        role.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        role.Property(x => x.IsActive).HasColumnName("is_active");
        role.HasIndex(x => x.Code).IsUnique();

        var permission = modelBuilder.Entity<AppPermission>();
        permission.ToTable("app_permission");
        permission.HasKey(x => x.Id);
        permission.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();
        permission.Property(x => x.Code).HasColumnName("code").HasMaxLength(100).IsRequired();
        permission.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        permission.HasIndex(x => x.Code).IsUnique();

        var userRole = modelBuilder.Entity<AppUserRole>();
        userRole.ToTable("app_user_role");
        userRole.HasKey(x => new { x.UserId, x.RoleId });
        userRole.Property(x => x.UserId).HasColumnName("user_id");
        userRole.Property(x => x.RoleId).HasColumnName("role_id");
        userRole.HasOne<AppUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        userRole.HasOne<AppRole>().WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);

        var rolePermission = modelBuilder.Entity<AppRolePermission>();
        rolePermission.ToTable("app_role_permission");
        rolePermission.HasKey(x => new { x.RoleId, x.PermissionId });
        rolePermission.Property(x => x.RoleId).HasColumnName("role_id");
        rolePermission.Property(x => x.PermissionId).HasColumnName("permission_id");
        rolePermission.HasOne<AppRole>().WithMany().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
        rolePermission.HasOne<AppPermission>().WithMany().HasForeignKey(x => x.PermissionId).OnDelete(DeleteBehavior.Cascade);
    }
}
