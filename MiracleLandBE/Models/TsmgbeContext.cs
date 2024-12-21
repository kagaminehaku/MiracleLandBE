using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MiracleLandBE.Models

public partial class TsmgbeContext : DbContext
{
    public TsmgbeContext()
    {
    }

    public TsmgbeContext(DbContextOptions<TsmgbeContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AdminLog> AdminLogs { get; set; }

    public virtual DbSet<CsOrder> CsOrders { get; set; }

    public virtual DbSet<CsOrderDetail> CsOrderDetails { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ShoppingCart> ShoppingCarts { get; set; }

    public virtual DbSet<UserAccount> UserAccounts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=kagaminehaku.softether.net,25564;Database=TSMGBE;user id=sa;password=17102003;trust server certificate=true");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdminLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK_AdminLog");

            entity.ToTable("admin_log");

            entity.Property(e => e.LogId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("LogID");
            entity.Property(e => e.Action).HasMaxLength(255);
            entity.Property(e => e.ActionDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.AdminId)
                .HasMaxLength(50)
                .HasColumnName("AdminID");
        });

        modelBuilder.Entity<CsOrder>(entity =>
        {
            entity.HasKey(e => e.Orderid).HasName("PK_order_1");

            entity.ToTable("cs_order");

            entity.Property(e => e.Orderid)
                .ValueGeneratedNever()
                .HasColumnName("orderid");
            entity.Property(e => e.IsPayment).HasColumnName("is_payment");
            entity.Property(e => e.ShipId)
                .HasMaxLength(16)
                .HasColumnName("ship_id");
            entity.Property(e => e.Total)
                .HasColumnType("decimal(19, 4)")
                .HasColumnName("total");
            entity.Property(e => e.Uid).HasColumnName("uid");
        });

        modelBuilder.Entity<CsOrderDetail>(entity =>
        {
            entity.HasKey(e => e.Odid).HasName("PK_order_detail_1");

            entity.ToTable("cs_order_detail");

            entity.Property(e => e.Odid)
                .ValueGeneratedNever()
                .HasColumnName("odid");
            entity.Property(e => e.Orderid).HasColumnName("orderid");
            entity.Property(e => e.Pid).HasColumnName("pid");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.Order).WithMany(p => p.CsOrderDetails)
                .HasForeignKey(d => d.Orderid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_order_detail_order");

            entity.HasOne(d => d.OrderNavigation).WithMany(p => p.CsOrderDetails)
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
            entity.Property(e => e.Pprice)
                .HasColumnType("decimal(19, 4)")
                .HasColumnName("pprice");
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
            entity.Property(e => e.Avt)
                .HasColumnType("text")
                .HasColumnName("avt");
            entity.Property(e => e.Email)
                .HasMaxLength(128)
                .HasColumnName("email");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
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
