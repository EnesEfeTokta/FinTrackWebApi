
namespace FinTrackWebApi.Services.DocumentService
{
    public class DocumentGenerationService : IDocumentGenerationService
    {
        private readonly IServiceProvider _serviceProvider;
        // VEYA: Tüm IDocumentGenerator implementasyonlarını direkt enjekte edebilirsiniz
        // private readonly IEnumerable<IDocumentGenerator> _generators;

        // IServiceProvider kullanarak generator'ları anlık olarak çözmek daha esnek olabilir
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

        // Uygun generator'ı bulan yardımcı metod
        private IDocumentGenerator GetGenerator(DocumentFormat format)
        {
            // IServiceProvider kullanarak istenen implementasyonu al
            // Bu, her generator'ın DI'ye kaydedilmesini gerektirir (aşağıya bakın)
            return format switch
            {
                DocumentFormat.Pdf => _serviceProvider.GetRequiredService<PdfDocumentGenerator>(),
                DocumentFormat.Word => _serviceProvider.GetRequiredService<WordDocumentGenerator>(),
                DocumentFormat.Text => _serviceProvider.GetRequiredService<TextDocumentGenerator>(),
                // Gelecekteki formatlar...
                // DocumentFormat.Markdown => _serviceProvider.GetRequiredService<MarkdownDocumentGenerator>(),
                _ => throw new NotSupportedException($"Document format '{format}' is not supported.")
            };

            // Alternatif (Eğer IEnumerable<IDocumentGenerator> enjekte edildiyse):
            // return _generators.FirstOrDefault(g => g.GetType() == typeof(PdfDocumentGenerator)) // Daha iyi bir eşleştirme mekanizması gerekir (örn: enum veya attribute ile)
        }
    }
}
