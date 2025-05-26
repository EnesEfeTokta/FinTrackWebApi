using FinTrackWebApi.Services.DocumentService.Generations;
using FinTrackWebApi.Services.DocumentService.Generations.Budget;

namespace FinTrackWebApi.Services.DocumentService
{
    public class DocumentGenerationService : IDocumentGenerationService
    {
        private readonly IServiceProvider _serviceProvider;

        public DocumentGenerationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<(byte[] Content, string MimeType, string FileName)> GenerateDocumentAsync<TData>(TData data, DocumentFormat format, string baseFileName) where TData : class
        {
            IDocumentGenerator generator = GetGenerator(format);

            byte[] content = await generator.GenerateAsync(data);
            string fileName = $"{baseFileName}{generator.FileExtension}";

            return (content, generator.MimeType, fileName);
        }

        private IDocumentGenerator GetGenerator(DocumentFormat format)
        {
            return format switch
            {
                DocumentFormat.Pdf => _serviceProvider.GetRequiredService<PdfDocumentGenerator_Budget>(),
                DocumentFormat.Word => _serviceProvider.GetRequiredService<WordDocumentGenerator_Budget>(),
                DocumentFormat.Text => _serviceProvider.GetRequiredService<TextDocumentGenerator_Budget>(),
                DocumentFormat.Markdown => _serviceProvider.GetRequiredService<MarkdownDocumentGenerator_Budget>(),
                DocumentFormat.Xml => _serviceProvider.GetRequiredService<XmlDocumentGenerator_Budget>(),
                DocumentFormat.Xlsx => _serviceProvider.GetRequiredService<XlsxDocumentGenerator_Budget>(),

                _ => throw new NotSupportedException($"Document format '{format}' is not supported.")
            };
        }
    }
}