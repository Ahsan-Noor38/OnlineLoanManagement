namespace OnlineBankLoanPortal.Models;

public partial class LoanApplication
{
    public int ApplicationId { get; set; }

    public int? ProductId { get; set; }

    public decimal AmountRequested { get; set; }

    public string Status { get; set; } = null!;

    public DateTime DateApplied { get; set; }

    public DateTime? DateApproved { get; set; }

    public string? Feedback { get; set; }

    public Guid? UserId { get; set; }

    public virtual LoanProduct? Product { get; set; }

    public virtual ApplicationUser? ApplicationUser { get; set; }

    public virtual ICollection<Repayment> Repayments { get; set; }
}
