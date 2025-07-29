using FinTrackWebApi.Services.DocumentService.Generations;
using FinTrackWebApi.Services.DocumentService.Generations.Account;
using FinTrackWebApi.Services.DocumentService.Generations.Budget;
using FinTrackWebApi.Services.DocumentService.Generations.Transaction;

namespace FinTrackWebApi.Services.DocumentService
{
    public class DocumentGenerationService : IDocumentGenerationService
    {
        private readonly ILogger<DocumentGenerationService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public DocumentGenerationService(IServiceProvider serviceProvider, ILogger<DocumentGenerationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<(
            byte[] Content,
            string MimeType,
            string FileName
        )> GenerateDocumentAsync<TData>(
            TData data,
            DocumentFormat format,
            DocumentType type,
            string baseFileName
        )
            where TData : class
        {
            IDocumentGenerator generator = GetGenerator(format, type);

            byte[] content = await generator.GenerateAsync(data);
            string fileName = $"{baseFileName}{generator.FileExtension}";

            return (content, generator.MimeType, fileName);
        }

        private IDocumentGenerator GetGenerator(DocumentFormat format, DocumentType type)
        {
            switch (type)
            {
                case DocumentType.Budget:
                    return GetBudgetGenerator(format);
                case DocumentType.Transaction:
                    return GetTransactionGenerator(format);
                case DocumentType.Account:
                    return GetAccountGenerator(format);
                default:
                    throw new NotSupportedException($"Document type '{type}' is not supported.");
            }
        }

        private IDocumentGenerator GetAccountGenerator(DocumentFormat format)
        {
            return format switch
            {
                DocumentFormat.Pdf =>
                    _serviceProvider.GetRequiredService<PdfDocumentGenerator_Account>(),
                DocumentFormat.Word =>
                    _serviceProvider.GetRequiredService<WordDocumentGenerator_Account>(),
                DocumentFormat.Text =>
                    _serviceProvider.GetRequiredService<TextDocumentGenerator_Account>(),
                DocumentFormat.Markdown =>
                    _serviceProvider.GetRequiredService<MarkdownDocumentGenerator_Account>(),
                DocumentFormat.Xml =>
                    _serviceProvider.GetRequiredService<XmlDocumentGenerator_Account>(),
                DocumentFormat.Xlsx =>
                    _serviceProvider.GetRequiredService<XlsxDocumentGenerator_Account>(),
                _ => throw new NotSupportedException(
                    $"Document format '{format}' is not supported."
                ),
            };
        }

        private IDocumentGenerator GetBudgetGenerator(DocumentFormat format)
        {
            return format switch
            {
                DocumentFormat.Pdf =>
                    _serviceProvider.GetRequiredService<PdfDocumentGenerator_Budget>(),
                DocumentFormat.Word =>
                    _serviceProvider.GetRequiredService<WordDocumentGenerator_Budget>(),
                DocumentFormat.Text =>
                    _serviceProvider.GetRequiredService<TextDocumentGenerator_Budget>(),
                DocumentFormat.Markdown =>
                    _serviceProvider.GetRequiredService<MarkdownDocumentGenerator_Budget>(),
                DocumentFormat.Xml =>
                    _serviceProvider.GetRequiredService<XmlDocumentGenerator_Budget>(),
                DocumentFormat.Xlsx =>
                    _serviceProvider.GetRequiredService<XlsxDocumentGenerator_Budget>(),

                _ => throw new NotSupportedException(
                    $"Document format '{format}' is not supported."
                ),
            };
        }

        private IDocumentGenerator GetTransactionGenerator(DocumentFormat format)
        {
            return format switch
            {
                DocumentFormat.Pdf =>
                    _serviceProvider.GetRequiredService<PdfDocumentGenerator_Transaction>(),
                DocumentFormat.Word =>
                    _serviceProvider.GetRequiredService<WordDocumentGenerator_Transaction>(),
                DocumentFormat.Text =>
                    _serviceProvider.GetRequiredService<TextDocumentGenerator_Transaction>(),
                DocumentFormat.Markdown =>
                    _serviceProvider.GetRequiredService<MarkdownDocumentGenerator_Transaction>(),
                DocumentFormat.Xml =>
                    _serviceProvider.GetRequiredService<XmlDocumentGenerator_Transaction>(),
                DocumentFormat.Xlsx =>
                    _serviceProvider.GetRequiredService<XlsxDocumentGenerator_Transaction>(),

                _ => throw new NotSupportedException(
                    $"Document format '{format}' is not supported."
                ),
            };
        }
    }
}
