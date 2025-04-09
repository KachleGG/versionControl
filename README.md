 # Single file executable Automatic GitHub updater

 ## Documentation
 This is an Automatic Updater for github repositories written as a class in C#.

 ## Use
 Copy the "Updater.cs" file into your source code folder and Make a constructor.
 The constructor is composed of the github users:
 - username
 - repository name
 - code version
 - App name
 - branch(default is "main")

 ## Compilation for debugging: ```dotnet publish -c Release -r linux-x64 --self-contained true /p:PublishTrimmed=true /p:TrimMode=link /p:PublishSingleFile=true /p:InvariantGlobalization=true /p:DebugType=None /p:DebugSymbols=false /p:AssemblyName="Updatr_linux-x64-1.1.4"
```