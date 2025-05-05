using Microsoft.AspNetCore.SignalR;

namespace OnlineBankLoanPortal.Hubs
{
    public class LoanApplicationHub : Hub
    {
        public async Task NotifyLoanApplicationCreated(int loanApplicationId)
        {
            await Clients.All.SendAsync("LoanApplicationCreated", loanApplicationId);
        }

        public async Task NotifyLoanApplicationStatusChanged(int loanApplicationId, string status)
        {
            await Clients.All.SendAsync("LoanApplicationStatusChanged", loanApplicationId, status);
        }
    }
}
