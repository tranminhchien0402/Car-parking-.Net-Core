using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class Ticket
{
    public int TicketId { get; set; }

    public string TicketCode { get; set; } = null!;

    public int VehicleId { get; set; }

    public string LicensePlate { get; set; } = null!;

    public int VehicleTypeId { get; set; }

    public int ParkingLotId { get; set; }

    public int UserId { get; set; }

    public DateTime CheckInTime { get; set; }

    public DateTime? CheckOutTime { get; set; }

    public decimal? TotalAmount { get; set; }

    public bool? IsMonthlyTicket { get; set; }

    public string? Status { get; set; }

    public virtual ICollection<ParkingHistory> ParkingHistories { get; set; } = new List<ParkingHistory>();

    public virtual ParkingLot ParkingLot { get; set; } = null!;

    public virtual User User { get; set; } = null!;

    public virtual Vehicle Vehicle { get; set; } = null!;

    public virtual VehicleType VehicleType { get; set; } = null!;
}
