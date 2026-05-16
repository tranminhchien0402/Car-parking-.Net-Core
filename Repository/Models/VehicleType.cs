using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class VehicleType
{
    public int VehicleTypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public decimal HourlyRate { get; set; }

    public decimal DailyRate { get; set; }

    public decimal MonthlyRate { get; set; }

    public virtual ICollection<MonthlyTicket> MonthlyTickets { get; set; } = new List<MonthlyTicket>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
