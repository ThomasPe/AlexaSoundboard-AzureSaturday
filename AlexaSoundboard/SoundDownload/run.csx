#r "Microsoft.WindowsAzure.Storage"

using Microsoft.WindowsAzure.Storage.Blob;

using System.Diagnostics;
using System.IO;
using Systen.Net;


public static void Run(string myQueueItem, IBinder binder, TraceWriter log)
{
    log.Info($"C# Queue trigger function processed: {myQueueItem}");
    byte[] result = null;

    using (WebClient webClient = new WebClient())
    {
        result = webClient.DownloadData(myQueueItem);
    }
    var attributes = new Attribute[]
    {
        new BlobAttribute("sounds/neindochoh.mp3"),
        new StorageAccountAttribute("alexasoundboard_STORAGE")
    };
    var writer = await binder.BindAsync<CloudBlockBlob>(attributes);
    await writer.UploadFromByteArrayAsync(result, 0, result.Length);

}
