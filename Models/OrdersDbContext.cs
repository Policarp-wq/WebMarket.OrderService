using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace WebMarket.OrderService.Models;

public partial class OrdersDbContext : DbContext
{
    public OrdersDbContext()
    {
    }

    public OrdersDbContext(DbContextOptions<OrdersDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Checkpoint> Checkpoints { get; set; }

    public virtual DbSet<OrderStatusStory> CustomerHistories { get; set; }

    public virtual DbSet<CustomerOrder> CustomerOrders { get; set; }
    public virtual DbSet<OrderTrace> OrderTraceses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            //.HasPostgresEnum("order_status", new[] { "processing", "packing_up", "delivering", "delivered", "completed", "denied" })
            .HasPostgresExtension("fuzzystrmatch")
            .HasPostgresExtension("postgis")
            .HasPostgresExtension("tiger", "postgis_tiger_geocoder")
            .HasPostgresExtension("topology", "postgis_topology");

        modelBuilder.Entity<OrderTrace>(entity =>
        {
            entity.HasKey(o => o.TraceId);

            entity.ToTable("order_trace");

            entity.Property(o => o.OrderId)
                .HasColumnName("order_id");

            entity.Property(o => o.CheckpointId)
                .HasColumnName("checkpoint_id");
            entity.Property(o => o.DeliveryStatus)
                .HasColumnName("delivery_status");

            entity.Property(o => o.DeliveryDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("delivery_date");

            entity.HasOne<CustomerOrder>(o => o.CustomerOrder).WithMany(c => c.OrderTraces);
        });

        modelBuilder.Entity<Checkpoint>(entity =>
        {
            entity.HasKey(e => e.CheckpointId).HasName("checkpoint_pkey");

            entity.ToTable("checkpoint");

            entity.HasIndex(e => e.OwnerId, "checkpoint_owner_id_key").IsUnique();

            entity.Property(e => e.CheckpointId).HasColumnName("checkpoint_id");
            entity.Property(e => e.IsDeliveryPoint).HasColumnName("is_delivery_point");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.Location).HasColumnName("location").HasColumnType("geometry (point)");
            entity.Property(e => e.Address).HasColumnName("address");
        });

        modelBuilder.Entity<OrderStatusStory>(entity =>
        {
            entity.HasKey(e => e.StoryId).HasName("story_id_pkey");//!

            entity.ToTable("customer_history");

            entity.Property(e => e.StoryId).HasColumnName("story_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ChangeDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("change_date");
            entity.Property(e => e.Status)
                .HasColumnName("order_status")
                .HasConversion(s => (int)s,
                    v => (OrderStatus)Enum.Parse(typeof(OrderStatus), v.ToString()));
        });

        modelBuilder.Entity<CustomerOrder>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("customer_order_pkey");

            entity.ToTable("customer_order");

            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.DeliveryPointId).HasColumnName("delivery_point_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Status)
            .HasColumnName("status")
            .HasConversion(s => (int)s,
                    v => (OrderStatus)Enum.Parse(typeof(OrderStatus), v.ToString())); ;
            //.HasConversion(v => v.ToString().ToLower(), v => (CustomerOrder.OrderStatus)Enum.Parse(typeof(CustomerOrder.OrderStatus), v))
            //.HasColumnType("order_status");

            entity.Property(e => e.TrackNumber).HasColumnName("track_number");

            entity.HasOne(d => d.DeliveryPoint).WithMany(p => p.CustomerOrderDeliveryPoints)
                .HasForeignKey(d => d.DeliveryPointId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("customer_order_delivery_point_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
