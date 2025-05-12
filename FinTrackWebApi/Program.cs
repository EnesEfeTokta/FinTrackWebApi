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
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using FinTrackWebApi.Services.ChatBotService.Plugins;
using FinTrackWebApi.Services.ChatBotService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Trace);

builder.Services.Configure<CurrencyFreaksSettings>(builder.Configuration.GetSection("CurrencyFreaks"));
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient("CurrencyFreaksClient", (serviceProvider, client) =>
{
    var settings = serviceProvider.GetRequiredService<IOptions<CurrencyFreaksSettings>>().Value;
    if (!string.IsNullOrWhiteSpace(settings.BaseUrl))
    {
        if (Uri.TryCreate(settings.BaseUrl, UriKind.Absolute, out var baseUri))
        {
            client.BaseAddress = baseUri;
        }
        else
        {
            Console.Error.WriteLine($"Geçersiz BaseUrl formatı: {settings.BaseUrl}");
        }
    }
    else
    {
        Console.Error.WriteLine("CurrencyFreaks BaseUrl yapılandırılmamış!");
    }
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
    options.UseNpgsql(connectionString),
    ServiceLifetime.Scoped);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<FinancePlugin>();

builder.Services.AddControllers();
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

var geminiApiKey = builder.Configuration["Gemini:ApiKey"];
var geminiModelId = builder.Configuration["Gemini:ModelId"];

if (string.IsNullOrWhiteSpace(geminiApiKey))
{
    throw new InvalidOperationException("Gemini API anahtarı yapılandırmada bulunamadı. 'Gemini:ApiKey' değerini kontrol edin.");
}
if (string.IsNullOrWhiteSpace(geminiModelId))
{
    geminiModelId = "gemini-1.5-flash-latest";
    Console.WriteLine($"Uyarı: Gemini Model ID yapılandırmada bulunamadı. Varsayılan model kullanılıyor: {geminiModelId}");
}

var kernelBuilder = Kernel.CreateBuilder();

kernelBuilder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConfiguration(builder.Configuration.GetSection("Logging"));
    loggingBuilder.AddConsole();
    loggingBuilder.SetMinimumLevel(LogLevel.Trace);
});
kernelBuilder.Services.AddMemoryCache();
kernelBuilder.Services.AddHttpContextAccessor();
kernelBuilder.Services.AddDbContext<MyDataContext>(options =>
    options.UseNpgsql(connectionString),
    ServiceLifetime.Scoped);
kernelBuilder.Services.AddScoped<FinancePlugin>();


#pragma warning disable SKEXP0070, SKEXP0011, SKEXP0020
kernelBuilder.AddGoogleAIGeminiChatCompletion(
    modelId: geminiModelId,
    apiKey: geminiApiKey
);
#pragma warning restore SKEXP0070, SKEXP0011, SKEXP0020

kernelBuilder.Plugins.AddFromType<FinancePlugin>();

var kernel = kernelBuilder.Build();

builder.Services.AddSingleton(kernel);
builder.Services.AddSingleton<IChatCompletionService>(sp => sp.GetRequiredService<Kernel>().GetRequiredService<IChatCompletionService>());
builder.Services.AddScoped<IChatBotService, ChatBotService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var serviceProviderInScope = scope.ServiceProvider;
    var logger = serviceProviderInScope.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("--- ANA DI ÇÖZÜMLEME TESTİ BAŞLIYOR ---");
    try
    {
        var dbContext = serviceProviderInScope.GetService<MyDataContext>();
        logger.LogInformation(dbContext == null ? "HATA: MyDataContext ANA DI'DAN ÇÖZÜLEMEDİ!" : "BAŞARI: MyDataContext ANA DI'DAN çözüldü.");
    }
    catch (Exception exFactory)
    {
        logger.LogError(exFactory, "HATA: MyDataContext çözümlenirken genel bir istisna oluştu!");
    }

    try
    {
        var financePluginInstance = serviceProviderInScope.GetService<FinancePlugin>();
        logger.LogInformation(financePluginInstance == null ? "HATA: FinancePlugin ANA DI'DAN ÇÖZÜLEMEDİ!" : "BAŞARI: FinancePlugin ANA DI'DAN çözüldü.");
    }
    catch (Exception exPlugin)
    {
        logger.LogError(exPlugin, "HATA: FinancePlugin çözümlenirken bir istisna oluştu!");
    }
    var kernelInstance = serviceProviderInScope.GetService<Kernel>();
    logger.LogInformation(kernelInstance == null ? "HATA: Kernel ANA DI'DAN ÇÖZÜLEMEDİ!" : "BAŞARI: Kernel ANA DI'DAN çözüldü.");

    if (kernelInstance != null && kernelInstance.Plugins.Any())
    {
        logger.LogInformation("Kernel'a yüklenen plugin'ler ({Count} adet):", kernelInstance.Plugins.Count);
        foreach (var pluginEntry in kernelInstance.Plugins)
        {
            logger.LogInformation("  Plugin Adı: {PluginName}", pluginEntry.Name);
            if (!pluginEntry.Any()) { logger.LogWarning("    Plugin '{PluginName}' içinde HİÇ fonksiyon bulunmuyor!", pluginEntry.Name); }
            else { foreach (var func in pluginEntry) { logger.LogInformation("    - Fonksiyon: {FunctionName}, Açıklama: {Description}", func.Name, func.Description); } }
        }
    }
    else if (kernelInstance != null)
    {
        logger.LogWarning("KERNEL'DA HİÇ PLUGIN BULUNMUYOR (Kernel instance var ama plugin yok)!");
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