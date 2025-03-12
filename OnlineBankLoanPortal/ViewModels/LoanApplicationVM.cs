using System.ComponentModel.DataAnnotations;

namespace OnlineBankLoanPortal.ViewModels
{
    public class LoanApplicationVM
    {
        public string ApplicantName { get; set; }
        public string Address { get; set; }
        public double TotalIncome { get; set; }
        [EmailAddress]
        public string ApplicantEmail { get; set; }
        public int? Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }

        [Required]
        [Display(Name = "Amount Requested")]
        //[Range(1000, 100000, ErrorMessage = "Amount must be between $1,000 and $100,000.")]
        public decimal AmountRequested { get; set; }

        public Guid? UserId { get; set; } // Foreign key to the user

        [Required]
        public string Status { get; set; } = "Pending"; // e.g., "Pending", "Approved", "Rejected"

        public DateTime DateApplied { get; set; } = DateTime.Now; // Default to now
    }
}
