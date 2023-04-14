﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace CameraAPI.Models
{
    public partial class CameraAPIdbContext : DbContext
    {
        public CameraAPIdbContext()
        {
        }

        public CameraAPIdbContext(DbContextOptions<CameraAPIdbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Camera> Cameras { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            object value = optionsBuilder.UseSqlServer("Server=INTERN-TMPHUC1\\SQLEXPRESS;Database=InternShop;uid=minhphuc;password=minhphuc0159@;Trusted_Connection=True;encrypt=false;",
                sqlServerOptions => sqlServerOptions.EnableRetryOnFailure());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            

            modelBuilder.Entity<Camera>(entity =>
            {
                entity.ToTable("Camera");

                entity.HasIndex(e => e.Name, "UQ__Camera__737584F61827E239")
                    .IsUnique();

                entity.Property(e => e.CameraId).HasColumnName("CameraID");

                entity.Property(e => e.Brand).HasMaxLength(255);

                entity.Property(e => e.CategoryId).HasColumnName("CategoryID");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.Img).HasMaxLength(255);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
                entity.Property(e => e.IsDelete).HasColumnName("IsDelete");
                entity.Property(e => e.Sold).HasColumnName("Sold");
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Category");

                entity.HasIndex(e => e.Name, "UQ__Category__737584F6DE378F51")
                    .IsUnique();

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Order");

                entity.Property(e => e.Address).HasMaxLength(255);

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Payment).HasMaxLength(255);

                entity.Property(e => e.Price).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.Status).HasMaxLength(100);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.Username).HasMaxLength(255);
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("OrderDetail");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.Status).HasMaxLength(100);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.HasIndex(e => e.Username, "UQ__User__536C85E406C88EEC")
                    .IsUnique();

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.CreatedDate).HasColumnType("datetime");

                entity.Property(e => e.Email).HasMaxLength(255);

                entity.Property(e => e.Password).HasMaxLength(255);

                entity.Property(e => e.PhoneNumber)
                    .HasMaxLength(16)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.Role).HasMaxLength(255);

                entity.Property(e => e.Status).HasMaxLength(100);

                entity.Property(e => e.UpdatedDate).HasColumnType("datetime");

                entity.Property(e => e.Username)
                    .IsRequired()
                    .HasMaxLength(255);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}