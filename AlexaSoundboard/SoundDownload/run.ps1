$in = Get-Content $triggerInput -Raw
Write-Output "PowerShell script processed queue message '$in'"

Invoke-WebRequest -Uri $in -OutFile $outputFile

Out-file -FilePath $outputFile 