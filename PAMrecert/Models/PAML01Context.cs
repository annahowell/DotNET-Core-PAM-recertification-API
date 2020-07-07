using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace PAMrecert.Models
{
    public partial class PAML01Context : DbContext
    {
        public PAML01Context()
        {
        }

        public PAML01Context(DbContextOptions<PAML01Context> options)
            : base(options)
        {
        }

        public virtual DbSet<PrivTable> PrivTable { get; set; }
        public virtual DbSet<RecertCycleTable> RecertCycleTable { get; set; }
        public virtual DbSet<RolePrivLink> RolePrivLink { get; set; }
        public virtual DbSet<RoleTable> RoleTable { get; set; }
        public virtual DbSet<ServiceTable> ServiceTable { get; set; }
        public virtual DbSet<UserTable> UserTable { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:DefaultSchema", "db_owner");

            modelBuilder.Entity<PrivTable>(entity =>
            {
                entity.HasKey(e => e.PrivId)
                    .HasName("PK_PRIVTABLE");

                entity.ToTable("PrivTable", "dbo");

                entity.HasIndex(e => e.PrivId)
                    .HasName("UQ__PrivTabl__F0495AFA8C028FA1")
                    .IsUnique();

                entity.Property(e => e.PrivId).HasMaxLength(255);

                entity.Property(e => e.CredentialStorageMethod).HasMaxLength(255);

                entity.Property(e => e.PermissionGroup)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.ServiceId)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.ServicePrivSummary).IsRequired();

                entity.HasOne(d => d.Service)
                    .WithMany(p => p.PrivTable)
                    .HasForeignKey(d => d.ServiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("PrivTable_fk0");
            });

            modelBuilder.Entity<RecertCycleTable>(entity =>
            {
                entity.HasKey(e => e.RecertCycleId)
                    .HasName("PK_RECERTCYCLETABLE");

                entity.ToTable("RecertCycleTable", "dbo");

                entity.Property(e => e.RecertCycleTitle)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<RolePrivLink>(entity =>
            {
                entity.HasKey(e => e.RolePrivId)
                    .HasName("PK_ROLEPRIVLINK");

                entity.ToTable("RolePrivLink", "dbo");

                entity.HasIndex(e => new { e.RoleId, e.RoleOwner_PrivId })
                    .HasName("UQ_RolePrivLink01")
                    .IsUnique();

                entity.HasIndex(e => new { e.RoleId, e.ServiceOwner_PrivId })
                    .HasName("UQ_RolePrivLink02")
                    .IsUnique();

                entity.Property(e => e.RoleOwner_PrivId)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.RoleId)
                    .IsRequired()
                    .HasMaxLength(225);

                entity.Property(e => e.ServiceOwner_PrivId)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasOne(d => d.RoleOwner_Priv)
                    .WithMany(p => p.RolePrivLink)
                    .HasForeignKey(d => d.RoleOwner_PrivId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("RolePrivLink_fk1");

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.RolePrivLink)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("RolePrivLink_fk0");
            });

            modelBuilder.Entity<RoleTable>(entity =>
            {
                entity.HasKey(e => e.RoleId)
                    .HasName("PK_ROLETABLE");

                entity.ToTable("RoleTable", "dbo");

                entity.HasIndex(e => e.RoleId)
                    .HasName("UQ__RoleTabl__8AFACE1B510FFB53")
                    .IsUnique();

                entity.Property(e => e.RoleId).HasMaxLength(225);

                entity.Property(e => e.RoleName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.RoleOwner_RoleId).HasMaxLength(225);
            });

            modelBuilder.Entity<ServiceTable>(entity =>
            {
                entity.HasKey(e => e.ServiceId)
                    .HasName("PK_SERVICETABLE");

                entity.ToTable("ServiceTable", "dbo");

                entity.HasIndex(e => e.ServiceId)
                    .HasName("UQ__ServiceT__C51BB00B7EA1F166")
                    .IsUnique();

                entity.Property(e => e.ServiceId).HasMaxLength(255);

                entity.Property(e => e.ServiceName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.ServiceOwner_RoleId).HasMaxLength(225);

                entity.HasOne(d => d.ServiceOwner_Role)
                    .WithMany(p => p.ServiceTable)
                    .HasForeignKey(d => d.ServiceOwner_RoleId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("ServiceTable_fk0");
            });

            modelBuilder.Entity<UserTable>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("PK_USERTABLE");

                entity.ToTable("UserTable", "dbo");

                entity.HasIndex(e => e.UserId)
                    .HasName("UQ__UserTabl__1788CC4D2C1717CD")
                    .IsUnique();

                entity.Property(e => e.UserId).HasMaxLength(225);

                entity.Property(e => e.LastCertifiedBy).HasMaxLength(225);

                entity.Property(e => e.RoleId).HasMaxLength(225);

                entity.Property(e => e.UserFullName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.UserTable)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("UserTable_fk0");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
