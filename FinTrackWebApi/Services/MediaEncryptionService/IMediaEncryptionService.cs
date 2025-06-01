namespace FinTrackWebApi.Services.MediaEncryptionService
{
    public interface IMediaEncryptionService
    {
        string GenerateRandomKey(int length = 20);
        Task EncryptFileAsync(string inputFile, string outputFile, string password, string salt, string iv);
        Stream GetDecryptedVideoStream(string inputFile, string password, string salt, string iv);
        string GenerateSalt();
        string GenerateIV();
        string HashKey(string key);
    }
}