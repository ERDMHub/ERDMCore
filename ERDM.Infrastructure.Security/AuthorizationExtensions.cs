using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace ERDM.Infrastructure.Security
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddCustomAuthorization(this IServiceCollection services)
        {
            services.AddAuthorizationCore(options =>
            {
                options.AddPolicy("CreditOfficer", policy =>
                    policy.RequireRole("CreditOfficer", "Admin"));

                options.AddPolicy("Underwriter", policy =>
                    policy.RequireRole("Underwriter", "Admin"));

                options.AddPolicy("Admin", policy =>
                    policy.RequireRole("Admin"));

                // Permission-based policies
                options.AddPolicy("Permission:applications.view", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim("permissions", Permissions.ViewApplications) ||
                        context.User.IsInRole("Admin")));

                options.AddPolicy("Permission:applications.approve", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim("permissions", Permissions.ApproveApplications) ||
                        context.User.IsInRole("Underwriter") ||
                        context.User.IsInRole("Admin")));
            });

            services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

            return services;
        }
    }
}
