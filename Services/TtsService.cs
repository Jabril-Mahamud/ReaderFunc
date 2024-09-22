using Newtonsoft.Json;
using System.Text;

namespace ReaderFunc.Services;

public class TtsService : ITtsService
{
    private readonly HttpClient _httpClient;

    public TtsService(string apiKey)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.elevenlabs.io/")
        };
        _httpClient.DefaultRequestHeaders.Add("xi-api-key", apiKey);
    }

    public async Task<byte[]> GetTextToSpeechAsync(string text)
    {
        var requestUrl = "v1/text-to-speech/21m00Tcm4TlvDq8ikWAM"; // Adjust voice ID as necessary
        var requestBody = new
        {
            text,
            voice_settings = new
            {
                stability = 0.5,
                similarity_boost = 0.5
            }
        };
        var requestContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(requestUrl, requestContent);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to get TTS from ElevenLabs: {response.StatusCode}, Details: {errorContent}");
        }

        return await response.Content.ReadAsByteArrayAsync();
    }
}