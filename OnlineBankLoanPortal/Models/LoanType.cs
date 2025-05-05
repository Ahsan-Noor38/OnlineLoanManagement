using System.ComponentModel.DataAnnotations;

namespace OnlineBankLoanPortal.Models
{
    public class LoanType
    {
        [Key]
        public int LoanTypeId { get; set; }

        [Required(ErrorMessage = "Loan Type Name is required.")]
        [StringLength(255)]
        [Display(Name = "Loan Type Name")]
        public string LoanTypeName { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        // Navigation property
        public ICollection<InterestRate> InterestRates { get; set; }
        public ICollection<RepaymentTerm> RepaymentTerms { get; set; }
        public ICollection<LoanApplication> LoanApplications { get; set; }
    }
}
