using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineBankLoanPortal.Models
{
    public class Payment
    {
        [Key]
        public int PaymentId { get; set; }
        [Required]
        public int LoanApplicationId { get; set; }
        [ForeignKey("LoanApplicationId")]
        public LoanApplication LoanApplication { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public DateTime PaymentDate { get; set; }
        public string ReceiptImageUrl { get; set; }
        public string RecordedBy { get; set; } // maps to AspNetUsers.Id
    }
}
