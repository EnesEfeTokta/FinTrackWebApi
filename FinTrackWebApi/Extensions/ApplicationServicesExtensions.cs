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
using System.Text.Json.Serialization;
using Stripe;

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
            services.AddHttpClient();

            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<PdfDocumentGenerator_Budget>();
            services.AddScoped<WordDocumentGenerator_Budget>();
            services.AddScoped<TextDocumentGenerator_Budget>();
            services.AddScoped<MarkdownDocumentGenerator_Budget>();
            services.AddScoped<XlsxDocumentGenerator_Budget>();
            services.AddScoped<XmlDocumentGenerator_Budget>();
            services.AddScoped<PdfDocumentGenerator_Transaction>();
            services.AddScoped<WordDocumentGenerator_Transaction>();
            services.AddScoped<TextDocumentGenerator_Transaction>();
            services.AddScoped<MarkdownDocumentGenerator_Transaction>();
            services.AddScoped<XlsxDocumentGenerator_Transaction>();
            services.AddScoped<XmlDocumentGenerator_Transaction>();
            services.AddScoped<IDocumentGenerationService, DocumentGenerationService>();
            services.AddScoped<ICurrencyDataProvider, CurrencyFreaksProvider>();
            services.AddScoped<ICurrencyService, CurrencyCacheService>();
            services.AddScoped<ISecureDebtService, SecureDebtService>();
            services.AddScoped<IPaymentService, StripePaymentService>();
            services.AddScoped<IMediaEncryptionService, MediaEncryptionService>();
            services.AddScoped<IChatBotService, ChatBotService>();

            services.AddHostedService<CurrencyUpdateService>();

            //services.Configure<StripeSettings>(services.Configuration.GetSection("StripeSettings"));

            services.AddHttpContextAccessor();

            return services;
        }
    }
}
