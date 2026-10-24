using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LogoCrmTelegramIntegration.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LogoCrmTelegramIntegration.Infrastructure.Services;

public class VoiceSpeechTranscriber : IVoiceSpeechTranscriber
{
	private readonly IConfiguration _configuration;

	private readonly ILogger<VoiceSpeechTranscriber> _logger;

	private static readonly HttpClient _httpClient = new HttpClient();

	public VoiceSpeechTranscriber(IConfiguration configuration, ILogger<VoiceSpeechTranscriber> logger)
	{
		_configuration = configuration;
		_logger = logger;
	}

	public async Task<string?> TranscribeAudioAsync(byte[] audioBytes, string fileName, CancellationToken ct = default(CancellationToken))
	{
		string text = _configuration["SpeechToText:ApiKey"] ?? _configuration["OpenAI:ApiKey"] ?? _configuration["Groq:ApiKey"];
		string text2 = _configuration["SpeechToText:Endpoint"];
		if (string.IsNullOrWhiteSpace(text2))
		{
			text2 = ((!string.IsNullOrWhiteSpace(_configuration["Groq:ApiKey"])) ? "https://api.groq.com/openai/v1/audio/transcriptions" : "https://api.openai.com/v1/audio/transcriptions");
		}
		if (string.IsNullOrWhiteSpace(text))
		{
			_logger.LogWarning("SpeechToText ApiKey henüz yapılandırılmadığı için ses kaydı metne dönüştürülemedi.");
			return null;
		}
		try
		{
			using MultipartFormDataContent content = new MultipartFormDataContent();
			ByteArrayContent byteArrayContent = new ByteArrayContent(audioBytes);
			byteArrayContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/ogg");
			content.Add(byteArrayContent, "file", fileName);
			string content2 = _configuration["SpeechToText:Model"] ?? "whisper-1";
			content.Add(new StringContent(content2), "model");
			content.Add(new StringContent("tr"), "language");
			using HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, text2);
			req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", text);
			req.Content = content;
			HttpResponseMessage resp = await _httpClient.SendAsync(req, ct);
			if (resp.IsSuccessStatusCode)
			{
				using JsonDocument jsonDocument = JsonDocument.Parse(await resp.Content.ReadAsStringAsync(ct));
				if (jsonDocument.RootElement.TryGetProperty("text", out var value))
				{
					string text3 = value.GetString();
					_logger.LogInformation("Ses kaydı başarıyla Türkçe metne dönüştürüldü: '{Text}'", text3);
					return text3;
				}
			}
			else
			{
				string text4 = await resp.Content.ReadAsStringAsync(ct);
				_logger.LogError("Speech-to-Text servisi hata döndürdü: [{Status}] {Body}", resp.StatusCode, text4);
			}
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Ses kaydı Türkçe metne dönüştürülürken hata oluştu.");
		}
		return null;
	}
}
