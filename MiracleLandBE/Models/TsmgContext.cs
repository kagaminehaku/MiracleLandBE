using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MiracleLandBE.Models;

public partial class TsmgContext : DbContext
{
    public TsmgContext()
    {
    }

    public TsmgContext(DbContextOptions<TsmgContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ShoppingCart> ShoppingCarts { get; set; }

    public virtual DbSet<UserAccount> UserAccounts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=kagaminehaku.softether.net,25565;Database=TSMG;user id=sa;password=17102003;trust server certificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Orderid).HasName("PK_order_1");

            entity.ToTable("order");

            entity.Property(e => e.Orderid)
                .ValueGeneratedNever()
                .HasColumnName("orderid");
            entity.Property(e => e.Total).HasColumnName("total");
            entity.Property(e => e.Uid).HasColumnName("uid");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.Odid).HasName("PK_order_detail_1");

            entity.ToTable("order_detail");

            entity.Property(e => e.Odid)
                .ValueGeneratedNever()
                .HasColumnName("odid");
            entity.Property(e => e.Orderid).HasColumnName("orderid");
            entity.Property(e => e.Pid).HasColumnName("pid");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.Orderid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_order_detail_order");

            entity.HasOne(d => d.OrderNavigation).WithMany(p => p.OrderDetails)
                .HasForeignKey(d => d.Orderid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_order_detail_product");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Pid).HasName("PK_product_1");

            entity.ToTable("product");

            entity.Property(e => e.Pid)
                .ValueGeneratedNever()
                .HasColumnName("pid");
            entity.Property(e => e.Pimg)
                .HasMaxLength(1024)
                .HasColumnName("pimg");
            entity.Property(e => e.Pinfo)
                .HasMaxLength(256)
                .HasColumnName("pinfo");
            entity.Property(e => e.Pname)
                .HasMaxLength(64)
                .HasColumnName("pname");
            entity.Property(e => e.Pprice).HasColumnName("pprice");
            entity.Property(e => e.Pquantity).HasColumnName("pquantity");
        });

        modelBuilder.Entity<ShoppingCart>(entity =>
        {
            entity.HasKey(e => e.Cartitemid).HasName("PK_shopping_cart_1");

            entity.ToTable("shopping_cart");

            entity.Property(e => e.Cartitemid)
                .ValueGeneratedNever()
                .HasColumnName("cartitemid");
            entity.Property(e => e.Pid).HasColumnName("pid");
            entity.Property(e => e.Pquantity).HasColumnName("pquantity");
            entity.Property(e => e.Uid).HasColumnName("uid");

            entity.HasOne(d => d.PidNavigation).WithMany(p => p.ShoppingCarts)
                .HasForeignKey(d => d.Pid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_shopping_cart_product");

            entity.HasOne(d => d.Pid1).WithMany(p => p.ShoppingCarts)
                .HasForeignKey(d => d.Pid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_shopping_cart_user_account");
        });

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(e => e.Uid).HasName("PK_user_account_1");

            entity.ToTable("user_account");

            entity.Property(e => e.Uid)
                .ValueGeneratedNever()
                .HasColumnName("uid");
            entity.Property(e => e.Address)
                .HasMaxLength(128)
                .HasColumnName("address");
            entity.Property(e => e.Email)
                .HasMaxLength(128)
                .HasColumnName("email");
            entity.Property(e => e.Password)
                .HasMaxLength(128)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(16)
                .HasColumnName("phone");
            entity.Property(e => e.Type)
                .HasMaxLength(16)
                .HasColumnName("type");
            entity.Property(e => e.Username)
                .HasMaxLength(128)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
