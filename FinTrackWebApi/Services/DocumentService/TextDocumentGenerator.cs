using System.Text;

namespace FinTrackWebApi.Services.DocumentService
{
    public class TextDocumentGenerator : IDocumentGenerator
    {
        public string FileExtension => ".txt";
        public string MimeType => "text/plain";

        public async Task<byte[]> GenerateAsync<TData>(TData data) where TData : class
        {
            // Veriyi düz metne çevirme mantığı
            // Örnek (ToString() veya özel formatlama):
            StringBuilder sb = new StringBuilder();
            if (data is BudgetReportModel reportData) // Örnek veri modeli
            {
                sb.AppendLine($"--- {reportData.ReportTitle} ---");
                sb.AppendLine($"Generated: {DateTime.Now}");
                sb.AppendLine("\nSummary:");
                sb.AppendLine(reportData.Description);
                // ... diğer veriler ...
                sb.AppendLine("\n--- End of Report ---");
            }
            else
            {
                sb.Append(data.ToString()); // Varsayılan ToString
            }

            // String'i UTF-8 byte dizisine çevir
            // Task.Run gerekmeyebilir.
            return await Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
        }
    }
}
