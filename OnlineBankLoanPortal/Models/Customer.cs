using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineBankLoanPortal.Models;

public class Customer
{
    [Key]
    public int CustomerId { get; set; }

    [Required]
    [ForeignKey("ApplicationUser")]
    public Guid UserId { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Address { get; set; }

    // Navigation property
    public ApplicationUser? ApplicationUser { get; set; }
}
