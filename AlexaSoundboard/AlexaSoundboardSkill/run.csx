#r "Microsoft.WindowsAzure.Storage"

using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Net;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, ICollector<string> outputQueueItem, TraceWriter log)
{
    outputQueueItem.Add(DateTime.Now.ToLocalTime().ToString());
    
    return req.CreateResponse(HttpStatusCode.OK);
}
