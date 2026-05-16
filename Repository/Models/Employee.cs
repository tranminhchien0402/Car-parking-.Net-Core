using System;
using System.Collections.Generic;

namespace Repository.Models;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Gender { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public int? ShiftId { get; set; }

    public virtual Shift? Shift { get; set; }

    public virtual User User { get; set; } = null!;
}
