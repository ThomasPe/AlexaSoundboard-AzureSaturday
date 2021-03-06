#r "Microsoft.WindowsAzure.Storage"

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;

using System.Net;
using System.Diagnostics;
using System.IO;

public static void Run(string myQueueItem, ICollector<string> logging, TraceWriter log)
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

    logging.Add("SoundDonwload: " + url);    
}

public static string GetEnvironmentVariable(string name)
{
    return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
}