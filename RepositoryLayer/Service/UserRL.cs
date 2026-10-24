using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Models;
using RepositoryLayer.Context;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
namespace RepositoryLayer.Service;
using Microsoft.EntityFrameworkCore;

using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class UserRL : IUserRL
{
    FundooContext context; // The context is the repository's gateway to Entity Framework Core and the configured database.
                           //It gives your repository access to database operations without requiring you to manually open a database connection and
                           //write an SQL INSERT statement for every entity.

    private PasswordHasher<UserEntity> hasher;

    public UserRL(FundooContext context, PasswordHasher<UserEntity> hasher)
    {
        this.context = context;
        this.hasher = hasher;
    }

    // Remember the dependency chain?
    //- FundooController depends on IUserBL.
    //- UserBL depends on IUserRL.
    //- UserRL depends on FundooContext.

    public void RegisterUserRL(UserEntity user)
    {
        context.Users.Add(user); 
        // context is your FundooContext instance.
        // Users is a DbSet < UserEntity > property declared in that context.
        // Add(user) tells EF Core to track this entity as a new record to be inserted.
        // Add marks the entity to be saved in EF tracker
        context.SaveChanges();
        // If the insert succeeds, SaveChanges() normally returns the number of state entries written.
        // Your code ignores that return value.
        //If the insert fails because of a database error or constraint violation, SaveChanges() typically
        //throws an exception. Your current repository doesn't catch it.
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