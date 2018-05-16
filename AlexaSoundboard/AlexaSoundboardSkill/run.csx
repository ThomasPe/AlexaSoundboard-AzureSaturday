#r "Microsoft.WindowsAzure.Storage"

using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Net;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, CloudQueueMessage myQueueItem, TraceWriter log)
{
    myQueueItem.Add(DateTime.Now.ToLocalTime());
}
