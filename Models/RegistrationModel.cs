using System.ComponentModel.DataAnnotations;

namespace Models;

public class RegistrationModel
{
    [MaxLength(15 , ErrorMessage = "Password is too long")]
    public string FirstName { get; set; }
    [MaxLength(15 , ErrorMessage = "Password is too long")]
    public string LastName { get; set; }
    [Required]
    [RegularExpression(@"^[a-z][a-z0-9]{1,15}$")]
    public string UserName { get; set; }
    [RegularExpression(@"^[a-zA-Z0-9]{1,9}")]
    [Required]
    public string Password { get; set; }
    
    [RegularExpression(@"^[0-9]{1,11}$")]
    public string ContactNumber { get; set; }
    
    [EmailAddress]
    public string Email { get; set; }

    public RegistrationModel() { }
    public RegistrationModel(string firstName, string lastName, string userName, string password, string contactNumber,
        string email)
    {
        FirstName = firstName;
        LastName = lastName;
        UserName = userName;
        Password = password;
        ContactNumber = contactNumber;
        Email = email;
    }
}