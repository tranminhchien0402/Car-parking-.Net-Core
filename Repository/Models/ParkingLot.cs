using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class ParkingLot
{
    public int ParkingLotId { get; set; }

    public string ParkingLotName { get; set; } = null!;

    public int TotalSlots { get; set; }

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
