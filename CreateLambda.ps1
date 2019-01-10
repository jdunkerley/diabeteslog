Push-Location $PSScriptRoot

dotnet publish -c Release -r linux-x64 . /p:GenerateRuntimeConfigurationFiles=true

Push-Location .\bin\Release\netcoreapp2.1\linux-x64\publish\
Copy-Item "$PSScriptRoot\bootstrap.sh" ".\bootstrap"
Compress-Archive -Path * -CompressionLevel Fastest -DestinationPath $PSScriptRoot\lambda_function.zip -Force
Pop-Location

$folderName = Split-Path -leaf $PSScriptRoot
$region = aws configure get region
$accountId = aws sts get-caller-identity --output text --query 'Account'

$roleARN = (aws iam create-role --role-name $folderName --assume-role-policy-document '{\"Version\":\"2012-10-17\",\"Statement\":[{\"Effect\":\"Allow\",\"Principal\":{\"Service\":\"lambda.amazonaws.com\"},\"Action\":\"sts:AssumeRole\"}]}' | ConvertFrom-Json).Role.Arn
$policy = (ConvertTo-Json -Depth 3 -Compress @{
    "Version" = "2012-10-17"
    "Statement" = @(
        @{
            "Effect" = "Allow"
            "Action" = "logs:CreateLogGroup"
            "Resource" = "arn:aws:logs:$($region):$($accountId):*"
        }
        @{
            "Effect" = "Allow"
            "Action" = @("logs:CreateLogStream", "logs:PutLogEvents")
            "Resource" = @("arn:aws:logs:$($region):$($accountId):log-group:/aws/lambda/$($folderName):*")
        }
    )
}).Replace("`"","`\`"")
$policyARN = (aws iam create-policy --policy-name $folderName --policy-document $policy | ConvertFrom-Json).Policy.Arn
aws iam attach-role-policy --role-name $folderName --policy-arn $policyARN

Start-Sleep -s 10
$namespace = (Get-Content .\Handler.cs | Select-String -Pattern "namespace " | Select-Object -ExpandProperty Line).Replace("namespace ","")
# aws lambda create-function --function-name $folderName --runtime "dotnetcore2.1" --handler "$folderName::$namespace.Handler::EntryPoint" --zip-file fileb://lambda_function.zip --role "$roleARN"
aws lambda create-function --function-name $folderName --runtime "provided" --handler "$folderName::$namespace.Handler::EntryPoint" --zip-file fileb://lambda_function.zip --role "$roleARN"

Remove-Item .\lambda_function.zip
Pop-Location
