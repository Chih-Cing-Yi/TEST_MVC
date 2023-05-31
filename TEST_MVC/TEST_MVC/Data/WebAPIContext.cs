using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TEST_MVC.Models;

namespace TEST_MVC.Data;

public partial class WebAPIContext : DbContext
{
    public WebAPIContext(DbContextOptions<WebAPIContext> options)
        : base(options)
    {
    }
   
    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Products");

            entity.ToTable("Product");

            entity.Property(e => e.CreatDate).HasColumnType("datetime");
            entity.Property(e => e.EditDate).HasColumnType("datetime");
            entity.Property(e => e.ImageUrl).HasMaxLength(250);
            entity.Property(e => e.Name).HasMaxLength(25);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(25);
            entity.Property(e => e.PassWord)
                .IsRequired()
                .HasMaxLength(25);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
