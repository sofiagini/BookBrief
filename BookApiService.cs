using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace BookBriefApp
{
    public class BookApiService
    {
        private readonly HttpClient _httpClient;

        public BookApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "CsharpBookApp");
        }

        public async Task<Book> SearchBookAsync(string title)
        {
            string encodedTitle = Uri.EscapeDataString(title);
            string searchUrl = $"https://openlibrary.org/search.json?title={encodedTitle}&limit=1";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(searchUrl);
                if (!response.IsSuccessStatusCode) return null;

                string jsonResponse = await response.Content.ReadAsStringAsync();
                JObject json = JObject.Parse(jsonResponse);

                if (json["numFound"] == null || json["numFound"].Value<int>() == 0)
                    return null;

                var doc = json["docs"]?[0];
                if (doc == null) return null;

                Book book = new Book
                {
                    Title = doc["title"]?.ToString() ?? "Без названия",
                    Author = doc["author_name"] != null ? string.Join(", ", doc["author_name"]) : "Автор неизвестен"
                };

                int editions = doc["edition_count"]?.Value<int>() ?? 0;
                book.Rating = editions > 40 ? "⭐ 4.7 / 5" : editions > 15 ? "⭐ 4.2 / 5" : "⭐ 3.8 / 5";

                string bookKey = doc["key"]?.ToString();

                if (!string.IsNullOrEmpty(bookKey))
                {
                    string detailsUrl = $"https://openlibrary.org{bookKey}.json";
                    HttpResponseMessage detailsResponse = await _httpClient.GetAsync(detailsUrl);

                    if (detailsResponse.IsSuccessStatusCode)
                    {
                        string detailsJson = await detailsResponse.Content.ReadAsStringAsync();
                        JObject details = JObject.Parse(detailsJson);

                        var descriptionToken = details["description"];
                        if (descriptionToken != null)
                        {
                            if (descriptionToken.Type == JTokenType.Object)
                            {
                                book.Description = descriptionToken["value"]?.ToString();
                            }
                            else
                            {
                                book.Description = descriptionToken.ToString();
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(book.Description))
                {
                    book.Description = doc["subject"] != null
                        ? "Краткое описание отсутствует. Основные темы книги:\n" + string.Join(", ", doc["subject"])
                        : "Описание книги отсутствует в открытой базе данных.";
                }

                // Перевод на русский, если описание нашлось
                if (!string.IsNullOrEmpty(book.Description) && book.Description != "Описание книги отсутствует в открытой базе данных.")
                {
                    book.Description = await TranslateToRussianAsync(book.Description);
                }

                return book;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Ошибка API: " + ex.Message);
                return null;
            }
        }

        private async Task<string> TranslateToRussianAsync(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            try
            {
                // Жестко ограничиваем до 450 символов, чтобы переводчик не ругался
                if (text.Length > 450) text = text.Substring(0, 450);

                string encodedText = Uri.EscapeDataString(text);
                string url = $"https://api.mymemory.translated.net/get?q={encodedText}&langpair=en|ru";

                string jsonResponse = await _httpClient.GetStringAsync(url);
                JObject json = JObject.Parse(jsonResponse);

                string translatedText = json["responseData"]?["translatedText"]?.ToString();

                return !string.IsNullOrEmpty(translatedText) ? translatedText : text;
            }
            catch
            {
                return text; // Если что-то пошло не так, просто покажем английский текст
            }
        }
    }
}
