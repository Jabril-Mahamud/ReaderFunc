using Supabase;
using Supabase.Storage;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

public class SupabaseStorageService : ISupabaseStorageService
{
    private readonly Supabase.Client _supabaseClient;
    private readonly string _bucketName;
    private readonly string _accessKey;   // New: Access Key
    private readonly string _secretKey;   // New: Secret Access Key

    // Update constructor to accept five arguments
    public SupabaseStorageService(string supabaseUrl, string apiKey, string accessKey, string secretKey, string bucketName)
    {
        var options = new SupabaseOptions
        {
            AutoConnectRealtime = true
        };
        _supabaseClient = new Supabase.Client(supabaseUrl, apiKey, options);
        _bucketName = bucketName;
        _accessKey = accessKey;    // Assigning access key
        _secretKey = secretKey;    // Assigning secret key
    }

    // Uploading audio as before
    public async Task<string> UploadAudioAsync(byte[] audioData, string fileName)
    {
        try
        {
            // Initialize the Supabase client
            await _supabaseClient.InitializeAsync();

            // Prepare the stream for upload
            using var stream = new MemoryStream(audioData);

            // Optionally add custom headers for access/secret keys (if supported by Supabase or custom backend)
            var requestHeaders = new HttpClient().DefaultRequestHeaders;
            requestHeaders.Add("X-Access-Key", _accessKey);
            requestHeaders.Add("X-Secret-Key", _secretKey);

            // Upload the file to Supabase storage
            var result = await _supabaseClient.Storage
                .From(_bucketName)
                .Upload(audioData, fileName, new Supabase.Storage.FileOptions { ContentType = "audio/mpeg" });

            // Generate the public URL for the uploaded file
            var publicUrl = _supabaseClient.Storage
                .From(_bucketName)
                .GetPublicUrl(result);

            return publicUrl;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to upload audio file: {ex.Message}", ex);
        }
    }

    public async Task<byte[]> GetAudioAsync(string fileName)
    {
        try
        {
            await _supabaseClient.InitializeAsync();
            var result = await _supabaseClient.Storage.From(_bucketName).Download(fileName, (TransformOptions?)null);
            return result;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to retrieve audio file: {ex.Message}", ex);
        }
    }

    public string GetPublicUrl(string fileName)
    {
        return _supabaseClient.Storage.From(_bucketName).GetPublicUrl(fileName);
    }
}
