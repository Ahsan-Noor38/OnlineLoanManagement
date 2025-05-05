using OnlineBankLoanPortal.Models;

namespace OnlineBankLoanPortal.ViewModels
{
    public class AdminDashboardVM
    {
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int RejectedApplications { get; set; }
        public int CancelledApplications { get; set; }
        public List<LoanApplication> RecentApplications { get; set; }
    }
}
