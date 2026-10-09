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
}