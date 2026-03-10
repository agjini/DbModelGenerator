#!/bin/bash
set -e

rm -Rf DbModelGenerator/bin
rm -Rf DbModelGenerator/obj

dotnet test DbModelGenerator.Test/DbModelGenerator.Test.csproj -c Release

dotnet pack DbModelGenerator/DbModelGenerator.csproj -c Release

# Be sure to add nuget API key before in your bashrc
# vi ~/.bashrc
# export NUGET_AUTH_KEY=XXXXX

dotnet nuget push DbModelGenerator/bin/Release/*.nupkg -k ${NUGET_AUTH_KEY} -s https://api.nuget.org/v3/index.json --skip-duplicate
