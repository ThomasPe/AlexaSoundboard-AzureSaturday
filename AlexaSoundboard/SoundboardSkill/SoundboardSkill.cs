using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Alexa.NET.Response.Directive;
using Alexa.NET.Response.Directive.Templates;
using Alexa.NET.Response.Directive.Templates.Types;
using AlexaSoundboard.Utils;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Unsplasharp;

namespace AlexaSoundboard
{
    public static class SoundboardSkill
    {
        private static UnsplasharpClient _unsplasharpClient;

        [FunctionName("SoundboardSkill")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "alexa-soundboard")]
            HttpRequestMessage req,
            [Queue("soundsearch")]
            IAsyncCollector<string> soundSearchQueue,
            TraceWriter log)
        {
            log.Info("Alexa Soundboard - Triggerd");

            // create unsplash client to get images
            _unsplasharpClient = new UnsplasharpClient(ApiCredentials.UnsplashKey);

            // get skill request
            var skillRequest = await req.Content.ReadAsAsync<SkillRequest>();

            // check for launch request
            if (skillRequest?.Request is LaunchRequest)
                return req.CreateResponse(HttpStatusCode.OK, await CreateStaticRequestResponse("welcome", Statics.WelcomeMessage));

            // check for intent request
            if (skillRequest?.Request is IntentRequest intentRequest)
            {
                switch (intentRequest.Intent.Name)
                {
                    case Statics.AmazonStopIntent:
                        return req.CreateResponse(HttpStatusCode.OK, await CreateStaticRequestResponse("stop", Statics.StopMessage));
                    case Statics.AmazonHelpIntent:
                        return req.CreateResponse(HttpStatusCode.OK, await CreateStaticRequestResponse("help", Statics.HelpMessage));
                    case Statics.AmazonCancelIntent:
                        return req.CreateResponse(HttpStatusCode.OK, await CreateStaticRequestResponse("cancel", Statics.CancelMessage));
                }
            }

            return req.CreateResponse(HttpStatusCode.OK, skillRequest?.Request);
        }

        private static async Task<SkillResponse> CreateStaticRequestResponse(string searchTerm, string message)
        {
            // get a welcome picture
            var pictureUrl = await GetPhotoAsync(searchTerm);

            // return skill response
            return string.IsNullOrEmpty(pictureUrl) 
                ? GetSkillResponse(message, false) 
                : GetSkillResponse(message, false, new StandardCard { Image = new CardImage { LargeImageUrl = pictureUrl, SmallImageUrl = pictureUrl } });
        }

        private static async Task<string> GetPhotoAsync(string query)
        {
            var photo = await _unsplasharpClient.GetRandomPhoto(UnsplasharpClient.Orientation.Squarish, false, query: query);

            return photo.FirstOrDefault() != null 
                ? photo.First().Urls.Regular 
                : string.Empty;
        }


        private static SkillResponse GetSkillResponse(string outputSpeech, bool shouldEndSession, StandardCard card = null, bool useSsml = false)
        {
            var response = new ResponseBody
            {
                ShouldEndSession = shouldEndSession,

            };

            if (card != null)
            {
                response.Card = card;
                response.Directives = new List<IDirective>
                {
                    new DisplayRenderTemplateDirective
                    {
                        Template = new BodyTemplate1
                        {
                            BackgroundImage = new TemplateImage
                            {
                                ContentDescription = "Picture",
                                Sources = new List<ImageSource> {new ImageSource {Url = card.Image.LargeImageUrl}}
                            },
                        }
                    }
                };
            }

            if (useSsml)
                response.OutputSpeech = new SsmlOutputSpeech { Ssml = outputSpeech };
            else
                response.OutputSpeech = new PlainTextOutputSpeech { Text = outputSpeech };

            var skillResponse = new SkillResponse
            {
                Response = response,
                Version = "1.0"
            };

            return skillResponse;
        }
    }
}
