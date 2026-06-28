using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SPL;

public static class GeminiService
{
   
    private const string ApiKey = "AQ.Ab8RN6Jak10z5MrET3Vc6TA9Ve4n412bNIJgWKtHR6T1wo-LdA";

    private static readonly HttpClient client = new HttpClient();

    public static async Task<string> TanyaGeminiAsync(string pertanyaan, string konteksTambahan = "")
    {
        try
        {
            string prompt = $"""
            Kamu adalah chatbot AI untuk sistem prediksi risiko tanah longsor.
            Jawab menggunakan Bahasa Indonesia, singkat, jelas, dan mudah dipahami mahasiswa.

            Konteks sistem:
            - Parameter: curah hujan, kelembapan tanah, kemiringan lereng, jenis tanah.
            - Sistem menggunakan Decision Tree, Fuzzy Logic, OpenCV Face Recognition, dan dashboard monitoring.
            - Fokus jawaban pada mitigasi longsor, prediksi risiko, parameter lingkungan, dan laporan masyarakat.

            Data/konteks tambahan:
            {konteksTambahan}

            Pertanyaan user:
            {pertanyaan}
            """;

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                }
            };

            string json = JsonSerializer.Serialize(requestBody);

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent");

            request.Headers.Add("x-goog-api-key", ApiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using HttpResponseMessage response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return $"Gemini API gagal diakses. Status: {response.StatusCode}";
            }

            string result = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(result);

            string? text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return string.IsNullOrWhiteSpace(text)
                ? "Gemini tidak memberikan jawaban."
                : text;
        }
        catch (Exception ex)
        {
            return $"Terjadi error Gemini: {ex.Message}";
        }
    }
}