using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Bot.Builder.Community.Recognizers.Fuzzy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bot.Builder.Community.Components.Middleware.Multilingual.AzureTranslateService
{
    public class TranslateService : ITranslateService
    {
        private IList<LanguageInformation> _languageInformation;
        private IList<string> _languageList;

        private readonly ISetting _setting;

        readonly FuzzyRecognizer _fuzzyRecognizer;

        public string DefaultLanguageCode => _setting.DefaultLanguageCode;
        public double ScoreThreshold => _setting.ScoreThreshold;
        public bool IsMultilingualEnabled => _setting.IsMultilingualEnabled;
        
        public TranslateService(ISetting setting)
        {
            LoadLanguageInformation();
            this._setting = setting;
            _fuzzyRecognizer = new FuzzyRecognizer();
        }

        public async Task<FuzzyMatch> IsLanguageAvailable(string text)
        {
            var result = await _fuzzyRecognizer.Recognize(_languageList, text);
            if (result?.Matches?.Count > 0)
            {
                return result.Matches[0];
            }
            return null;
        }

        public LanguageInformation GetLanguageInformation(string languageName)
        {
            return _languageInformation.FirstOrDefault(item => string.Compare(item.Language, languageName, StringComparison.OrdinalIgnoreCase) == 0);
        }

        public async Task<string> TranslateText(string sourceLanguage, string targetLanguage, string text)
        {

            var translateText = text;

            var route = "/translate?api-version=3.0&from=" + sourceLanguage + "&to=" + targetLanguage;

            var body = new object[] { new { Text = text } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage())
                {
                    request.Method = HttpMethod.Post;
                    request.RequestUri = new Uri(_setting.Endpoint + route);
                    request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    request.Headers.Add("Ocp-Apim-Subscription-Key", _setting.Key);
                    request.Headers.Add("Ocp-Apim-Subscription-Region", _setting.Location);

                    var response = await client.SendAsync(request).ConfigureAwait(false);
                    var result = await response.Content.ReadAsStringAsync();
                    var result1 = JArray.Parse(result);

                    if (result1?.Count > 0)
                    {
                        var jToken = result1[0]["translations"];
                        if (jToken != null)
                        {
                            translateText = jToken[0]?["text"]?.ToString();
                        }
                    }
                }
            }

            return translateText;
        }

        private void LoadLanguageInformation()
        {
            _languageInformation = new List<LanguageInformation>
            {
                new LanguageInformation() { Language = "Afrikaans", LanguageCode = "af" },
                new LanguageInformation() { Language = "Albanian", LanguageCode = "sq" },
                new LanguageInformation() { Language = "Amharic", LanguageCode = "am" },
                new LanguageInformation() { Language = "Arabic", LanguageCode = "ar" },
                new LanguageInformation() { Language = "Armenian", LanguageCode = "hy" },
                new LanguageInformation() { Language = "Assamese", LanguageCode = "as" },
                new LanguageInformation() { Language = "Azerbaijani (Latin)", LanguageCode = "az" },
                new LanguageInformation() { Language = "Bangla", LanguageCode = "bn" },
                new LanguageInformation() { Language = "Bashkir", LanguageCode = "ba" },
                new LanguageInformation() { Language = "Basque", LanguageCode = "eu" },
                new LanguageInformation() { Language = "Bosnian (Latin)", LanguageCode = "bs" },
                new LanguageInformation() { Language = "Bulgarian", LanguageCode = "bg" },
                new LanguageInformation() { Language = "Cantonese (Traditional)", LanguageCode = "yue" },
                new LanguageInformation() { Language = "Catalan", LanguageCode = "ca" },
                new LanguageInformation() { Language = "Chinese (Literary)", LanguageCode = "lzh" },
                new LanguageInformation() { Language = "Chinese Simplified", LanguageCode = "zh-Hans" },
                new LanguageInformation() { Language = "Chinese Traditional", LanguageCode = "zh-Hant" },
                new LanguageInformation() { Language = "Croatian", LanguageCode = "hr" },
                new LanguageInformation() { Language = "Czech", LanguageCode = "cs" },
                new LanguageInformation() { Language = "Danish", LanguageCode = "da" },
                new LanguageInformation() { Language = "Dari", LanguageCode = "prs" },
                new LanguageInformation() { Language = "Divehi", LanguageCode = "dv" },
                new LanguageInformation() { Language = "Dutch", LanguageCode = "nl" },
                new LanguageInformation() { Language = "English", LanguageCode = "en" },
                new LanguageInformation() { Language = "Estonian", LanguageCode = "et" },
                new LanguageInformation() { Language = "Faroese", LanguageCode = "fo" },
                new LanguageInformation() { Language = "Fijian", LanguageCode = "fj" },
                new LanguageInformation() { Language = "Filipino", LanguageCode = "fil" },
                new LanguageInformation() { Language = "Finnish", LanguageCode = "fi" },
                new LanguageInformation() { Language = "French", LanguageCode = "fr" },
                new LanguageInformation() { Language = "French (Canada)", LanguageCode = "fr-ca" },
                new LanguageInformation() { Language = "Galician", LanguageCode = "gl" },
                new LanguageInformation() { Language = "Georgian", LanguageCode = "ka" },
                new LanguageInformation() { Language = "German", LanguageCode = "de" },
                new LanguageInformation() { Language = "Greek", LanguageCode = "el" },
                new LanguageInformation() { Language = "Gujarati", LanguageCode = "gu" },
                new LanguageInformation() { Language = "Haitian Creole", LanguageCode = "ht" },
                new LanguageInformation() { Language = "Hebrew", LanguageCode = "he" },
                new LanguageInformation() { Language = "Hindi", LanguageCode = "hi" },
                new LanguageInformation() { Language = "Hmong Daw (Latin)", LanguageCode = "mww" },
                new LanguageInformation() { Language = "Hungarian", LanguageCode = "hu" },
                new LanguageInformation() { Language = "Icelandic", LanguageCode = "is" },
                new LanguageInformation() { Language = "Indonesian", LanguageCode = "id" },
                new LanguageInformation() { Language = "Inuinnaqtun", LanguageCode = "ikt" },
                new LanguageInformation() { Language = "Inuktitut", LanguageCode = "iu" },
                new LanguageInformation() { Language = "Inuktitut (Latin)", LanguageCode = "iu-Latn" },
                new LanguageInformation() { Language = "Irish", LanguageCode = "ga" },
                new LanguageInformation() { Language = "Italian", LanguageCode = "it" },
                new LanguageInformation() { Language = "Japanese", LanguageCode = "ja" },
                new LanguageInformation() { Language = "Kannada", LanguageCode = "kn" },
                new LanguageInformation() { Language = "Kazakh", LanguageCode = "kk" },
                new LanguageInformation() { Language = "Khmer", LanguageCode = "km" },
                new LanguageInformation() { Language = "Klingon", LanguageCode = "tlh-Latn" },
                new LanguageInformation() { Language = "Klingon(plqaD)", LanguageCode = "tlh-Piqd" },
                new LanguageInformation() { Language = "Korean", LanguageCode = "ko" },
                new LanguageInformation() { Language = "Kurdish (Central)", LanguageCode = "ku" },
                new LanguageInformation() { Language = "Kurdish (Northern)", LanguageCode = "kmr" },
                new LanguageInformation() { Language = "Kyrgyz (Cyrillic)", LanguageCode = "ky" },
                new LanguageInformation() { Language = "Lao", LanguageCode = "lo" },
                new LanguageInformation() { Language = "Latvian", LanguageCode = "lv" },
                new LanguageInformation() { Language = "Lithuanian", LanguageCode = "lt" },
                new LanguageInformation() { Language = "Macedonian", LanguageCode = "mk" },
                new LanguageInformation() { Language = "Malagasy", LanguageCode = "mg" },
                new LanguageInformation() { Language = "Malay (Latin)", LanguageCode = "ms" },
                new LanguageInformation() { Language = "Malayalam", LanguageCode = "ml" },
                new LanguageInformation() { Language = "Maltese", LanguageCode = "mt" },
                new LanguageInformation() { Language = "Maori", LanguageCode = "mi" },
                new LanguageInformation() { Language = "Marathi", LanguageCode = "mr" },
                new LanguageInformation() { Language = "Mongolian (Cyrillic)", LanguageCode = "mn-Cyrl" },
                new LanguageInformation() { Language = "Mongolian (Traditional)", LanguageCode = "mn-Mong" },
                new LanguageInformation() { Language = "Myanmar", LanguageCode = "my" },
                new LanguageInformation() { Language = "Nepali", LanguageCode = "ne" },
                new LanguageInformation() { Language = "Norwegian", LanguageCode = "nb" },
                new LanguageInformation() { Language = "Odia", LanguageCode = "or" },
                new LanguageInformation() { Language = "Pashto", LanguageCode = "ps" },
                new LanguageInformation() { Language = "Persian", LanguageCode = "fa" },
                new LanguageInformation() { Language = "Polish", LanguageCode = "pl" },
                new LanguageInformation() { Language = "Portuguese (Brazil)", LanguageCode = "pt" },
                new LanguageInformation() { Language = "Portuguese (Portugal)", LanguageCode = "pt-pt" },
                new LanguageInformation() { Language = "Punjabi", LanguageCode = "pa" },
                new LanguageInformation() { Language = "Queretaro Otomi", LanguageCode = "otq" },
                new LanguageInformation() { Language = "Romanian", LanguageCode = "ro" },
                new LanguageInformation() { Language = "Russian", LanguageCode = "ru" },
                new LanguageInformation() { Language = "Samoan (Latin)", LanguageCode = "sm" },
                new LanguageInformation() { Language = "Serbian (Cyrillic)", LanguageCode = "sr-Cyrl" },
                new LanguageInformation() { Language = "Serbian (Latin)", LanguageCode = "sr-Latn" },
                new LanguageInformation() { Language = "Slovak", LanguageCode = "sk" },
                new LanguageInformation() { Language = "Slovenian", LanguageCode = "sl" },
                new LanguageInformation() { Language = "Somali (Arabic)", LanguageCode = "so" },
                new LanguageInformation() { Language = "Spanish", LanguageCode = "es" },
                new LanguageInformation() { Language = "Swahili (Latin)", LanguageCode = "sw" },
                new LanguageInformation() { Language = "Swedish", LanguageCode = "sv" },
                new LanguageInformation() { Language = "Tahitian", LanguageCode = "ty" },
                new LanguageInformation() { Language = "Tamil", LanguageCode = "ta" },
                new LanguageInformation() { Language = "Tatar (Latin)", LanguageCode = "tt" },
                new LanguageInformation() { Language = "Telugu", LanguageCode = "te" },
                new LanguageInformation() { Language = "Thai", LanguageCode = "th" },
                new LanguageInformation() { Language = "Tibetan", LanguageCode = "bo" },
                new LanguageInformation() { Language = "Tigrinya", LanguageCode = "ti" },
                new LanguageInformation() { Language = "Tongan", LanguageCode = "to" },
                new LanguageInformation() { Language = "Turkish", LanguageCode = "tr" },
                new LanguageInformation() { Language = "Turkmen (Latin)", LanguageCode = "tk" },
                new LanguageInformation() { Language = "Ukrainian", LanguageCode = "uk" },
                new LanguageInformation() { Language = "Upper Sorbian", LanguageCode = "hsb" },
                new LanguageInformation() { Language = "Urdu", LanguageCode = "ur" },
                new LanguageInformation() { Language = "Uyghur (Arabic)", LanguageCode = "ug" },
                new LanguageInformation() { Language = "Uzbek (Latin)", LanguageCode = "uz" },
                new LanguageInformation() { Language = "Vietnamese", LanguageCode = "vi" },
                new LanguageInformation() { Language = "Welsh", LanguageCode = "cy" },
                new LanguageInformation() { Language = "Yucatec Maya", LanguageCode = "yua" },
                new LanguageInformation() { Language = "Zulu", LanguageCode = "zu" }
            };

            _languageList = _languageInformation.Select(item => item.Language).ToList();
        }
    }
}
