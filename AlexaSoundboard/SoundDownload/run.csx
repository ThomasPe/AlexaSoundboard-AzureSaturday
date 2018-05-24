#r "Microsoft.WindowsAzure.Storage"

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;

using System.Net;
using System.Diagnostics;
using System.IO;

public static void Run(string myQueueItem, IBinder binder, TraceWriter log)
{
    log.Info($"C# Queue trigger function processed: {myQueueItem}");
    
    string connectionString = GetEnvironmentVariable("alexasoundboard_STORAGE");
    log.Info(connectionString);
    var account = CloudStorageAccount.Parse(connectionString);
    var blobClient = account.CreateCloudBlobClient();
    var blobContainer = blobClient.GetContainerReference("sounds");
    // blobContainer.CreateIfNotExists();
    // var newBlockBlob = blobContainer.GetBlockBlobReference("test.mp3");
    // newBlockBlob.StartCopy(new Uri(myQueueItem));

}

public static string GetEnvironmentVariable(string name)
{
    return System.Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
}