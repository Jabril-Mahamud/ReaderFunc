using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

public class TextToSpeechFunction
{
    private readonly ITtsService _ttsService;
    private readonly ISupabaseStorageService _storageService;

    public TextToSpeechFunction(ITtsService ttsService, ISupabaseStorageService storageService)
    {
        _ttsService = ttsService;
        _storageService = storageService;
    }

    [Function("GenerateAudio")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req,
        FunctionContext context)
    {
        var log = context.GetLogger("GenerateAudio");
        log.LogInformation("Processing a text-to-speech request.");

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        dynamic data;

        try
        {
            data = JsonConvert.DeserializeObject(requestBody);
        }
        catch (JsonException ex)
        {
            log.LogError($"JSON parsing error: {ex.Message}");
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Invalid JSON format.");
            return badRequestResponse;
        }

        string text = data?.text;
        if (string.IsNullOrEmpty(text))
        {
            log.LogWarning("Text input is null or empty.");
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Please provide text to convert to speech.");
            return badRequestResponse;
        }

        try
        {
            log.LogInformation($"Received text: {text}");

            // Get audio from ElevenLabs
            byte[] audioData = await _ttsService.GetTextToSpeechAsync(text);

            // Save audio to Supabase
            string fileName = $"{Guid.NewGuid()}.mp3"; // Generate a unique file name
            string audioPath = await _storageService.UploadAudioAsync(audioData, fileName);

            var okResponse = req.CreateResponse(HttpStatusCode.OK);
            await okResponse.WriteAsJsonAsync(new { audioPath });
            return okResponse;
        }
        catch (Exception ex)
        {
            log.LogError($"Error generating audio: {ex.Message}");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("Internal server error.");
            return errorResponse;
        }
    }
}
