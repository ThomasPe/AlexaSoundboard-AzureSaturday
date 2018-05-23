open System.IO

let GetBlob (storageAcc:string) containerName blobName =
    let connString = ConfigurationManager.AppSettings.[storageAcc]
    let storageAccount = CloudStorageAccount.Parse(connString)
    let blobClient = storageAccount.CreateCloudBlobClient()
    let container = blobClient.GetContainerReference(containerName)
    container.GetBlockBlobReference(blobName)

let Run(inputMessage: string, log: TraceWriter) =
    log.Info(sprintf "F# Queue trigger function processed: '%s'" inputMessage)

    let client = new  HttpClient()
    let! response = client.GetByteArrayAsync(inputMessage)                         
                    |> Async.AwaitTask   

    let cloudBlockBlob = GetBlob "alexasoundboard_STORAGE" "sounds-new" "test.mp3" 
    let bytes2 = Encoding.UTF8.GetBytes("Using CloudBlockBlob directly")
    log.Verbose("uploading text...")
    cloudBlockBlob.UploadFromByteArrayAsync(bytes2, 0, bytes2.Length).Wait()
    log.Verbose("text uploaded")
    cloudBlockBlob.Metadata.["From"] <- "Mark Heath"
    cloudBlockBlob.SetMetadata();
    log.Verbose("metadata set")