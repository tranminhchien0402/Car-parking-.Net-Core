using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Repository.Models;

public partial class ParkingManagementDbContext : DbContext
{
    public ParkingManagementDbContext()
    {
    }

    public ParkingManagementDbContext(DbContextOptions<ParkingManagementDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<MonthlyTicket> MonthlyTickets { get; set; }

    public virtual DbSet<ParkingHistory> ParkingHistories { get; set; }

    public virtual DbSet<ParkingLot> ParkingLots { get; set; }

    public virtual DbSet<Shift> Shifts { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vehicle> Vehicles { get; set; }

    public virtual DbSet<VehicleType> VehicleTypes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=.;Database=ParkingManagementDB;User Id=sa;Password=123;TrustServerCertificate=true;Trusted_Connection=SSPI;Encrypt=false;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK__Employee__7AD04FF1F3D43E91");

            entity.HasIndex(e => e.ShiftId, "IX_Employees_ShiftID");

            entity.HasIndex(e => e.UserId, "UQ__Employee__1788CCAD3A772005").IsUnique();

            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.ShiftId).HasColumnName("ShiftID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Shift).WithMany(p => p.Employees)
                .HasForeignKey(d => d.ShiftId)
                .HasConstraintName("FK__Employees__Shift__4222D4EF");

            entity.HasOne(d => d.User).WithOne(p => p.Employee)
                .HasForeignKey<Employee>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Employees__UserI__412EB0B6");
        });

        modelBuilder.Entity<MonthlyTicket>(entity =>
        {
            entity.HasKey(e => e.MonthlyTicketId).HasName("PK__MonthlyT__8C5C60E9BCE75D7D");

            entity.HasIndex(e => e.VehicleId, "IX_MonthlyTickets_VehicleID");

            entity.HasIndex(e => e.VehicleTypeId, "IX_MonthlyTickets_VehicleTypeID");

            entity.Property(e => e.MonthlyTicketId).HasColumnName("MonthlyTicketID");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.VehicleId).HasColumnName("VehicleID");
            entity.Property(e => e.VehicleTypeId).HasColumnName("VehicleTypeID");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.MonthlyTickets)
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MonthlyTi__Vehic__5812160E");

            entity.HasOne(d => d.VehicleType).WithMany(p => p.MonthlyTickets)
                .HasForeignKey(d => d.VehicleTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MonthlyTi__Vehic__59063A47");
        });

        modelBuilder.Entity<ParkingHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__ParkingH__4D7B4ADDBC2DD41D");

            entity.ToTable("ParkingHistory");

            entity.HasIndex(e => e.TicketId, "IX_ParkingHistory_TicketID");

            entity.HasIndex(e => e.UserId, "IX_ParkingHistory_UserID");

            entity.Property(e => e.HistoryId).HasColumnName("HistoryID");
            entity.Property(e => e.ActionTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ActionType).HasMaxLength(20);
            entity.Property(e => e.TicketId).HasColumnName("TicketID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Ticket).WithMany(p => p.ParkingHistories)
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("FK__ParkingHi__Ticke__5DCAEF64");

            entity.HasOne(d => d.User).WithMany(p => p.ParkingHistories)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__ParkingHi__UserI__5EBF139D");
        });

        modelBuilder.Entity<ParkingLot>(entity =>
        {
            entity.HasKey(e => e.ParkingLotId).HasName("PK__ParkingL__6F271EA989401089");

            entity.ToTable("ParkingLot");

            entity.Property(e => e.ParkingLotId).HasColumnName("ParkingLotID");
            entity.Property(e => e.ParkingLotName)
                .HasMaxLength(100)
                .HasDefaultValue("Bãi xe trung tâm");
        });

        modelBuilder.Entity<Shift>(entity =>
        {
            entity.HasKey(e => e.ShiftId).HasName("PK__Shifts__C0A838E152DFFBE2");

            entity.Property(e => e.ShiftId).HasColumnName("ShiftID");
            entity.Property(e => e.ShiftName).HasMaxLength(50);
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("PK__Tickets__712CC62771CCED67");

            entity.HasIndex(e => e.ParkingLotId, "IX_Tickets_ParkingLotID");

            entity.HasIndex(e => e.UserId, "IX_Tickets_UserID");

            entity.HasIndex(e => e.VehicleId, "IX_Tickets_VehicleID");

            entity.HasIndex(e => e.VehicleTypeId, "IX_Tickets_VehicleTypeID");

            entity.HasIndex(e => e.TicketCode, "UQ__Tickets__598CF7A3A39617DF").IsUnique();

            entity.Property(e => e.TicketId).HasColumnName("TicketID");
            entity.Property(e => e.CheckInTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CheckOutTime).HasColumnType("datetime");
            entity.Property(e => e.IsMonthlyTicket).HasDefaultValue(false);
            entity.Property(e => e.LicensePlate).HasMaxLength(20);
            entity.Property(e => e.ParkingLotId).HasColumnName("ParkingLotID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("CheckedIn");
            entity.Property(e => e.TicketCode).HasMaxLength(20);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.VehicleId).HasColumnName("VehicleID");
            entity.Property(e => e.VehicleTypeId).HasColumnName("VehicleTypeID");

            entity.HasOne(d => d.ParkingLot).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.ParkingLotId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tickets__Parking__534D60F1");

            entity.HasOne(d => d.User).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tickets__UserID__5441852A");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tickets__Vehicle__5165187F");

            entity.HasOne(d => d.VehicleType).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.VehicleTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tickets__Vehicle__52593CB8");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCACBBAC235B");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E455A6ACE5").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(100);
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValue("Employee");
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.VehicleId).HasName("PK__Vehicles__476B54B21684955C");

            entity.HasIndex(e => e.VehicleTypeId, "IX_Vehicles_VehicleTypeID");

            entity.Property(e => e.VehicleId).HasColumnName("VehicleID");
            entity.Property(e => e.LicensePlate).HasMaxLength(20);
            entity.Property(e => e.OwnerName).HasMaxLength(100);
            entity.Property(e => e.OwnerPhone).HasMaxLength(20);
            entity.Property(e => e.VehicleTypeId).HasColumnName("VehicleTypeID");

            entity.HasOne(d => d.VehicleType).WithMany(p => p.Vehicles)
                .HasForeignKey(d => d.VehicleTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Vehicles__Vehicl__46E78A0C");
        });

        modelBuilder.Entity<VehicleType>(entity =>
        {
            entity.HasKey(e => e.VehicleTypeId).HasName("PK__VehicleT__9F44962326566892");

            entity.Property(e => e.VehicleTypeId).HasColumnName("VehicleTypeID");
            entity.Property(e => e.DailyRate).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.HourlyRate).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.MonthlyRate).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TypeName).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
