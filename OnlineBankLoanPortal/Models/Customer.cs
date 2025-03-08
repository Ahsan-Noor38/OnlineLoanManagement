using System;
using System.Collections.Generic;

namespace OnlineBankLoanPortal.Models;

public partial class Customer
{
    public int CId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Address { get; set; } = null!;
}
