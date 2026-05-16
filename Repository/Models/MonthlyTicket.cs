using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class MonthlyTicket
{
    public int MonthlyTicketId { get; set; }

    public int VehicleId { get; set; }

    public int VehicleTypeId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public decimal Price { get; set; }

    public bool? IsActive { get; set; }

    public virtual Vehicle Vehicle { get; set; } = null!;

    public virtual VehicleType VehicleType { get; set; } = null!;
}
