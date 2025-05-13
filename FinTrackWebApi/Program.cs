using Microsoft.EntityFrameworkCore;
using FinTrackWebApi.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FinTrackWebApi.Services.EmailService;
using FinTrackWebApi.Services.OtpService;
using Microsoft.OpenApi.Models;
using FinTrackWebApi.Services.DocumentService;
using QuestPDF.Infrastructure;
using FinTrackWebApi.Services.CurrencyServices;
using Microsoft.Extensions.Options;
using FinTrackWebApi.Services.PaymentService;
using FinTrackWebApi.Services.ChatBotService;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Trace);

builder.Services.Configure<CurrencyFreaksSettings>(builder.Configuration.GetSection("CurrencyFreaks"));
builder.Services.AddMemoryCache(); // Genel cacheleme için kalabilir
builder.Services.AddHttpClient(); // IHttpClientFactory için genel kayıt
builder.Services.AddHttpClient("PythonChatBotClient"); // İsimlendirilmiş HttpClient

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

builder.Services.AddDbContext<MyDataContext>(options => // VEYA AddDbContextFactory
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

// ChatBot servisini (Python servisine istek atacak olan) kaydet
builder.Services.AddScoped<IChatBotService, ChatBotService>();

var app = builder.Build();

// Başlangıç loglamalarını (Kernel plugin loglaması gibi) bu modelde kaldırabiliriz,
// çünkü Kernel artık bu projede değil. İsterseniz genel DI testi kalabilir.
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("--- ANA DI ÇÖZÜMLEME TESTİ (app.Services KULLANILARAK) BAŞLIYOR ---");
    try
    {
        var chatBotService = scope.ServiceProvider.GetService<IChatBotService>();
        logger.LogInformation(chatBotService == null ? "HATA: IChatBotService ANA DI'DAN ÇÖZÜLEMEDİ!" : "BAŞARI: IChatBotService ANA DI'DAN çözüldü.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "HATA: IChatBotService çözümlenirken genel bir istisna oluştu!");
    }
    logger.LogInformation("--- ANA DI ÇÖZÜMLEME TESTİ BİTTİ ---");
}


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