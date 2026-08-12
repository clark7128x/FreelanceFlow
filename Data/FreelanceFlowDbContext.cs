using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using FreelanceFlow.Models;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace FreelanceFlow.Data
{
    public class FreelanceFlowDbContext : DbContext
    {
        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<TimeLog> TimeLogs { get; set; } = null!;

        private readonly string _dbPath;

        public FreelanceFlowDbContext()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appDataPath, "FreelanceFlow");
            Directory.CreateDirectory(appFolder);
            _dbPath = Path.Combine(appFolder, "freelanceflow.db");
        }

        public FreelanceFlowDbContext(DbContextOptions<FreelanceFlowDbContext> options)
            : base(options)
        {
            _dbPath = "freelanceflow.db";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite($"Data Source={_dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasIndex(c => c.Name);
            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasIndex(p => p.Name);
                entity.HasIndex(p => p.Status);

                entity.HasOne(p => p.Client)
                    .WithMany(c => c.Projects)
                    .HasForeignKey(p => p.ClientId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TimeLog>(entity =>
            {
                entity.HasIndex(t => t.StartTime);
                entity.HasIndex(t => new { t.ProjectId, t.StartTime });

                entity.HasOne(t => t.Project)
                    .WithMany(p => p.TimeLogs)
                    .HasForeignKey(t => t.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>().HasData(
                new Client
                {
                    Id = 1,
                    Name = "Acme Corporation",
                    Email = "contact@acme.com",
                    DefaultHourlyRate = 75.00m,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                },
                new Client
                {
                    Id = 2,
                    Name = "TechStart Inc",
                    Email = "hello@techstart.io",
                    DefaultHourlyRate = 95.00m,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                }
            );

            modelBuilder.Entity<Project>().HasData(
                new Project
                {
                    Id = 1,
                    Name = "Website Redesign",
                    Description = "Complete overhaul of company website",
                    HourlyRate = 75.00m,
                    ClientId = 1,
                    CreatedAt = DateTime.Now,
                    Status = ProjectStatus.Active,
                    IsActive = true
                },
                new Project
                {
                    Id = 2,
                    Name = "Mobile App Development",
                    Description = "iOS and Android app for customer portal",
                    HourlyRate = 95.00m,
                    ClientId = 2,
                    CreatedAt = DateTime.Now,
                    Status = ProjectStatus.Active,
                    IsActive = true
                }
            );
        }
    }
}