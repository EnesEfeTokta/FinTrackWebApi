using FinTrackWebApi.Services.DocumentService.Models;
using System.Text;

namespace FinTrackWebApi.Services.DocumentService.Generations.Transaction
{
    public class MarkdownDocumentGenerator_Transaction : IDocumentGenerator
    {
        public string FileExtension => ".md";

        public string MimeType => "text/markdown";

        public Task<byte[]> GenerateAsync<TData>(TData data) where TData : class
        {
            if (!(data is TransactionsRaportModel reportData))
            {
                throw new ArgumentException($"Unsupported data type '{typeof(TData).FullName}' for Markdown generation. Expected TransactionsRaportModel.", nameof(data));
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"# {reportData.ReportTitle}");
            sb.AppendLine($"**Description:** {reportData.Description}");
            sb.AppendLine();
            sb.AppendLine("## Transaction Details");
            sb.AppendLine("| # | Account Name | Category | Amount | Description | Transaction |");
            sb.AppendLine("|:---|:---|:---|:---|:---|:---|");

            if (reportData.Items != null && reportData.Items.Any())
            {
                int index = 1;
                foreach (var item in reportData.Items)
                {
                    string name = Truncate(item.AccountName, 20);
                    string description = Truncate(item.Description, 25);
                    string category = Truncate(item.CategoryName, 15);
                    sb.AppendLine($"| {index++} | {name} | {category} | {item.Amount} | {description} | {item.TransactionDateUtc} |");
                }
            }

            sb.AppendLine("---");
            sb.AppendLine($"**TotalCount:** {reportData.TotalCount}");

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