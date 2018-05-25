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
using AlexaSoundboard.Helpers;
using AlexaSoundboard.SoundboardSkill.Utils;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Blob;
using Unsplasharp;

namespace AlexaSoundboard.SoundboardSkill
{
    public static class AlexaSoundboardSkill
    {
        private static TraceWriter _log;
        private static CloudBlobContainer _imageContainer;
        private static ICollector<string> _soundQueue;
        private static ICollector<string> _imageQueue;
        private static ICollector<string> _loggingQueue;

        [FunctionName("AlexaSoundboardSkill")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "alexa-soundboard")]
            HttpRequestMessage req,
            [Queue("soundsearch")]
            ICollector<string> soundQueue,
            [Queue("imagesearch")]
            ICollector<string> imageQueue,
            [Queue("logging")]
            ICollector<string> loggingQueue,
            [Blob("sounds")]
            CloudBlobContainer soundContainer,
            [Blob("images")]
            CloudBlobContainer imageContainer,
            TraceWriter log)
        {
            _log = log;
            _loggingQueue = loggingQueue;
            _imageContainer = imageContainer;

            _log.Info("Alexa Soundboard - Triggerd");

            // get skill request
            var skillRequest = await req.Content.ReadAsAsync<SkillRequest>();

            // check for launch request
            if (skillRequest?.Request is LaunchRequest)
                return req.CreateResponse(HttpStatusCode.OK, CreateRequestResponse("welcome", Statics.WelcomeMessage, false));

            // check for intent request
            if (skillRequest?.Request is IntentRequest intentRequest)
            {
                switch (intentRequest.Intent.Name)
                {
                    case Statics.AmazonStopIntent:
                        return req.CreateResponse(HttpStatusCode.OK, CreateRequestResponse("stop", Statics.StopMessage));
                    case Statics.AmazonHelpIntent:
                        return req.CreateResponse(HttpStatusCode.OK, CreateRequestResponse("help", Statics.HelpMessage, false));
                    case Statics.AmazonCancelIntent:
                        return req.CreateResponse(HttpStatusCode.OK, CreateRequestResponse("cancel", Statics.CancelMessage));
                    case Statics.SoundIntent:
                        _soundQueue = soundQueue;
                        _imageQueue = imageQueue;
                        var soundNames = soundContainer.ListBlobs().Cast<CloudBlockBlob>().Select(b => b.Name.Split('.').First());
                        return req.CreateResponse(HttpStatusCode.OK, HandleSoundIntent(intentRequest, soundNames));
                    case Statics.RandomSoundIntent:
                        var files = soundContainer.ListBlobs().Select(b => b.Uri.OriginalString);
                        return req.CreateResponse(HttpStatusCode.OK, HandleRandomSoundIntent(files));
                }
            }

            // return help intent if nothing suits
            return req.CreateResponse(HttpStatusCode.OK, CreateRequestResponse("help", Statics.HelpMessage, false));
        }

        /// <summary>
        /// Creates a response to handle the Sound Intent.
        /// </summary>
        private static SkillResponse HandleSoundIntent(IntentRequest intentRequest, IEnumerable<string> soundNames)
        {
            var slots = intentRequest.Intent.Slots;
            if (!slots.ContainsKey("sound"))
                return CreateRequestResponse("error", Statics.NoSoundProvidedMessage);

            var soundName = slots["sound"].Value;
            var soundFileName = soundName.AsFileName();

            if (IsSoundAvailable(soundFileName, soundNames))
            {
                _loggingQueue.Add($"AlexaSoundboardSkill - Playing: {soundName}");

                return CreateRequestResponse(soundFileName, string.Format(Statics.SoundMessage, soundFileName), false, true);
            }

            _soundQueue.Add(soundName);
            _imageQueue.Add(soundName);

            _loggingQueue.Add($"AlexaSoundboardSkill - Sound Not Found: {soundName}");

            return CreateRequestResponse("error", string.Format(Statics.SoundNotAvailableMessage, soundName), false);
        }

        /// <summary>
        /// Creates a response to handle the Random Sound Intent.
        /// </summary>
        private static SkillResponse HandleRandomSoundIntent(IEnumerable<string> files)
        {
            var soundFile = files.PickRandom();
            var imageName = soundFile.Split('/').Last().Split('.').First();

            _loggingQueue.Add($"AlexaSoundboardSkill - Playing: {imageName}");

            // get a picture
            var pictureUrl = GetPhoto(imageName);

            // return skill response
            return string.IsNullOrEmpty(pictureUrl)
                ? GetSkillResponse(string.Format(Statics.RandomSoundMessage, soundFile), false, useSsml: true)
                : GetSkillResponse(string.Format(Statics.RandomSoundMessage, soundFile), false, new StandardCard { Image = new CardImage { LargeImageUrl = pictureUrl, SmallImageUrl = pictureUrl } }, true);
        }

        private static string GetPhoto(string query)
        {
            var images = _imageContainer.ListBlobs().Cast<CloudBlockBlob>().ToList();
            return images.FirstOrDefault(i => i.Name.Split('.').First() == query)?.Uri?.OriginalString;
        }

        /// <summary>
        /// Checks if the sound is available.
        /// </summary>
        /// <param name="soundName">Sound to play</param>
        /// <param name="soundNames">List of available sounds in the blob storage</param>
        private static bool IsSoundAvailable(string soundName, IEnumerable<string> soundNames)
        {
            var isSoundAvailable = soundNames.Contains(soundName);

            _log.Info($"Alexa Soundboard - Sound Name: {soundName} | IsSoundAvailable: {isSoundAvailable}");

            return isSoundAvailable;
        }

        /// <summary>
        /// Creates a SkillResponse with a given message.
        /// </summary>
        /// <param name="searchTerm">Search query for the picture</param>
        /// <param name="message">Text which should be played</param>
        /// <param name="shouldEndSession">Indicates if the session should be closed</param>
        /// <param name="useSsml">Indicates if the text is using SSML or not</param>
        private static SkillResponse CreateRequestResponse(string searchTerm, string message, bool shouldEndSession = true, bool useSsml = false)
        {
            // get a picture
            var pictureUrl = GetPhoto(searchTerm);

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
