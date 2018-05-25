using System;
using System.Net.Http;
using AlexaSoundboard.Helpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Blob;
using PixabaySharp;

namespace AlexaSoundboard.ImageDownload
{
    public static class ImageDownloadFunction
    {
        [FunctionName("ImageDownloadFunction")]
        public static async void Run(
            [QueueTrigger("imagesearch", Connection = "alexasoundboard_STORAGE")]
            string query,
            [Blob("images")]
            CloudBlobContainer imageContainer,
            TraceWriter log)
        {
            log.Info($"ImageDownloadFunction started with query: {query}");

            var apiKey = GetEnvironmentVariable("PixabayKey");
            var pixabayClient = new PixabaySharpClient(apiKey);
            var imageResult = await pixabayClient.SearchImagesAsync(query);
            var imageItem = imageResult.Images.PickRandom();

            var httpClient = new HttpClient();
            var imageBytes = await httpClient.GetByteArrayAsync(imageItem.LargeImageURL);

            var blob = imageContainer.GetBlockBlobReference($"{query.AsFileName()}.jpg");
            await blob.UploadFromByteArrayAsync(imageBytes, 0, imageBytes.Length);
        }


        /// <summary>
        /// Gets the enivronment variable from the settings.
        /// </summary>
        /// <param name="name">Name of the setting</param>
        private static string GetEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
    }
}
