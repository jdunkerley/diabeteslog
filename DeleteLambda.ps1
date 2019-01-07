$folderName = Split-Path -leaf $PSScriptRoot
$region = aws configure get region
$accountId = aws sts get-caller-identity --output text --query 'Account'

aws lambda delete-function --function-name $folderName
aws iam detach-role-policy --role-name diabeteslog --policy-arn "arn:aws:iam::$($accountId):policy/$($folderName)"
aws iam delete-policy --policy-arn "arn:aws:iam::$($accountId):policy/$($folderName)"
aws iam delete-role --role-name $folderName
