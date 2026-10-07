using Models;

namespace BusinessLayer.Interface;

public interface IUserBL
{
    public ResponseModel<RegistrationModel> RegisterUserBL(RegistrationModel register);
    public ResponseModel<LoginResponseModel> LoginUserBL(LoginModel login);
}