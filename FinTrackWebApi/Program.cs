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

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

builder.Services.Configure<CurrencyFreaksSettings>(builder.Configuration.GetSection("CurrencyFreaks"));

builder.Services.AddMemoryCache();

builder.Services.AddHttpClient("CurrencyFreaksClient", (serviceProvider, client) =>
{
    // Ayarlarý IOptions ile güvenli bir þekilde al
    var settings = serviceProvider.GetRequiredService<IOptions<CurrencyFreaksSettings>>().Value;

    // BaseUrl ayarlanmýþsa HttpClient'ýn BaseAddress'ini ayarla
    if (!string.IsNullOrWhiteSpace(settings.BaseUrl))
    {
        // Uri'nin geçerli olduðundan emin olalým
        if (Uri.TryCreate(settings.BaseUrl, UriKind.Absolute, out var baseUri))
        {
            client.BaseAddress = baseUri;
        }
        else
        {
            // Loglama veya hata fýrlatma eklenebilir
            Console.Error.WriteLine($"Geçersiz BaseUrl formatý: {settings.BaseUrl}");
        }
    }
    else
    {
        Console.Error.WriteLine("CurrencyFreaks BaseUrl yapýlandýrýlmamýþ!");
        // Loglama eklenebilir
    }
    // Gerekirse diðer HttpClient ayarlarý (Timeout, Default Headers vb.)
    // client.Timeout = TimeSpan.FromSeconds(30);
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Token:SecurityKey"])),
            ClockSkew = TimeSpan.Zero
        };
    });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// --- Servisler ---
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

// --- Arkaplan Servisleri ---
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


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();