using Models;
using RepositoryLayer.Entity;

namespace RepositoryLayer.Interface;

public interface IUserRL
{
    public ResponseModel<RegistrationModel> RegisterUserRL(RegistrationModel register);
    public UserEntity? LoginUserRL(LoginModel login);

    public UserEntity? GetUserById(int userId);

    public UserEntity? FindUserByEmail(string email);
    public void UpdatePassword(UserEntity user);

    UserEntity? FindUserByExternalLogin(string provider, string providerSubject);

    bool ExternalLoginExists(string provider, string providerSubject);

    void AddExternalLogin(int userId, string provider, string providerSubject);

}