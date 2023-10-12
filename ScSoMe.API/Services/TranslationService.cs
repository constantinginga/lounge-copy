using DetectLanguage;
using ScSoMe.EF;
using System.Text.RegularExpressions;
using TranslatorService;

namespace ScSoMe.API.Services
{
    public class TranslationService
    {
        private static readonly string key = "7769a508f2d44c10b3724c08397ef58a"; //remember to change the key when expired
        private static readonly string endpoint = "https://api.cognitive.microsofttranslator.com/";
        private static readonly string location = "westeurope";
        private TranslatorClient translatorClient;
        private DetectLanguageClient detectClient;
        private readonly ScSoMeContext db;



        public TranslationService()
        {
            translatorClient = new TranslatorClient(key, location);
            detectClient = new DetectLanguageClient("1a47501667a568f5389535e149ac74ab"); 
            db = new ScSoMeContext();
        }


        //public async Task<string> TranslateToEn(string post)
        //{
        //    string route = "/translate?api-version=3.0&to=en";
        //    object[] body = new object[] { new { Text = post } };
        //    var requestBody = JsonConvert.SerializeObject(body);
        //    using (var client = new HttpClient())
        //    using (var request = new HttpRequestMessage())
        //    {
        //        // Build the request.
        //        request.Method = HttpMethod.Post;
        //        request.RequestUri = new Uri(endpoint + route);
        //        request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
        //        request.Headers.Add("Ocp-Apim-Subscription-Key", key);
        //        request.Headers.Add("Ocp-Apim-Subscription-Region", location);

        //        // Send the request and get response.
        //        HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
        //        // Read response as a string.
        //        string result = await response.Content.ReadAsStringAsync();
        //        Console.WriteLine(result);
        //        return result;
        //    }

        //}

        public async Task<string> TranslatePost(long commentId)
        {
            try
            {
                var translation = db.Translations.Where( x=> x.CommentId == commentId ).FirstOrDefault();
                if (translation == null)
                {
                    var comment = db.Comments.Where( x=> x.CommentId == commentId ).FirstOrDefault();
                    string lang = await DetectLanguage(comment.Text);

                    if (lang.Equals("da"))
                    {


                        var result = await translatorClient.TranslateAsync(comment.Text, to: "en");
                        string translatedTo = result.Translation.To;
                        await db.Translations.AddAsync(new Translation
                        {
                            CommentId = commentId,
                            LanguageCode = translatedTo,
                            TranslatedComment = result.Translation.Text
                        });
                        await db.SaveChangesAsync();

                        return result.Translation.Text;


                    }
                    else if (lang.Equals("en"))
                    {
                        var result = await translatorClient.TranslateAsync(comment.Text, to: "da");
                        string translatedTo = result.Translation.To;
                        await db.Translations.AddAsync(new Translation
                        {
                            CommentId = commentId,
                            LanguageCode = translatedTo,
                            TranslatedComment = result.Translation.Text
                        });
                        await db.SaveChangesAsync();
                        return result.Translation.Text;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return translation.TranslatedComment;

                }
            } 
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> DetectLanguage(string comment)
        {
            string clearText = Regex.Replace(comment, "<.*?>", string.Empty);
            string languageCode = await detectClient.DetectCodeAsync(clearText);
            return languageCode;
        }

        public async Task DeleteTranslation(long commentId)
        {
            try
            {
                var translation = db.Translations.Where(t => t.CommentId == commentId).FirstOrDefault();
                if (translation != null)
                {
                    db.Translations.Remove(translation);
                    await db.SaveChangesAsync();
                }

            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
