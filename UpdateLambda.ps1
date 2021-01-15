Push-Location $PSScriptRoot

dotnet publish -c Release -r linux-x64 . /p:GenerateRuntimeConfigurationFiles=true

Push-Location .\bin\Release\netcoreapp2.1\linux-x64\publish\
Copy-Item "$PSScriptRoot\bootstrap.sh" ".\bootstrap"
wsl chmod 777 "./bootstrap"
Compress-Archive -Path * -CompressionLevel Fastest -DestinationPath $PSScriptRoot\lambda_function.zip -Force
Pop-Location

$folderName = Split-Path -leaf $PSScriptRoot
aws s3 cp ./lambda_function.zip "s3://jdunkerley/$folderName.zip"
aws lambda update-function-code --function-name $folderName --zip-file fileb://lambda_function.zip

Remove-Item .\lambda_function.zip

aws lambda invoke --function-name $folderName test.log
Get-Content test.log
Remove-Item test.log

Pop-Location
