using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineBankLoanPortal.Models
{
    public class RepaymentTerm
    {
        [Key]
        public int RepaymentTermId { get; set; }

        [ForeignKey("LoanType")]
        [Required(ErrorMessage = "Loan Type is required.")]
        [Display(Name = "Loan Type")]
        public int LoanTypeId { get; set; }
        public LoanType LoanType { get; set; }

        [Required(ErrorMessage = "Term in Months is required.")]
        [Range(1, 360, ErrorMessage = "Term must be between 1 and 360 months.")]
        [Display(Name = "Term (Months)")]
        public int TermInMonths { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
    }
}
