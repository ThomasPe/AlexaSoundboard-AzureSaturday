#r "Microsoft.WindowsAzure.Storage"
#r "System.Net.Http"

using Microsoft.WindowsAzure.Storage.Blob;

using System.Diagnostics;
using System.IO;
using System.Net.Http;

public static async Task Run(CloudBlockBlob myBlob, string name, Binder binder, TraceWriter log)
{
    log.Info($"SoundConverter started: {name}");

    byte[] bytes = null;

    using(var ms = new MemoryStream()){
        myBlob.DownloadToStream(ms);
        bytes = ms.ToArray();
    }

    var f = @"D:\home\site\wwwroot\SoundConverter\ffmpeg.exe";

    var temp = Path.GetTempFileName();
    var tempOut = Path.GetTempFileName() + ".mp3";

    var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    Directory.CreateDirectory(tempPath);

    File.WriteAllBytes(temp, bytes);

    log.Info($"Exists: {File.Exists(temp)}");

    var readBack = File.ReadAllBytes(temp);

    log.Info($"ReadBack: {readBack.Length}, {temp}");

    var psi = new ProcessStartInfo();

    psi.FileName = f;
    psi.Arguments = $"-i \"{temp}\" -y -ac 2 -codec:a libmp3lame -b:a 48k -ar 16000 \"{tempOut}\"";
    psi.RedirectStandardOutput = true;
    psi.RedirectStandardError = true;
    psi.UseShellExecute = false;
    
    log.Info($"Args: {psi.Arguments}");

    var process = Process.Start(psi);
    //string output = process.StandardOutput.ReadToEnd();
    //string err = process.StandardError.ReadToEnd();
    process.WaitForExit((int)TimeSpan.FromSeconds(60).TotalMilliseconds);

// log.Info(output);
// log.Info(err);

    log.Info($"Output: {process.ExitCode}");
    

    log.Info($"Temp Out Exists: {File.Exists(tempOut)}");

    var attributes = new Attribute[]
    {
        new BlobAttribute("sounds/" + name),
        new StorageAccountAttribute("alexasoundboard_STORAGE")
    };


    var renc = File.ReadAllBytes(tempOut);
    log.Info($"Renc Length: {renc.Length}");
    var writer = await binder.BindAsync<CloudBlockBlob>(attributes);

    await writer.UploadFromByteArrayAsync(renc, 0, renc.Length);

    File.Delete(tempOut);
    File.Delete(temp);
    Directory.Delete(tempPath, true);    

    myBlob.DeleteIfExists();

    // Notify Twitter 
    var twitterAppUri = "https://prod-46.westeurope.logic.azure.com:443/workflows/62e19b15ab7e4354a0745e4272875e11/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=oFNsoHMDaTKs_vxbi1G_UrgL8nMmgiOEE49NwiKWtBg";
    var message = $"{{\"message\":\"I have found a new sound! {name}.mp3\"}}";
    using (var client = new HttpClient())
    {
        var response = client.PostAsync(twitterAppUri, new StringContent(message, Encoding.UTF8, "application/json")).Result;
    }
}