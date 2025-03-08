using System;
using System.Collections.Generic;

namespace OnlineBankLoanPortal.Models;

public partial class LoanOfficer
{
    public int LoId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public virtual ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();
}
