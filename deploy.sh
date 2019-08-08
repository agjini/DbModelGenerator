#!/bin/bash

rm -Rf DbModelGenerator/bin
rm -Rf DbModelGenerator/obj

dotnet build -c Release

dotnet nuget push DbModelGenerator/bin/Release/*.nupkg -k oy2gtjnfmjscinvysijq2kitgf4ksacennwwuqcwgcaj2a -s https://api.nuget.org/v3/index.json

