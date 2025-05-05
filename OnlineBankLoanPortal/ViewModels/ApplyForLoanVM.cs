using System.ComponentModel.DataAnnotations;

namespace OnlineBankLoanPortal.ViewModels
{
    public class ApplyForLoanVM
    {
        [Required(ErrorMessage = "Please select a loan type.")]
        public int LoanTypeId { get; set; }

        [Required(ErrorMessage = "Please enter the loan amount.")]
        [Range(1, double.MaxValue, ErrorMessage = "Loan amount must be greater than zero.")]
        public decimal LoanAmount { get; set; }

        [Required(ErrorMessage = "Please enter the purpose of the loan.")]
        public string Purpose { get; set; }
        public List<IFormFile>? Files { get; set; }
    }
}
