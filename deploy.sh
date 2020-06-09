#!/bin/bash

rm -Rf DbModelGenerator/bin
rm -Rf DbModelGenerator/obj

dotnet build DbModelGenerator/DbModelGenerator.csproj

dotnet build -c Release

dotnet nuget push DbModelGenerator/bin/Release/*.nupkg -k oy2ohsyqqinqyjkdvdcq44743cnzilkezneur75tgky2gi -s https://api.nuget.org/v3/index.json

