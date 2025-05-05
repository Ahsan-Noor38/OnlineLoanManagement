using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineBankLoanPortal.Models
{
    public class LoanApplicationDocument
    {
        [Key]
        public int LoanApplicationDocumentId { get; set; }

        [Required]
        [ForeignKey("LoanApplication")]
        public int LoanApplicationId { get; set; }

        [Required]
        public string DocumentType { get; set; } // E.g., "ID Card", "Pay Slip"

        [Required]
        public string FilePath { get; set; } // Store the file path or URL

        // Navigation Property
        public LoanApplication LoanApplication { get; set; }
    }
}
