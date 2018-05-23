open System.IO

let GetBlob (storageAcc:string) containerName blobName =
    let connString = ConfigurationManager.AppSettings.[storageAcc]
    let storageAccount = CloudStorageAccount.Parse(connString)
    let blobClient = storageAccount.CreateCloudBlobClient()
    let container = blobClient.GetContainerReference(containerName)
    container.GetBlockBlobReference(blobName)

let Run(inputMessage: string, log: TraceWriter) =
    log.Info(sprintf "F# Queue trigger function processed: '%s'" inputMessage)


    let cloudBlockBlob = GetBlob "alexasoundboard_STORAGE" "sounds-new" "test.mp3" 