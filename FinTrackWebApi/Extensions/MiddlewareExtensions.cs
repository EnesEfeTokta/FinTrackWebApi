using FinTrackWebApi.Middlewares;
using Microsoft.AspNetCore.Identity;

namespace FinTrackWebApi.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }

        public static WebApplication ConfigurePipeline(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FinTrack API V1"));
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseRouting();
            app.UseRequestLogging();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            return app;
        }

        public static async Task SeedDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                logger.LogInformation("Application starting. Seeding database with roles...");
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
                var roleNames = new[] { "Admin", "User", "Operator" };
                IdentityResult roleResult;

                foreach (var roleName in roleNames)
                {
                    var roleExist = await roleManager.RoleExistsAsync(roleName);
                    if (!roleExist)
                    {
                        roleResult = await roleManager.CreateAsync(new IdentityRole<int>(roleName));
                        if (roleResult.Succeeded)
                        {
                            logger.LogInformation("Role '{RoleName}' created successfully.", roleName);
                        }
                        else
                        {
                            foreach (var error in roleResult.Errors)
                            {
                                logger.LogError(
                                    "Error creating role '{RoleName}': {ErrorDescription}",
                                    roleName,
                                    error.Description
                                );
                            }
                        }
                    }
                    else
                    {
                        logger.LogInformation("Role '{RoleName}' already exists.", roleName);
                    }
                }

                logger.LogInformation("Database seeding finished.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }
    }
}
