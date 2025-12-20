using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SporSalonuYonetim.Models;
using Microsoft.Extensions.Configuration;

namespace SporSalonuYonetim.Services
{
    public class GeminiFitnessService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        // EN KRİTİK NOKTA: ÇALIŞAN MODEL
        private const string Model = "gemini-2.5-flash";

        public GeminiFitnessService(IConfiguration configuration, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Gemini:ApiKey"] ?? "";

            if (!string.IsNullOrWhiteSpace(_apiKey))
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("x-goog-api-key", _apiKey);
            }
        }

        public async Task<string> GetFitnessPlanAsync(FitnessAiRequestViewModel req)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                return "Gemini API anahtarı bulunamadı.";

            var prompt = $@"
Kullanıcının bilgilerine dayanarak fitness + beslenme programı öner.
TÜRKÇE yaz.

Bilgiler:
- Cinsiyet: {req.Gender}
- Yaş: {req.Age}
- Boy: {req.HeightCm} cm
- Kilo: {req.WeightKg} kg
- Hedef: {req.Goal}
- Aktivite Seviyesi: {req.ActivityLevel}
- Ek Bilgi: {req.AdditionalInfo}

Çıktıyı şu başlıklarla yaz:
1) Antrenman
2) Beslenme
3) Tavsiyeler
";

            var body = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{Model}:generateContent";

            var response = await _httpClient.PostAsync(url, content);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return $"Gemini API hatası: {response.StatusCode}\n{result}";

            try
            {
                using var doc = JsonDocument.Parse(result);
                var root = doc.RootElement;

                var text = root
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (string.IsNullOrWhiteSpace(text))
                    return "Gemini yanıt döndü ama metin alınamadı.";

                return text.Trim().Replace("\n", "<br />");
            }
            catch
            {
                return "Gemini yanıtı beklenen formatta değildi.";
            }
        }
    }
}
