using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class ParkingHistory
{
    public int HistoryId { get; set; }

    public int? TicketId { get; set; }

    public string? ActionType { get; set; }

    public DateTime? ActionTime { get; set; }

    public int? UserId { get; set; }

    public virtual Ticket? Ticket { get; set; }

    public virtual User? User { get; set; }
}
