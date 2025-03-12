using Microsoft.AspNetCore.Identity;

namespace OnlineBankLoanPortal.Models;

public partial class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; }
    public virtual ICollection<LoanApplication> LoanApplications { get; set; } = [];
}
