using Microsoft.AspNetCore.Identity;
using Models;
using RepositoryLayer.Context;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
namespace RepositoryLayer.Service;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

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

    public UserEntity? FindUserByEmail(string email)
    {
        return context.Users.FirstOrDefault(u => u.Email == email);
    }

    public UserEntity? GetUserById(int userId)
    {
        return context.Users.FirstOrDefault(user => user.UserId == userId);
    }

    public void UpdatePassword(UserEntity user)
    {
        context.Users.Update(user);
        context.SaveChanges();
    }


    public UserEntity? FindUserByExternalLogin(string provider, string providerSubject)
    {
        return context.ExternalLogins
            .Where(e => e.Provider == provider && e.ProviderSubject == providerSubject)
            .Select(e => e.User)
            .FirstOrDefault();
    }

    public bool ExternalLoginExists(string provider, string providerSubject)
    {
        return context.ExternalLogins.Any(
            e => e.Provider == provider &&
                 e.ProviderSubject == providerSubject);
    }

    public void AddExternalLogin(int userId, string provider, string providerSubject)
    {
        var externalLogin = new ExternalLoginEntity
        {
            UserId = userId,
            Provider = provider,
            ProviderSubject = providerSubject
        };

        context.ExternalLogins.Add(externalLogin);
        context.SaveChanges();
    }

    public UserEntity CreateGoogleUserWithExternalLogin(
        string email,
        string firstName,
        string lastName,
        string providerSubject)
    {
        using var transaction = context.Database.BeginTransaction();

        // Do not silently link an existing password account by email.
        if (context.Users.Any(u => u.Email == email))
        {
            throw new InvalidOperationException(
                "An account with this email already exists. " +
                "Sign in with that account and use a verified account-linking flow.");
        }

        var user = new UserEntity
        {
            FirstName = string.IsNullOrWhiteSpace(firstName)
                ? "Google"
                : firstName,
            LastName = string.IsNullOrWhiteSpace(lastName)
                ? "User"
                : lastName,
            Email = email,
            PhoneNumber = string.Empty
        };

        // Google users do not supply a Fundoo password.
        // Store a hash of a random, undisclosed secret instead.
        var randomSecret = Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(32));

        user.Password = hasher.HashPassword(user, randomSecret);

        context.Users.Add(user);
        context.SaveChanges();

        var externalLogin = new ExternalLoginEntity
        {
            UserId = user.UserId,
            Provider = "Google",
            ProviderSubject = providerSubject
        };

        context.ExternalLogins.Add(externalLogin);
        context.SaveChanges();

        transaction.Commit();

        return user;
    }


}