using FinTrackWebApi.Services.ChatBotService;
using FinTrackWebApi.Services.CurrencyServices;
using FinTrackWebApi.Services.DocumentService;
using FinTrackWebApi.Services.DocumentService.Generations.Account;
using FinTrackWebApi.Services.DocumentService.Generations.Budget;
using FinTrackWebApi.Services.DocumentService.Generations.Transaction;
using FinTrackWebApi.Services.EmailService;
using FinTrackWebApi.Services.MediaEncryptionService;
using FinTrackWebApi.Services.OtpService;
using FinTrackWebApi.Services.PaymentService;
using FinTrackWebApi.Services.SecureDebtService;
using System.Text.Json.Serialization;

namespace FinTrackWebApi.Extensions
{
    public static class ApplicationServicesExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                });

            services.Configure<CurrencyFreaksSettings>(configuration.GetSection("CurrencyFreaks"));
            services.AddMemoryCache();

            var currencyFreaksSettings = configuration.GetSection("CurrencyFreaks").Get<CurrencyFreaksSettings>();

            if (currencyFreaksSettings == null || string.IsNullOrWhiteSpace(currencyFreaksSettings.BaseUrl))
            {
                throw new InvalidOperationException("CurrencyFreaks BaseUrl is not configured in appsettings.json under the 'CurrencyFreaks' section.");
            }

            services.AddHttpClient("CurrencyFreaksClient", client =>
            {
                client.BaseAddress = new Uri(currencyFreaksSettings.BaseUrl);
            });


            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddTransient<PdfDocumentGenerator_Budget>();
            services.AddTransient<WordDocumentGenerator_Budget>();
            services.AddTransient<TextDocumentGenerator_Budget>();
            services.AddTransient<MarkdownDocumentGenerator_Budget>();
            services.AddTransient<XlsxDocumentGenerator_Budget>();
            services.AddTransient<XmlDocumentGenerator_Budget>();
            services.AddTransient<PdfDocumentGenerator_Transaction>();
            services.AddTransient<WordDocumentGenerator_Transaction>();
            services.AddTransient<TextDocumentGenerator_Transaction>();
            services.AddTransient<MarkdownDocumentGenerator_Transaction>();
            services.AddTransient<XlsxDocumentGenerator_Transaction>();
            services.AddTransient<XmlDocumentGenerator_Transaction>();
            services.AddTransient<PdfDocumentGenerator_Account>();
            services.AddTransient<WordDocumentGenerator_Account>();
            services.AddTransient<TextDocumentGenerator_Account>();
            services.AddTransient<MarkdownDocumentGenerator_Account>();
            services.AddTransient<XlsxDocumentGenerator_Account>();
            services.AddTransient<XmlDocumentGenerator_Account>();
            services.AddScoped<IDocumentGenerationService, DocumentGenerationService>();
            services.AddScoped<ICurrencyDataProvider, CurrencyFreaksProvider>();
            services.AddScoped<ICurrencyService, CurrencyCacheService>();
            services.AddScoped<ISecureDebtService, SecureDebtService>();
            services.AddScoped<IPaymentService, StripePaymentService>();
            services.AddScoped<IMediaEncryptionService, MediaEncryptionService>();
            services.AddScoped<IChatBotService, ChatBotService>();

            services.AddHostedService<DebtOverdueCheckerService>();

            services.AddHostedService<CurrencyUpdateService>();

            services.Configure<StripeSettings>(configuration.GetSection("StripeSettings"));

            services.AddHttpContextAccessor();

            return services;
        }
    }
}
