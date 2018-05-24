#r "Microsoft.WindowsAzure.Storage"

using Microsoft.WindowsAzure.Storage.Blob;

using System.Diagnostics;
using System.IO;
using Systen.Net;


public static void Run(string myQueueItem, IBinder binder, TraceWriter log)
{
    log.Info($"C# Queue trigger function processed: {myQueueItem}");
    
    string connectionString = ConfigurationManager.AppSettings["alexasoundboard_STORAGE"];
    log.Info(connectionString);
    var account = CloudStorageAccount.Parse(connectionString);
    var blobClient = account.CreateCloudBlobClient();
    var blobContainer = blobClient.GetContainerReference("sounds");
    blobContainer.CreateIfNotExists();
    var newBlockBlob = blobContainer.GetBlockBlobReference("test.mp3");
    newBlockBlob.StartCopy(new Uri(myQueueItem));

}
