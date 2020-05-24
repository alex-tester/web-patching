using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace wsPatching.Models.DatabaseModels
{
    public partial class PatchingAutomationContext : DbContext
    {
        public PatchingAutomationContext()
        {
        }

        public PatchingAutomationContext(DbContextOptions<PatchingAutomationContext> options)
            : base(options)
        {
        }

        public virtual DbSet<PatchingAvailablePatches> PatchingAvailablePatches { get; set; }
        public virtual DbSet<PatchingConfig> PatchingConfig { get; set; }
        public virtual DbSet<PatchingExecution> PatchingExecution { get; set; }
        public virtual DbSet<PatchingResults> PatchingResults { get; set; }
        public virtual DbSet<PatchingSource> PatchingSource { get; set; }
        public virtual DbSet<ServerObject> ServerObject { get; set; }

        //        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //        {
        //            if (!optionsBuilder.IsConfigured)
        //            {
        //#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
        //                optionsBuilder.UseSqlServer("Server=SQL03.observicing.net;Database=PatchingAutomation;Trusted_Connection=true;");
        //            }
        //        }

        public static string ConnectionString { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PatchingAvailablePatches>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.KbNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedOn).HasColumnType("datetime");

                entity.Property(e => e.Title)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.HasOne(d => d.PatchingExecution)
                    .WithMany(p => p.PatchingAvailablePatches)
                    .HasForeignKey(d => d.PatchingExecutionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PatchingAvailablePatches_PatchingExecution");

                entity.HasOne(d => d.Server)
                    .WithMany(p => p.PatchingAvailablePatches)
                    .HasForeignKey(d => d.ServerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PatchingAvailablePatches_ServerObject");
            });

            modelBuilder.Entity<PatchingConfig>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.EndTime).HasColumnType("datetime");

                entity.Property(e => e.ModifiedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedOn).HasColumnType("datetime");

                entity.Property(e => e.PatchingName)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.Property(e => e.PostPatchScript).IsUnicode(false);

                entity.Property(e => e.PrePatchScript).IsUnicode(false);

                entity.Property(e => e.StartTime).HasColumnType("datetime");

                entity.Property(e => e.TelerikRecurrenceRule)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateVmwareTools).HasColumnName("UpdateVMwareTools");

                entity.HasOne(d => d.PatchingSource)
                    .WithMany(p => p.PatchingConfig)
                    .HasForeignKey(d => d.PatchingSourceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PatchingConfig_PatchingSource");

                entity.HasOne(d => d.Server)
                    .WithMany(p => p.PatchingConfig)
                    .HasForeignKey(d => d.ServerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PatchingConfig_ServerObject");
            });

            modelBuilder.Entity<PatchingExecution>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.ModifiedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            });

            modelBuilder.Entity<PatchingResults>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.KbNumber)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedOn).HasColumnType("datetime");

                entity.Property(e => e.Status)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Title)
                    .HasMaxLength(200)
                    .IsUnicode(false);

                entity.HasOne(d => d.PatchingExecution)
                    .WithMany(p => p.PatchingResults)
                    .HasForeignKey(d => d.PatchingExecutionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PatchingResults_PatchingExecution");

                entity.HasOne(d => d.Server)
                    .WithMany(p => p.PatchingResults)
                    .HasForeignKey(d => d.ServerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_PatchingResults_ServerObject");
            });

            modelBuilder.Entity<PatchingSource>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.ModifiedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedOn).HasColumnType("datetime");

                entity.Property(e => e.PatchingSource1)
                    .IsRequired()
                    .HasColumnName("PatchingSource")
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<ServerObject>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedOn).HasColumnType("datetime");

                entity.Property(e => e.Hostname)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.ModifiedOn).HasColumnType("datetime");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
