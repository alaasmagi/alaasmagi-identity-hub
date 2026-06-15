using DTO.DataAccess.DTO;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Context;

public class AppDbContext : IdentityDbContext<AppUserEntity, AppRoleEntity, Guid>
{
    private const int DefaultNameMaxLength = 256;
    private const int HashMaxLength = 512;
    private const int UrlMaxLength = 2048;
    private const int SerializedListMaxLength = 4096;
    private const int ReasonMaxLength = 2048;
    private const int IpAddressMaxLength = 45;
    private const int UserAgentMaxLength = 512;

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ClientEntity> Clients { get; set; }
    public DbSet<AppUserClientEntity> AppUserClients { get; set; }
    public DbSet<SecurityEventEntity> SecurityEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUserClientEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.GrantedBy)
                .HasMaxLength(DefaultNameMaxLength);

            entity.Property(e => e.RevokedBy)
                .HasMaxLength(DefaultNameMaxLength);

            entity.Property(e => e.RevokeReason)
                .HasMaxLength(ReasonMaxLength);

            entity.Property(e => e.ConsentIp)
                .HasMaxLength(IpAddressMaxLength);

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserClients)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Client)
                .WithMany(c => c.UserClients)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.ClientId }).IsUnique();
        });

        modelBuilder.Entity<ClientEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .HasMaxLength(DefaultNameMaxLength);

            entity.Property(e => e.ClientSecretHash)
                .HasMaxLength(HashMaxLength);

            entity.Property(e => e.AllowedOrigins)
                .HasMaxLength(SerializedListMaxLength);

            entity.HasIndex(e => e.ClientId).IsUnique();
        });

        modelBuilder.Entity<AppUserEntity>(entity =>
        {
            entity.Property(e => e.FullName)
                .HasMaxLength(DefaultNameMaxLength);

            entity.Property(e => e.ProfilePictureUrl)
                .HasMaxLength(UrlMaxLength);

            entity.Property(e => e.BanReason)
                .HasMaxLength(ReasonMaxLength);
        });

        modelBuilder.Entity<AppRoleEntity>(entity =>
        {
            entity.HasOne(e => e.Client)
                .WithMany(c => c.Roles)
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SecurityEventEntity>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.IpAddress)
                .HasMaxLength(IpAddressMaxLength);

            entity.Property(e => e.UserAgent)
                .HasMaxLength(UserAgentMaxLength);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Client)
                .WithMany()
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
