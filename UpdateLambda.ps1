Push-Location $PSScriptRoot

dotnet publish -c Release -r linux-x64 . /p:GenerateRuntimeConfigurationFiles=true

Push-Location .\bin\Release\netcoreapp2.1\linux-x64\publish\
Compress-Archive -Path * -CompressionLevel Fastest -DestinationPath $PSScriptRoot\lambda_function.zip -Force
Pop-Location

$folderName = Split-Path -leaf $PSScriptRoot
aws lambda update-function-code --function-name $folderName --zip-file fileb://lambda_function.zip

Remove-Item .\lambda_function.zip

aws lambda invoke --function-name $folderName test.log
Get-Content test.log
Remove-Item test.log

Pop-Location
