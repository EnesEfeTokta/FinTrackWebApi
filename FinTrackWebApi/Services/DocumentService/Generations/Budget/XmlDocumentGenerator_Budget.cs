using System.Text;
using System.Xml;
using System.Xml.Serialization;
using FinTrackWebApi.Services.DocumentService.Models;

namespace FinTrackWebApi.Services.DocumentService.Generations.Budget
{
    public class XmlDocumentGenerator_Budget : IDocumentGenerator
    {
        public string FileExtension => ".xml";
        public string MimeType => "application/xml";

        public Task<byte[]> GenerateAsync<TData>(TData data)
            where TData : class
        {
            if (!(data is BudgetReportModel reportData))
            {
                throw new ArgumentException(
                    $"Unsupported data type '{typeof(TData).FullName}' for XML generation. Expected BudgetReportModel.",
                    nameof(data)
                );
            }

            var serializer = new XmlSerializer(typeof(BudgetReportModel));
            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true,
                IndentChars = "  ",
                NewLineOnAttributes = false,
            };

            byte[] resultBytes;

            using (var memoryStream = new MemoryStream())
            {
                using (var xmlWriter = XmlWriter.Create(memoryStream, settings))
                {
                    var namespaces = new XmlSerializerNamespaces();
                    namespaces.Add(string.Empty, string.Empty);
                    serializer.Serialize(xmlWriter, reportData, namespaces);
                }

                resultBytes = memoryStream.ToArray();
            }

            return Task.FromResult(resultBytes);
        }
    }
}
