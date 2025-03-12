using System.ComponentModel.DataAnnotations;

namespace OnlineBankLoanPortal.ViewModels
{
    public class RepaymentVM
    {
        public int LoanApplicationId { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal AmountPaid { get; set; }

        [Required(ErrorMessage = "Payment date is required.")]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; }

        [Required(ErrorMessage = "Receipt is required.")]
        public IFormFile Receipt { get; set; }
    }
}
