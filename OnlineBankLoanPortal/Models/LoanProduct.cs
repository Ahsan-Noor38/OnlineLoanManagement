namespace OnlineBankLoanPortal.Models;

public partial class LoanProduct
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public decimal InterestRate { get; set; }

    public string RepaymentTerms { get; set; } = null!;

    public virtual ICollection<LoanApplication> LoanApplications { get; set; } = new List<LoanApplication>();
}
