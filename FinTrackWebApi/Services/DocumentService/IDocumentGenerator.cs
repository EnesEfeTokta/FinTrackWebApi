namespace FinTrackWebApi.Services.DocumentService
{
    public interface IDocumentGenerator
    {
        string FileExtension { get; }
        string MimeType { get; }
        Task<byte[]> GenerateAsync<TData>(TData data) where TData : class;
    }
}