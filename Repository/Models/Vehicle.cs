using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class Vehicle
{
    public int VehicleId { get; set; }

    public string LicensePlate { get; set; } = null!;

    public int VehicleTypeId { get; set; }

    public string? OwnerName { get; set; }

    public string? OwnerPhone { get; set; }

    public virtual ICollection<MonthlyTicket> MonthlyTickets { get; set; } = new List<MonthlyTicket>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual VehicleType VehicleType { get; set; } = null!;
}
