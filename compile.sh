#!/bin/bash

proj_name="Updatr"
version=$(cat version.txt)

# Lin:
dotnet publish -c Release -r linux-x64 --self-contained true /p:PublishTrimmed=true /p:TrimMode=link /p:PublishSingleFile=true /p:InvariantGlobalization=true /p:DebugType=None /p:DebugSymbols=false /p:AssemblyName="$proj_name-linux-x64-$version"


# Copy executables to the main directory
cp bin/Release/net9.0/linux-x64/publish/$proj_name-linux-x64-$version ./

# Cleaning up the install
rm -rf bin/*
rm -rf obj/*