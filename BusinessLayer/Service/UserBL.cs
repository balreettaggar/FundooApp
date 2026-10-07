using BusinessLayer.Interface;
using Models;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;

namespace BusinessLayer.Service;

public class UserBL : IUserBL
{
    public IUserRL userRL;
    public JwtService jwtService;
    public UserBL(IUserRL userRL , JwtService jwtService)
    {
        this.userRL = userRL;
        this.jwtService = jwtService;
    }
    
    public ResponseModel<RegistrationModel> RegisterUserBL(RegistrationModel register)
    {
        return userRL.RegisterUserRL(register);
    }

    public ResponseModel<LoginResponseModel> LoginUserBL(LoginModel login)
    {
        
        UserEntity? userEntity =   userRL.LoginUserRL(login);
        ResponseModel<LoginResponseModel> rm = new ResponseModel<LoginResponseModel>();
        if (userEntity == null)
        {
            rm.IsSuccess = false;
            rm.Message = "login email or password is wrong";
            rm.Data = null;
        }
        else
        {
            rm.IsSuccess = true;
            rm.Message = "login success";
            rm.Data = new LoginResponseModel
            {
                Email = userEntity.Email,
                Token = jwtService.GenerateToken(userEntity)
            };
        }
        return rm;
    }
}