using MediatR;
using Middleware.Service;
using Models;
using RepositoryLayer.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Middleware.Services.Encryption;

namespace BusinessLayer.Query.Users
{
    public class LoginUserHandler
    : IRequestHandler<LoginUserQuery, ResponseModel<LoginResponseModel>>
    {
        private readonly IUserRL userRL;
        private readonly PasswordHandler passwordService;
        private readonly JwtService jwtService;

        public LoginUserHandler(
            IUserRL userRL,
            PasswordHandler passwordService,
            JwtService jwtService)
        {
            this.userRL = userRL;
            this.passwordService = passwordService;
            this.jwtService = jwtService;
        }

        public Task<ResponseModel<LoginResponseModel>> Handle(
            LoginUserQuery request,
            CancellationToken cancellationToken)
        {
            var model = request.LoginModel;

            var user = userRL.FindUserByEmail(model.Email);

            if (user == null ||
                !passwordService.VerifyPassword(
                    model.Password,
                    user.Password,
                    user.Salt))
            {
                return Task.FromResult(
                    new ResponseModel<LoginResponseModel>
                    {
                        IsSuccess = false,
                        Message = "Invalid email or password.",
                        Data = null
                    });
            }

            var response = new ResponseModel<LoginResponseModel>
            {
                IsSuccess = true,
                Message = "Login successful.",
                Data = new LoginResponseModel
                {
                    Email = user.Email,
                    Token = jwtService.GenerateToken(user)
                }
            };

            return Task.FromResult(response);
        }
    }
}
