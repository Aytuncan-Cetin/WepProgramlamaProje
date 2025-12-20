using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace SporSalonuYonetim.Services
{
    public class GeminiAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        // Diğer projende de çalışan model
        private const string Model = "gemini-2.5-flash";

        public GeminiAiService(IConfiguration configuration, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Gemini:ApiKey"] ?? "";

            if (!string.IsNullOrWhiteSpace(_apiKey))
            {
                _httpClient.DefaultRequestHeaders.Clear();
                // Öteki projende olduğu gibi KEY'i header'a koyuyoruz
                _httpClient.DefaultRequestHeaders.Add("x-goog-api-key", _apiKey);
            }
        }

        // YapayZekaController’ın beklediği imza
        public async Task<string> EgzersizOnerisiOlustur(string boy, string kilo, string hedef, string? tip)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                return "Gemini API anahtarı bulunamadı (appsettings.Development.json içindeki 'Gemini:ApiKey'i kontrol et).";

            string prompt = $@"
Sen profesyonel bir fitness antrenörü ve diyetisyensin.

Kullanıcı bilgileri:
- Boy: {boy} cm
- Kilo: {kilo} kg
- Hedef: {hedef}
- Vücut Tipi: {tip}

Bu kişiye özel:
1) Genel değerlendirme (VKİ yorumuna benzer),
2) Haftalık detaylı egzersiz programı (gün gün, madde madde),
3) Beslenme / makro önerileri,
4) Dikkat etmesi gerekenler.

Cevabı Türkçe ve okunaklı biçimde, madde madde yaz.
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

            string json = JsonSerializer.Serialize(body);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Çalışan endpoint yapısı (diğer projendeki gibi)
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/{Model}:generateContent";

            var response = await _httpClient.PostAsync(url, content);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // Hata detayını kullanıcıya gösterelim, sunumda işine yarar
                return $"Gemini isteği başarısız oldu. Kod: {(int)response.StatusCode}\nDetay: {responseText}";
            }

            try
            {
                using var doc = JsonDocument.Parse(responseText);
                var root = doc.RootElement;

                var text = root
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                return string.IsNullOrWhiteSpace(text)
                    ? "Gemini yanıt döndürdü ancak metin boş geldi."
                    : text.Trim();
            }
            catch
            {
                return "Gemini yanıtı beklenen formatta değil:\n" + responseText;
            }
        }
    }
}
