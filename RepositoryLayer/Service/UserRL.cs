using Microsoft.AspNetCore.Identity;
using Models;
using RepositoryLayer.Context;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
namespace RepositoryLayer.Service;

public class UserRL : IUserRL
{
    FundooContext context;
    private PasswordHasher<UserEntity> hasher;
    public UserRL(FundooContext context , PasswordHasher<UserEntity> hasher)
    {
        this.context = context;
        this.hasher = hasher;
    }
    
    
    public ResponseModel<RegistrationModel> RegisterUserRL(RegistrationModel register)
    {
        var tempUser = context.Users.FirstOrDefault(u => u.Email == register.Email);
        ResponseModel<RegistrationModel> rm = new ResponseModel<RegistrationModel>();
        if (tempUser != null)
        {
            rm.IsSuccess = false;
            rm.Message = "Email already exists";
            rm.Data = null;
            return rm;
        }
        
        UserEntity user = new UserEntity();
        user.FirstName = register.FirstName;
        user.LastName = register.LastName;
        user.Email = register.Email;
        user.PhoneNumber = register.ContactNumber;
        
        user.Password = hasher.HashPassword(user, register.Password);
        
        context.Users.Add(user);
      var result =   context.SaveChanges();
      Console.WriteLine(result);
      rm.IsSuccess = true;
      rm.Message = "User successfully registered";
      rm.Data = register;
        return rm;
    }

    public UserEntity? LoginUserRL(LoginModel login)
    {
        var user = context.Users.FirstOrDefault(u => u.Email == login.Email );
        
        LoginResponseModel loginResponse = new LoginResponseModel();
        loginResponse.Email = login.Email;
        if (user == null)
        {
            return null;
        }
        
        var decryptedPassword = hasher.VerifyHashedPassword(user, user.Password, login.Password);

        if (decryptedPassword == PasswordVerificationResult.Success) return user;
        

        return null;
    }
    
    
    
}