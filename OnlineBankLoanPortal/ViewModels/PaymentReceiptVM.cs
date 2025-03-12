using System.ComponentModel.DataAnnotations;

namespace OnlineBankLoanPortal.ViewModels
{
    public class PaymentReceiptVM
    {
        public int Id { get; set; }

        public int LoanApplicationId { get; set; } // Foreign key to LoanApplication

        [Required]
        [Display(Name = "Receipt File")]
        public string ReceiptFilePath { get; set; } // Path to the uploaded file

        [Required]
        [Display(Name = "Payment Date")]
        public DateTime PaymentDate { get; set; } = DateTime.Now; // Default to now
    }
}
