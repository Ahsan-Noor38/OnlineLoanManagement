namespace OnlineBankLoanPortal.Models;

public partial class Repayment
{
    public int? RepaymentId { get; set; }

    public int ApplicationId { get; set; }

    public decimal AmountPaid { get; set; }

    public DateTime PaymentDate { get; set; }

    public string? ReceiptUpload { get; set; }

    public virtual LoanApplication? Application { get; set; }
}
