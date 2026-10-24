using MediatR;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.Command.Users
{
    public class ResetPasswordCommand : IRequest<ResponseModel<string>>
    {
        public ResetPasswordModel ResetPasswordModel { get; set; } = new();
    }
}
