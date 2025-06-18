using FinTrackWebApi.Services.DocumentService.Models;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace FinTrackWebApi.Services.DocumentService.Generations.Transaction
{
    public class XmlDocumentGenerator_Transaction : IDocumentGenerator
    {
        public string FileExtension => ".xml";

        public string MimeType => "application/xml";

        public Task<byte[]> GenerateAsync<TData>(TData data) where TData : class
        {
            if (!(data is TransactionsRaportModel reportData))
            {
                throw new ArgumentException($"Unsupported data type '{typeof(TData).FullName}' for XML generation. Expected TransactionsRaportModel.", nameof(data));
            }

            var serializer = new XmlSerializer(typeof(TransactionsRaportModel));
            var settings = new XmlWriterSettings
            {
                Encoding = Encoding.UTF8,
                Indent = true,
                IndentChars = "  ",
                NewLineOnAttributes = false
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
