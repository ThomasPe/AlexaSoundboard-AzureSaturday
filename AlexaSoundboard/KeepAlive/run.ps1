Write-Output "KeepAlive trigger function executed at:$(get-date)";
$YourURI = 'https://alexasoundboardskill.azurewebsites.net/api/alexa-soundboard'
Invoke-WebRequest -Uri $YourURI -Method POST -UseBasicParsing

Write-Output "Done triggering KeepAlive";
"KeepAlive: ran successfully" | Out-File -Encoding UTF8 $logging;

