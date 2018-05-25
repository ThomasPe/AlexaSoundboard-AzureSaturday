#r "Microsoft.WindowsAzure.Storage"

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;

using System.Net;
using System.Diagnostics;
using System.IO;

public static void Run(string myQueueItem, IBinder binder, TraceWriter log)
{
    log.Info($"SoundDonwload started: {myQueueItem}");
    string url = myQueueItem.Split(';').First();
    log.Info(url);
    string name = myQueueItem.Split(';').Last();
    log.Info(name);
    string filename = System.IO.Path.GetFileName(url);
    log.Info(filename);
    string connectionString = GetEnvironmentVariable("alexasoundboard_STORAGE");
    log.Info(connectionString);
    var account = CloudStorageAccount.Parse(connectionString);
    var blobClient = account.CreateCloudBlobClient();
    var blobContainer = blobClient.GetContainerReference("sounds-new");
    blobContainer.CreateIfNotExists();
    var newBlockBlob = blobContainer.GetBlockBlobReference(name + ".mp3");
    newBlockBlob.StartCopy(new Uri(url));


    // Notify Twitter 
    var twitterAppUri = "https://prod-46.westeurope.logic.azure.com:443/workflows/62e19b15ab7e4354a0745e4272875e11/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=oFNsoHMDaTKs_vxbi1G_UrgL8nMmgiOEE49NwiKWtBg";
    var message = $"{{\"message\":\"I have found a new sound! {name}.mp3\"}}";
    using (var client = new HttpClient())
    {
        var response = client.PostAsync(twitterAppUri, new StringContent(message, Encoding.UTF8, "application/json")).Result;
    }
}

public static string GetEnvironmentVariable(string name)
{
    return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
}