using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineBankLoanPortal.Models
{
    public class InterestRate
    {
        [Key]
        public int InterestRateId { get; set; }

        [ForeignKey("LoanType")]
        [Required(ErrorMessage = "Loan Type is required.")]
        [Display(Name = "Loan Type")]
        public int LoanTypeId { get; set; }
        public LoanType LoanType { get; set; }

        [Required(ErrorMessage = "Interest Rate is required.")]
        [Range(0.01, 100, ErrorMessage = "Interest Rate must be between 0.01 and 100.")]
        [Display(Name = "Rate (%)")]
        [Column(TypeName = "decimal(5, 2)")]
        public decimal Rate { get; set; }

        [Required(ErrorMessage = "Effective Date is required.")]
        [Display(Name = "Effective Date")]
        public DateTime EffectiveDate { get; set; }

        [Display(Name = "End Date")]
        public DateTime? EndDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
    }
}
