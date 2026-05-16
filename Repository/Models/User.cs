using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public bool? IsActive { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual ICollection<ParkingHistory> ParkingHistories { get; set; } = new List<ParkingHistory>();

    public virtual ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}
