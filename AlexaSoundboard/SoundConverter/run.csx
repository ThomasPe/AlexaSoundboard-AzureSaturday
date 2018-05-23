#r "Microsoft.WindowsAzure.Storage"

using Microsoft.WindowsAzure.Storage.Blob;

using System.Diagnostics;
using System.IO;

public static async Task Run(CloudBlockBlob myBlob, string name, Binder binder, TraceWriter log)
{
    log.Info($"Jordan C# Blob trigger function Processed blob\n Name:{name}");

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
}