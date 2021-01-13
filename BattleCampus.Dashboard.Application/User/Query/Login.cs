using Application.Errors;
using BattleCampus.Core.Utils;
using BattleCampus.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BattleCampus.Dashboard.Application.User.Query
{
    public class Login
    {
        public class Query : IRequest<AdminUser>
        {
            public string Id { get; set; }
            public string Password { get; set; }
        }

        public class Hander : IRequestHandler<Query, AdminUser>
        {
            private readonly SignInManager<IdentityUser> _signInManager;
            private readonly UserManager<IdentityUser> _userManager;
            private readonly IJwtGenerator _jwtGenerator;

            public Hander(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IJwtGenerator jwtGenerator)
            {
                _signInManager = signInManager;
                _userManager = userManager;
                _jwtGenerator = jwtGenerator;
            }

            public async Task<AdminUser> Handle(Query request, CancellationToken cancellationToken)
            {
                if (string.IsNullOrEmpty(request.Id) || string.IsNullOrEmpty(request.Password))
                {
                    throw new RestException(HttpStatusCode.BadRequest, "Id and password cannot be empty");
                }

                var user = await _userManager.FindByIdAsync(request.Id);

                if (user == null)
                    throw new RestException(HttpStatusCode.Unauthorized);

                var result = await _signInManager
                    .CheckPasswordSignInAsync(user, request.Password, false);

                if (result.Succeeded)
                {
                    // TODO: generate token
                    return new AdminUser
                    {
                        Token = _jwtGenerator.CreateToken(user),
                        Name = user.UserName,
                    };
                }

                throw new RestException(HttpStatusCode.Unauthorized);
            }
        }
    }
}
