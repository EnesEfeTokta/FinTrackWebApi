using System.Text;
using FinTrackWebApi.Services.DocumentService.Models;

namespace FinTrackWebApi.Services.DocumentService
{
    public class MarkdownDocumentGenerator : IDocumentGenerator
    {
        public string FileExtension => ".md";
        public string MimeType => "text/markdown";
        public Task<byte[]> GenerateAsync<TData>(TData data) where TData : class
        {
            if (!(data is BudgetReportModel reportData))
            {
                throw new ArgumentException($"Unsupported data type '{typeof(TData).FullName}' for Markdown generation. Expected BudgetReportModel.", nameof(data));
            }
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"# {reportData.ReportTitle}");
            sb.AppendLine($"**Description:** {reportData.Description}");
            sb.AppendLine();
            sb.AppendLine("## Budget Details");
            sb.AppendLine("| # | Name | Description | Category | Type | Start | End | Created | Updated | Allocated |");
            sb.AppendLine("|---|------|-------------|----------|------|-------|-----|---------|--------|-----------|");
            if (reportData.Items != null && reportData.Items.Any())
            {
                int index = 1;
                foreach (var item in reportData.Items)
                {
                    string allocatedStr = item.AllocatedAmount.ToString();
                    string updatedAtStr = item.UpdatedAt == DateTime.MinValue || item.UpdatedAt == default ? "-" : item.UpdatedAt.ToString("yyyy-MM-dd");
                    string name = Truncate(item.Name, 20);
                    string description = Truncate(item.Description, 25);
                    string category = Truncate(item.Category, 15);
                    sb.AppendLine($"| {index++} | {name} | {description} | {category} | {item.Type} | {item.StartDate:yyyy-MM-dd} | {item.EndDate:yyyy-MM-dd} | {item.CreatedAt:yyyy-MM-dd} | {updatedAtStr} | {allocatedStr} |");
                }
            }
            return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
        }
        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
                return value;
            return value.Substring(0, maxLength) + "...";
        }
    }
}
