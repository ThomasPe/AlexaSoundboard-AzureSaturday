using System;
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
using AlexaSoundboard.SoundboardSkill.Extensions;
using AlexaSoundboard.SoundboardSkill.Models;
using AlexaSoundboard.SoundboardSkill.Utils;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Unsplasharp;

namespace AlexaSoundboard.SoundboardSkill
{
    public static class AlexaSoundboardSkill
    {
        private static UnsplasharpClient _unsplasharpClient;
        private static HttpClient _httpClient;
        private static TraceWriter _log;
        private static IAsyncCollector<string> _soundSearchQueue;

        [FunctionName("AlexaSoundboardSkill")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "alexa-soundboard")]
            HttpRequestMessage req,
            [Queue("soundsearch")]
            IAsyncCollector<string> soundSearchQueue,
            [Blob("sounds")]
            CloudBlobContainer blobContainer,
            TraceWriter log)
        {
            _log = log;

            _log.Info("Alexa Soundboard - Triggerd");

            // create unsplash client to get images
            var apiKey = GetEnvironmentVariable("UnsplashKey");
            _unsplasharpClient = new UnsplasharpClient(apiKey);

            // get skill request
            var skillRequest = await req.Content.ReadAsAsync<SkillRequest>();

            // check for launch request
            if (skillRequest?.Request is LaunchRequest)
                return req.CreateResponse(HttpStatusCode.OK, await CreateRequestResponse("welcome", Statics.WelcomeMessage, false));

            // check for intent request
            if (skillRequest?.Request is IntentRequest intentRequest)
            {
                switch (intentRequest.Intent.Name)
                {
                    case Statics.AmazonStopIntent:
                        return req.CreateResponse(HttpStatusCode.OK, await CreateRequestResponse("stop", Statics.StopMessage));
                    case Statics.AmazonHelpIntent:
                        return req.CreateResponse(HttpStatusCode.OK, await CreateRequestResponse("help", Statics.HelpMessage, false));
                    case Statics.AmazonCancelIntent:
                        return req.CreateResponse(HttpStatusCode.OK, await CreateRequestResponse("cancel", Statics.CancelMessage));
                    case Statics.SoundIntent:
                        _soundSearchQueue = soundSearchQueue;
                        return req.CreateResponse(HttpStatusCode.OK, await HandleSoundIntentAsync(intentRequest));
                    case Statics.RandomSoundIntent:
                        var files = blobContainer.ListBlobs().Select(b => b.Uri.OriginalString);
                        return req.CreateResponse(HttpStatusCode.OK, await HandleRandomSoundIntentAsync(files));
                }
            }

            // return help intent if nothing suits
            return req.CreateResponse(HttpStatusCode.OK, await CreateRequestResponse("help", Statics.HelpMessage));
        }

        /// <summary>
        /// Creates a response to handle the Sound Intent.
        /// </summary>
        private static async Task<SkillResponse> HandleSoundIntentAsync(IntentRequest intentRequest)
        {
            var slots = intentRequest.Intent.Slots;
            if (!slots.ContainsKey("sound"))
                return await CreateRequestResponse("error", Statics.NoSoundProvidedMessage);

            var soundName = slots["sound"].Value;
            var soundFileName = soundName.AsFileName();

            if (await IsSoundAvailableAsync(soundFileName))
                return await CreateRequestResponse(soundFileName, string.Format(Statics.SoundMessage, soundFileName), useSsml: true);

            await _soundSearchQueue.AddAsync(soundName);

            //var mailAddress = await GetUserMailAddressAsync(_accessToken);
            //_log.Info($"Alexa Soundboard - Mail: {mailAddress}");

            return await CreateRequestResponse("error", Statics.SoundNotAvailableMessage);
        }

        /// <summary>
        /// Creates a response to handle the Random Sound Intent.
        /// </summary>
        private static async Task<SkillResponse> HandleRandomSoundIntentAsync(IEnumerable<string> files)
        {
            // get a picture
            var pictureUrl = await GetPhotoAsync("random");

            // return skill response
            return string.IsNullOrEmpty(pictureUrl)
                ? GetSkillResponse(string.Format(Statics.RandomSoundMessage, files.PickRandom()), false, useSsml: true)
                : GetSkillResponse(string.Format(Statics.RandomSoundMessage, files.PickRandom()), false, new StandardCard { Image = new CardImage { LargeImageUrl = pictureUrl, SmallImageUrl = pictureUrl } }, true);
        }

        /// <summary>
        /// Gets a random photo from unsplash.com
        /// </summary>
        /// <param name="query">Search query</param>
        private static async Task<string> GetPhotoAsync(string query)
        {
            var photo = await _unsplasharpClient.GetRandomPhoto(UnsplasharpClient.Orientation.Squarish, false, query: query);

            return photo.FirstOrDefault() != null
                ? photo.First().Urls.Regular
                : string.Empty;
        }

        /// <summary>
        /// Checks if the sound is available.
        /// </summary>
        /// <param name="soundName">Sound to play</param>
        private static async Task<bool> IsSoundAvailableAsync(string soundName)
        {
            if (_httpClient == null)
                _httpClient = new HttpClient();

            var response = await _httpClient.GetAsync(string.Format(Statics.SoundUrl, soundName));

            _log.Info($"Alexa Soundboard - Sound Name: {soundName} | IsSoundAvailable: {response.IsSuccessStatusCode}");

            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Gets the enivronment variable from the settings.
        /// </summary>
        /// <param name="name">Name of the setting</param>
        private static string GetEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }

        /// <summary>
        /// Creates a SkillResponse with a given message.
        /// </summary>
        /// <param name="searchTerm">Search query for the picture</param>
        /// <param name="message">Text which should be played</param>
        /// <param name="shouldEndSession">Indicates if the session should be closed</param>
        /// <param name="useSsml">Indicates if the text is using SSML or not</param>
        private static async Task<SkillResponse> CreateRequestResponse(string searchTerm, string message, bool shouldEndSession = true, bool useSsml = false)
        {
            // get a picture
            var pictureUrl = await GetPhotoAsync(searchTerm);

            // return skill response
            return string.IsNullOrEmpty(pictureUrl)
                ? GetSkillResponse(message, shouldEndSession, useSsml: useSsml)
                : GetSkillResponse(message, shouldEndSession, new StandardCard { Image = new CardImage { LargeImageUrl = pictureUrl, SmallImageUrl = pictureUrl } }, useSsml);
        }

        /// <summary>
        /// Creates the Skill Response which will be returned by the Azure Function.
        /// </summary>
        /// <param name="outputSpeech">Text which should be played</param>
        /// <param name="shouldEndSession">Indicated if the session should be ended</param>
        /// <param name="card">Card which will be shown in the companion app</param>
        /// <param name="useSsml">Indicates if the text is using SSML or not</param>
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
