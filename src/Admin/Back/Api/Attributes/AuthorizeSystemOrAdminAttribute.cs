using GesFer.Admin.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GesFer.Admin.Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeSystemOrAdminAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var httpContext = context.HttpContext;
        var logger = httpContext.RequestServices.GetRequiredService<ILogger<AuthorizeSystemOrAdminAttribute>>();

        // 1. Validar Shared Secret (System)
        if (httpContext.Request.Headers.TryGetValue("X-Internal-Secret", out var secretValues))
        {
            var secret = secretValues.FirstOrDefault();
            var configuration = httpContext.RequestServices.GetRequiredService<IConfiguration>();
            var configuredSecret = configuration["SharedSecret"];

            if (!string.IsNullOrEmpty(secret) && secret == configuredSecret)
            {
                // Autenticado como Sistema
                return;
            }
            logger.LogWarning("Intento de acceso por sistema fallido: Secret incorrecto");
        }

        // 2. Validar JWT (Admin) - Si no es sistema, delegar a la autenticación estándar de ASP.NET Core
        // Pero como este es un filtro personalizado, necesitamos verificar manualmente si el usuario está autenticado y es Admin
        // O simplemente dejar pasar si no hay secret, asumiendo que el endpoint tiene [Authorize] adicional.
        // Sin embargo, para simplificar, si no es sistema, requerimos que el usuario esté autenticado como Admin.

        var user = httpContext.User;
        if (user.Identity?.IsAuthenticated == true && user.IsInRole("Admin"))
        {
            // Autenticado como Admin
            return;
        }

        // Fallo en ambas
        context.Result = new UnauthorizedResult();
    }
}
