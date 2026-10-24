using Models;
using RepositoryLayer.Entity;

namespace RepositoryLayer.Interface;

public interface IUserRL
{
    public void RegisterUserRL(UserEntity user);

    public UserEntity? GetUserById(int userId);

    public UserEntity? FindUserByEmail(string email);
    public void UpdatePassword(UserEntity user);

    UserEntity? FindUserByExternalLogin(string provider, string providerSubject);

    bool ExternalLoginExists(string provider, string providerSubject);

    void AddExternalLogin(int userId, string provider, string providerSubject);

    UserEntity CreateGoogleUserWithExternalLogin(
        string email,
        string firstName,
        string lastName,
        string providerSubject);

}