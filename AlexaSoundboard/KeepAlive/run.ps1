Write-Output "KeepAlive trigger function executed at:$(get-date)";
$Url = "https://alexasoundboardskill.azurewebsites.net/api/alexa-soundboard";
$Response = Invoke-WebRequest $Url
Write-Output $Response.StatusCode