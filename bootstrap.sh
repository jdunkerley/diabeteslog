#!/bin/sh

set -euo pipefail

# Install DotNet
sudo yum update -y
sudo yum install libunwind libicu -y
sudo mkdir -p /opt/dotnet && sudo chmod 777 /opt/dotnet
curl -sSL -o /opt/dotnet/dotnet.tar.gz https://download.visualstudio.microsoft.com/download/pr/53cace8f-2907-487e-84d7-bc7a7ba5de05/326704ffa2ef9d4dcd0db2f1da996ebb/aspnetcore-runtime-2.2.1-linux-x64.tar.gz
tar zxf /opt/dotnet/dotnet.tar.gz -C /opt/dotnet
export DOTNET_ROOT=/opt/dotnet 
export PATH=$PATH:/opt/dotnet

# Get The Runner
sudo mkdir -p /opt/runner && sudo chmod 777 /opt/runner
curl -sSL -o /opt/runner/runner.zip https://s3.eu-west-2.amazonaws.com/jdunkerley/runner.zip
unzip /opt/runner/runner.zip

# Processing
cd $LAMBDA_TASK_ROOT
dotnet /opt/runner/runner.dll "$LAMBDA_TASK_ROOT/diabeteslog.dll "DiabetesLog.Handler" "EntryPoint"
