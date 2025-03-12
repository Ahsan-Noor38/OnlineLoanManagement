using System.ComponentModel.DataAnnotations;

namespace OnlineBankLoanPortal.ViewModels
{
    public class RegisterViewModel
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, ErrorMessage = "Password must be at least {2} characters long.", MinimumLength = 6)]
        public string Password { get; set; }

        //[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        //[Display(Name = "Confirm Password")]
        //public string ConfirmPassword { get; set; }
    }
}
