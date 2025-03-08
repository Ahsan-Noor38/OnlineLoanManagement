using System;
using System.Collections.Generic;

namespace OnlineBankLoanPortal.Models;

public partial class LoanApplication
{
    public int LaId { get; set; }

    public string Date { get; set; } = null!;

    public string Amount { get; set; } = null!;

    public int CId { get; set; }

    public int LpId { get; set; }

    public int LoId { get; set; }

    public string ApplicantName { get; set; } = null!;

    public string ApplicantEmail { get; set; } = null!;

    public string LoanProduct { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string TotalIncome { get; set; } = null!;

    public virtual LoanOfficer Lo { get; set; } = null!;
}
