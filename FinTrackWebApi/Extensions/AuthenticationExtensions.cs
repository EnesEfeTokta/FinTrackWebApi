using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace FinTrackWebApi.Extensions
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddAuthenticationServices(
            this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = environment.IsProduction();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Token:Issuer"],
                    ValidAudience = configuration["Token:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            configuration["Token:SecurityKey"]
                            ?? throw new InvalidOperationException("Token:SecurityKey is not configured.")
                        )
                    ),
                    ClockSkew = TimeSpan.Zero
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine(
                            "!!!!!!!!!!!!!!!!! [OnAuthenticationFailed] JWT AUTHENTICATION FAILED !!!!!!!!!!!!!!!!!"
                        );
                        Console.WriteLine("Exception Type: " + context.Exception?.GetType().FullName);
                        Console.WriteLine("Exception Message: " + context.Exception?.Message);
                        Console.WriteLine(
                            "------------------------------------------------------------------------------------"
                        );
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine(
                            "***************** [OnTokenValidated] JWT TOKEN VALIDATED *****************"
                        );
                        Console.WriteLine(
                            "Validated for Principal Identity Name: " + context.Principal.Identity?.Name
                        );
                        if (context.Principal.Claims.Any())
                        {
                            foreach (var claim in context.Principal.Claims)
                            {
                                Console.WriteLine(
                                    $"VALIDATED CLAIM: Type = {claim.Type}, Value = {claim.Value}"
                                );
                            }
                        }
                        else
                        {
                            Console.WriteLine("No claims found in the validated principal.");
                        }
                        Console.WriteLine(
                            "**************************************************************************"
                        );
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        Console.WriteLine(">>> [OnMessageReceived] Attempting to process message for JWT.");
                        var authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                        if (string.IsNullOrEmpty(authorizationHeader))
                        {
                            Console.WriteLine(">>> [OnMessageReceived] Authorization header is MISSING.");
                        }
                        else if (
                            !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                        )
                        {
                            Console.WriteLine(
                                $">>> [OnMessageReceived] Authorization header is PRESENT but NOT 'Bearer ': {authorizationHeader.Substring(0, Math.Min(authorizationHeader.Length, 20))}"
                            );
                        }
                        else
                        {
                            Console.WriteLine(
                                ">>> [OnMessageReceived] Authorization 'Bearer' header is PRESENT and looks OK for JWT."
                            );
                        }
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        Console.WriteLine("--- [OnChallenge] JWT OnChallenge Triggered ---");
                        Console.WriteLine("Error: " + context.Error);
                        Console.WriteLine("Error Description: " + context.ErrorDescription);
                        if (context.AuthenticateFailure != null)
                        {
                            Console.WriteLine(
                                "AuthenticateFailure in OnChallenge (JWT): "
                                    + context.AuthenticateFailure.GetType().FullName
                                    + " - "
                                    + context.AuthenticateFailure.Message
                            );
                        }
                        Console.WriteLine("---------------------------------------------");
                        return Task.CompletedTask;
                    },
                };
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                };
                options.Cookie.Name = "FinTrack.AuthCookie.Suppressed";
            });

            return services;
        }
    }
}
