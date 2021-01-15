#!/bin/sh
set -euo pipefail

# Install DotNet
pushd /tmp
mkdir -p dotnet
pushd dotnet
curl -sSL -o ./dotnet.tar.gz https://download.visualstudio.microsoft.com/download/pr/53cace8f-2907-487e-84d7-bc7a7ba5de05/326704ffa2ef9d4dcd0db2f1da996ebb/aspnetcore-runtime-2.2.1-linux-x64.tar.gz
tar zxf ./dotnet.tar.gz
export DOTNET_ROOT=$PWD
export PATH=$PATH:$PWD
popd

# Get The Runner
mkdir -p ./runner
pushd ./runner
curl -sSL -o ./runner.zip https://s3.eu-west-2.amazonaws.com/jdunkerley/runner.zip
unzip ./runner.zip
export DOTNET_RUNNER=$PWD
popd
popd

# Processing
dotnet --info
cd $LAMBDA_TASK_ROOT
dotnet "$DOTNET_RUNNER/runner.dll" "$LAMBDA_TASK_ROOT/diabeteslog.dll "DiabetesLog.Handler" "EntryPoint"