using System.Net.Http;
using System.Text;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace AlexaSoundboard.Logging
{
    public static class Logging
    {
        [FunctionName("Logging")]
        public static void Run([QueueTrigger("logging", Connection = "alexasoundboard_STORAGE")]string myQueueItem, TraceWriter log)
        {
            log.Info($"C# Queue trigger function processed: {myQueueItem}");
            if (string.IsNullOrEmpty(myQueueItem))
            {
                myQueueItem = "Test with empty message";
            }
            var client = new HttpClient();
            client.PostAsync(
                "https://livelogging.azurewebsites.net/api/message", 
                new StringContent(myQueueItem, Encoding.UTF8, "application/json"));
        }
    }
}
