using FinTrackWebApi.Enums;

namespace FinTrackWebApi.Services.DocumentService
{
    public interface IDocumentGenerationService
    {
        Task<(byte[] Content, string MimeType, string FileName)> GenerateDocumentAsync<TData>(
            TData data,
            Enums.DocumentFormat format,
            DocumentType type,
            string baseFileName
        )
            where TData : class;
    }
}
