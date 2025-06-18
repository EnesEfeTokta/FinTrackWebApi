using System.Text;
using System.Text.Json.Serialization;
using FinTrackWebApi.Data;
using FinTrackWebApi.Models;
using FinTrackWebApi.Services.ChatBotService;
using FinTrackWebApi.Services.CurrencyServices;
using FinTrackWebApi.Services.DocumentService;
using FinTrackWebApi.Services.DocumentService.Generations.Budget;
using FinTrackWebApi.Services.DocumentService.Generations.Transaction;
using FinTrackWebApi.Services.EmailService;
using FinTrackWebApi.Services.MediaEncryptionService;
using FinTrackWebApi.Services.OtpService;
using FinTrackWebApi.Services.PaymentService;
using FinTrackWebApi.Services.SecureDebtService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Trace);

builder.Services.Configure<CurrencyFreaksSettings>(
    builder.Configuration.GetSection("CurrencyFreaks")
);
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient("PythonChatBotClient");

builder.Services.AddHttpClient(
    "CurrencyFreaksClient",
    (serviceProvider, client) =>
    {
        var settings = serviceProvider.GetRequiredService<IOptions<CurrencyFreaksSettings>>().Value;
        if (string.IsNullOrWhiteSpace(settings.BaseUrl))
        {
            throw new InvalidOperationException(
                "CurrencyFreaks BaseUrl is not configured in appsettings.json."
            );
        }
        client.BaseAddress = new Uri(settings.BaseUrl);
    }
);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "DefaultConnection connection string is not configured."
    );

builder.Services.AddDbContext<MyDataContext>(options => options.UseNpgsql(connectionString));

builder
    .Services.AddIdentityCore<UserModel>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole<int>>()
    .AddSignInManager<SignInManager<UserModel>>()
    .AddEntityFrameworkStores<MyDataContext>()
    .AddDefaultTokenProviders();

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = builder.Environment.IsProduction();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Token:Issuer"],
            ValidAudience = builder.Configuration["Token:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Token:SecurityKey"]
                        ?? throw new InvalidOperationException(
                            "Token:SecurityKey is not configured."
                        )
                )
            ),
            ClockSkew = TimeSpan.Zero,
        };
        Console.WriteLine(
            $"TOKEN VALIDATION - Using SecurityKey from config: '{builder.Configuration["Token:SecurityKey"]}'"
        );
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

builder.Services.ConfigureApplicationCookie(options =>
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
    options.Cookie.Name = "FinTrack.AuthCookie.Suppressed"; // �sim �nemli de�il kullan�lmayacak.
});

builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<PdfDocumentGenerator_Budget>();
builder.Services.AddScoped<WordDocumentGenerator_Budget>();
builder.Services.AddScoped<TextDocumentGenerator_Budget>();
builder.Services.AddScoped<MarkdownDocumentGenerator_Budget>();
builder.Services.AddScoped<XlsxDocumentGenerator_Budget>();
builder.Services.AddScoped<XmlDocumentGenerator_Budget>();
builder.Services.AddScoped<PdfDocumentGenerator_Transaction>();
builder.Services.AddScoped<WordDocumentGenerator_Transaction>();
builder.Services.AddScoped<TextDocumentGenerator_Transaction>();
builder.Services.AddScoped<MarkdownDocumentGenerator_Transaction>();
builder.Services.AddScoped<XlsxDocumentGenerator_Transaction>();
builder.Services.AddScoped<XmlDocumentGenerator_Transaction>();
builder.Services.AddScoped<IDocumentGenerationService, DocumentGenerationService>();
builder.Services.AddScoped<ICurrencyDataProvider, CurrencyFreaksProvider>();
builder.Services.AddScoped<ICurrencyService, CurrencyCacheService>();
builder.Services.AddScoped<ISecureDebtService, SecureDebtService>();
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("StripeSettings"));
builder.Services.AddScoped<IPaymentService, StripePaymentService>();
builder.Services.AddHostedService<CurrencyUpdateService>();
builder.Services.AddScoped<IMediaEncryptionService, MediaEncryptionService>();
builder.Services.AddScoped<IChatBotService, ChatBotService>();

builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "FinTrack API", Version = "v1" });
    options.AddSecurityDefinition(
        JwtBearerDefaults.AuthenticationScheme,
        new OpenApiSecurityScheme
        {
            Description =
                "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            BearerFormat = "JWT",
        }
    );
    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = JwtBearerDefaults.AuthenticationScheme,
                    },
                },
                new string[] { }
            },
        }
    );
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation(
            "Application starting. Seeding database with roles and admin user..."
        );

        var userManager = services.GetRequiredService<UserManager<UserModel>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

        string[] roleNames = { "Admin", "User", "Operator" };
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
    }
}

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
