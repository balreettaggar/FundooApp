using MediatR;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Query.Users
{
    public class LoginUserQuery : IRequest<ResponseModel<LoginResponseModel>>
    {
        public LoginModel LoginModel { get; set; } = new();
    }
}
