public interface ISupabaseStorageService
{
    Task<string> UploadAudioAsync(byte[] audioData, string fileName);  // Changed to byte[] to match the implementation
    Task<byte[]> GetAudioAsync(string fileName);
}
