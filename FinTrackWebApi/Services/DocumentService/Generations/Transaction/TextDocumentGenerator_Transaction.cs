using System.Text;
using FinTrackWebApi.Services.DocumentService.Models;

namespace FinTrackWebApi.Services.DocumentService.Generations.Transaction
{
    public class TextDocumentGenerator_Transaction : IDocumentGenerator
    {
        public string FileExtension => ".txt";
        public string MimeType => "text/plain";

        public Task<byte[]> GenerateAsync<TData>(TData data)
            where TData : class
        {
            if (!(data is TransactionsRaportModel reportData))
            {
                throw new ArgumentException(
                    $"Unsupported data type '{typeof(TData).FullName}' for Text generation. Expected TransactionsRaportModel.",
                    nameof(data)
                );
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"==================================================");
            sb.AppendLine($"          {reportData.ReportTitle}");
            sb.AppendLine($"==================================================");

            sb.AppendLine($"\nDescription:");
            sb.AppendLine(reportData.Description);
            sb.AppendLine();

            sb.AppendLine("Transaction Details:");

            sb.AppendLine(
                "--------------------------------------------------------------------------------------------------------------------------------------------------------------"
            );
            sb.AppendFormat(
                "| {0,-3} | {1,-20} | {2,-18} | {3,-12} | {4,-25} | {5,-10} |\n",
                "#",
                "Account Name",
                "Category",
                "Amount",
                "Description",
                "Transaction"
            );
            sb.AppendLine(
                "--------------------------------------------------------------------------------------------------------------------------------------------------------------"
            );

            if (reportData.Items != null && reportData.Items.Any())
            {
                int index = 1;
                foreach (var item in reportData.Items)
                {
                    string name = Truncate(item.AccountName, 20);
                    string description = Truncate(item.Description, 25);
                    string category = Truncate(item.CategoryName, 15);

                    sb.AppendFormat(
                        "| {0,-3} | {1,-20} | {2,-18} | {3,-12} | {4,-25} | {5,-10:yyyy-MM-dd} |\n",
                        index++,
                        name,
                        category,
                        item.Amount,
                        description,
                        item.TransactionDateUtc
                    );
                }
                sb.AppendLine(
                    "--------------------------------------------------------------------------------------------------------------------------------------------------------------"
                );
            }
            else
            {
                sb.AppendLine("No transaction details found.");
            }

            sb.AppendLine("\n--- End of Report ---");

            return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
        }

        private static string Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength - 3) + "...";
        }
    }
}
