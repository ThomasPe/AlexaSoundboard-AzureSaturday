Write-Output "KeepAlive trigger function executed at:$(get-date)";
$YourURI = 'https://alexasoundboardskill.azurewebsites.net/api/alexa-soundboard'
Invoke-RestMethod -Method Post -Uri $YourURI