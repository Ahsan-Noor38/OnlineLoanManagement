using OnlineBankLoanPortal.Models;
using System.ComponentModel.DataAnnotations;

namespace OnlineBankLoanPortal.ViewModels
{
    public class LoanApplicationVM
    {
        public int? Id { get; set; }
        public string Address { get; set; }
        public double TotalIncome { get; set; }
        [EmailAddress]
        public string ApplicantEmail { get; set; }
        [Required]
        public int LoanTypeId { get; set; }
        public string LoanTypeName { get; set; } // Display Name
        //public int ProductId { get; set; }
        //public string? ProductName { get; set; }
        public string Purpose { get; set; }

        [Required]
        [Display(Name = "Amount Requested")]
        //[Range(1000, 100000, ErrorMessage = "Amount must be between $1,000 and $100,000.")]
        public decimal AmountRequested { get; set; }

        public Guid? UserId { get; set; }
        public string CustomerName { get; set; }

        [Required]
        public LoanApplicationStatus Status { get; set; }

        public DateTime DateApplied { get; set; } = DateTime.Now;
        public DateTime? ApprovedDate { get; set; }
        public Guid ApprovedBy { get; set; }
        public string ApplicationNotes { get; set; }
        public DateTime? CancelledDate { get; set; }
        public Guid? CancelledBy { get; set; }
        public string CancellationReason { get; set; }
        public Guid? AssignedOfficerId { get; set; }
        public string? OfficerName { get; set; }

        public DateTime? ExpectedDisbursementDate { get; set; }
        public DateTime? ActualDisbursementDate { get; set; }
        public decimal? InterestRate { get; set; } // Store the applicable rate at approval
        public int? RepaymentTermMonths { get; set; } // Store the applicable term at approval
        public decimal? MonthlyPayment { get; set; }
        public DateTime? LastRepaymentDate { get; set; }
        public decimal? OutstandingBalance { get; set; }
        public ICollection<LoanApplicationDocument> LoanApplicationDocuments { get; internal set; }
    }
}
