using FinTrackWebApi.Services.DocumentService.Models;
using System.Text;

namespace FinTrackWebApi.Services.DocumentService.Generations.Account
{
    public class MarkdownDocumentGenerator_Account : IDocumentGenerator
    {
        public string FileExtension => ".md";
        public string MimeType => "text/markdown";

        public Task<byte[]> GenerateAsync<TData>(TData data)
            where TData : class
        {
            if (!(data is AccountReportModel accountReport))
            {
                throw new ArgumentException(
                    $"Unsupported data type '{typeof(TData).FullName}' for Markdown generation. Expected AccountReportModel.",
                    nameof(data)
                );
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"# {accountReport.ReportTitle}");
            sb.AppendLine($"**Description:** {accountReport.Description}");
            sb.AppendLine();
            sb.AppendLine("## Account Details");
            sb.AppendLine(
                "| # | Name | Type | Created | Updated | Balance |"
            );
            sb.AppendLine(
                "|---|------|----------|---------|--------|-----------|"
            );

            if (accountReport.Items != null && accountReport.Items.Any())
            {
                int index = 1;
                foreach (var item in accountReport.Items)
                {
                    string balanceStr = item.Balance.ToString();
                    string updatedAtStr =
                        item.UpdatedAt == DateTime.MinValue || item.UpdatedAt == default
                            ? "-"
                            : item.UpdatedAt.ToString("yyyy-MM-dd");
                    string name = Truncate(item.Name, 20);
                    string category = Truncate(item.Type?.ToString() ?? "N/A", 15);
                    sb.AppendLine(
                        $"| {index++} | {name} | {category} | {item.CreatedAt:yyyy-MM-dd} | {updatedAtStr} | {balanceStr} |"
                    );
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
