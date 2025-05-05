using Microsoft.AspNetCore.Identity;
using OnlineBankLoanPortal.Models;
using OnlineBankLoanPortal.Repository;

namespace OnlineBankLoanPortal
{
    public static class SeedData
    {
        public static async Task Initialize(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.EnsureCreated();
                if (!context.Users.Any())
                {
                    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

                    string[] roleNames = { "Customer", "Admin", "LoanOfficer" };
                    IdentityResult roleResult;

                    foreach (var roleName in roleNames)
                    {
                        var roleExist = await roleManager.RoleExistsAsync(roleName);
                        if (!roleExist)
                            roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
                    }

                    var adminUser = new ApplicationUser { FullName = "Admin", UserName = "admin@example.com", Email = "admin@example.com", EmailConfirmed = true, PhoneNumberConfirmed = true, PhoneNumber = "090078601" };
                    string adminPassword = "Admin@123";
                    var user = await userManager.FindByEmailAsync(adminUser.Email);

                    if (user == null)
                    {
                        var createAdminUser = await userManager.CreateAsync(adminUser, adminPassword);
                        if (createAdminUser.Succeeded)
                            await userManager.AddToRoleAsync(adminUser, "Admin");
                    }

                    var loanOfficerUser = new ApplicationUser { FullName = "Loan Officer", UserName = "officer@example.com", Email = "officer@example.com", EmailConfirmed = true, PhoneNumberConfirmed = true, PhoneNumber = "090078601" };
                    string loanOfficerPassword = "Officer@123";
                    var user2 = await userManager.FindByEmailAsync(loanOfficerUser.Email);

                    if (user2 == null)
                    {
                        var createloanofficerUser = await userManager.CreateAsync(loanOfficerUser, loanOfficerPassword);
                        if (createloanofficerUser.Succeeded)
                            await userManager.AddToRoleAsync(loanOfficerUser, "LoanOfficer");
                    }
                }
            }
        }
    }
}
