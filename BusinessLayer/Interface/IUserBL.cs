using Models;

namespace BusinessLayer.Interface;

public interface IUserBL
{
    public ResponseModel<string> RegisterUserBL(RegistrationModel register);
    public ResponseModel<LoginResponseModel> LoginUserBL(LoginModel login);
    public Task<ResponseModel<string>> ForgetPasswordBL(ForgetPasswordModel model);
    public Task<ResponseModel<string>> ResetPassword(ResetPasswordModel model);
    public ResponseModel<LoginResponseModel> GoogleLoginBL(string email, string providerSubject, string firstName, string lastName);
}