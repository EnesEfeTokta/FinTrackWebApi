using FinTrackWebApi.Data;
using FinTrackWebApi.Services.ChatBotService;
using FinTrackWebApi.Services.CurrencyServices;
using FinTrackWebApi.Services.DocumentService;
using FinTrackWebApi.Services.EmailService;
using FinTrackWebApi.Services.OtpService;
using FinTrackWebApi.Services.PaymentService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Trace);

builder.Services.Configure<CurrencyFreaksSettings>(builder.Configuration.GetSection("CurrencyFreaks"));
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient("PythonChatBotClient");

builder.Services.AddHttpClient("CurrencyFreaksClient", (serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<CurrencyFreaksSettings>>().Value;

    if (string.IsNullOrWhiteSpace(settings.BaseUrl))
    {
        throw new InvalidOperationException("CurrencyFreaks BaseUrl is not configured in appsettings.json.");
    }
    client.BaseAddress = new Uri(settings.BaseUrl);
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Token:Issuer"],
            ValidAudience = builder.Configuration["Token:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Token:SecurityKey"] ?? throw new InvalidOperationException("Token:SecurityKey is not configured."))),
            ClockSkew = TimeSpan.Zero
        };
    });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("DefaultConnection connection string is not configured.");
}

builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<PdfDocumentGenerator>();
builder.Services.AddScoped<WordDocumentGenerator>();
builder.Services.AddScoped<TextDocumentGenerator>();
builder.Services.AddScoped<MarkdownDocumentGenerator>();
builder.Services.AddScoped<XlsxDocumentGenerator>();
builder.Services.AddScoped<XmlDocumentGenerator>();
builder.Services.AddScoped<IDocumentGenerationService, DocumentGenerationService>();
builder.Services.AddScoped<ICurrencyDataProvider, CurrencyFreaksProvider>();
builder.Services.AddScoped<ICurrencyService, CurrencyCacheService>();
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("StripeSettings"));
builder.Services.AddScoped<IPaymentService, StripePaymentService>();
builder.Services.AddHostedService<CurrencyUpdateService>();

builder.Services.AddDbContext<MyDataContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "FinTrack API", Version = "v1" });
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                      "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                      "Example: \"Bearer 12345abcdef\""
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddScoped<IChatBotService, ChatBotService>();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();