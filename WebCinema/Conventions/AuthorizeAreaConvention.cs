using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace WebCinema.Conventions
{
    public class AuthorizeAreaConvention : IControllerModelConvention
    {
        private string _v;
        private readonly string _roles;

        public AuthorizeAreaConvention(string v, string roles)
        {
            _v = v;
            _roles = roles;
        }

        public void Apply(ControllerModel controller)
        {
            if (controller.RouteValues.TryGetValue("area", out var routeValues) && routeValues == _v)
            {
                if (_roles.IsNullOrEmpty())
                {
                    controller.Filters.Add(new AuthorizeFilter());
                }
                else
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .RequireRole(_roles)
                        .Build();

                    controller.Filters.Add(new AuthorizeFilter(policy));
                }
            }
        }
    }
}
