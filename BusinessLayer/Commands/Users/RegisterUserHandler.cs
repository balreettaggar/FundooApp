using MediatR;
using Models;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Middleware.Services.Encryption;

namespace BusinessLayer.Command.Users
{
    public class RegisterUserHandler
    : IRequestHandler<RegisterUserCommand, ResponseModel<string>>
    {
        private readonly IUserRL userRL;
        private readonly PasswordHandler _passwordService;

        public RegisterUserHandler(
            IUserRL userRL,
            PasswordHandler passwordService)
        {
            this.userRL = userRL;
            _passwordService = passwordService;
        }

        public Task<ResponseModel<string>> Handle(
            RegisterUserCommand request,
            CancellationToken cancellationToken)
        {
            var model = request.RegisterModel;

            var (hash, salt) =
                _passwordService.HashPassword(model.Password);

            var user = new UserEntity
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.ContactNumber,
                Password = hash,
                Salt = salt
            };

            userRL.RegisterUserRL(user);

            var response = new ResponseModel<string>
            {
                IsSuccess = true,
                Message = "Registration successful.",
                Data = user.Email
            };

            return Task.FromResult(response);
        }
    }
}
