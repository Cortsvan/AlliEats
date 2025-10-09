using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using ASI.Basecode.Data.Models;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ASI.Basecode.Data
{
    public partial class AsiBasecodeDBContext : DbContext
    {
        public AsiBasecodeDBContext()
        {
        }

        public AsiBasecodeDBContext(DbContextOptions<AsiBasecodeDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<MenuItem> MenuItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Only configure if not already configured (for design-time scenarios)
            if (!optionsBuilder.IsConfigured)
            {
                // Try to find and load configuration for design-time scenarios
                var configuration = GetDesignTimeConfiguration();
                if (configuration != null)
                {
                    var connectionString = configuration.GetConnectionString("DefaultConnection");
                    if (!string.IsNullOrEmpty(connectionString))
                    {
                        optionsBuilder.UseSqlServer(connectionString);
                    }
                }
            }
        }

        private IConfiguration GetDesignTimeConfiguration()
        {
            try
            {
                // Try multiple potential paths for the configuration file
                var basePaths = new[]
                {
                    Path.Combine(Directory.GetCurrentDirectory(), "..", "ASI.Basecode.WebApp"),
                    Path.Combine(Directory.GetCurrentDirectory(), "ASI.Basecode.WebApp"),
                    Directory.GetCurrentDirectory()
                };

                foreach (var basePath in basePaths)
                {
                    var configPath = Path.Combine(basePath, "appsettings.json");
                    if (File.Exists(configPath))
                    {
                        return new ConfigurationBuilder()
                            .SetBasePath(basePath)
                            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                            .Build();
                    }
                }
            }
            catch
            {
                // Ignore errors and return null
            }
            
            return null;
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.UserId, "UQ__Users__1788CC4D5F4A160F")
                    .IsUnique();

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Role)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasDefaultValue("User");

                entity.Property(e => e.UpdatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<MenuItem>(entity =>
            {
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.Price)
                    .IsRequired()
                    .HasColumnType("decimal(10,2)");

                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedTime).HasColumnType("datetime");

                entity.Property(e => e.UpdatedBy)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdatedTime).HasColumnType("datetime");

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
