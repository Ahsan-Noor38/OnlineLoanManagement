using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineBankLoanPortal.Models;

public partial class LoanApplication
{
    public int LoanApplicationId { get; set; }

    public int? ProductId { get; set; }

    public int? LoanTypeId { get; set; }
    public int? RepaymentTermId { get; set; }

    //public int? InterestRateId { get; set; }

    public Guid? AssignedLoanOfficerId { get; set; }

    public decimal AmountRequested { get; set; }

    public int Status { get; set; }

    public string? RejectionReason { get; set; }

    [Required]
    public DateTime DateApplied { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    public DateTime? ApprovedDate { get; set; }
    public string Purpose { get; set; }

    public DateTime? DisbursementDate { get; set; }

    public string? Feedback { get; set; }

    public Guid? CustomerId { get; set; }
    public Guid? ApprovedBy { get; set; }

    public string? ApplicationNotes { get; set; }
    public DateTime? CancelledDate { get; set; }
    public string? CancelledBy { get; set; }
    public string? CancellationReason { get; set; }


    public DateTime? ExpectedDisbursementDate { get; set; }
    public DateTime? ActualDisbursementDate { get; set; }
    public decimal? InterestRate { get; set; }
    public int? RepaymentTermMonths { get; set; }
    public decimal? MonthlyPayment { get; set; }
    public DateTime? LastRepaymentDate { get; set; }
    public decimal? OutstandingBalance { get; set; }
    public virtual LoanProduct? Product { get; set; }

    [ForeignKey("LoanTypeId")]
    public virtual LoanType? LoanType { get; set; }

    [ForeignKey("CustomerId")]
    public virtual ApplicationUser Customer { get; set; }
    [ForeignKey("AssignedLoanOfficerId")]
    public virtual ApplicationUser? LoanOfficer { get; set; }

    public virtual ICollection<Repayment> Repayments { get; set; }
    public virtual ICollection<LoanApplicationDocument> LoanApplicationDocuments { get; set; }
}
