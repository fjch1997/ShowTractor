$ErrorActionPreference = "Stop"
Import-Module "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\Tools\Microsoft.VisualStudio.DevShell.dll"
Enter-VsDevShell -VsInstallPath "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise" -SkipAutomaticLocation
msbuild ShowTractor.sln /t:Restore /p:Configuration=Release
dotnet test ShowTractor.Tests
dotnet test ShowTractor.Plugins.Tmdb.Tests
msbuild ShowTractor.sln /p:Configuration=Release /p:Platform=x86
msbuild ShowTractor.sln /p:Configuration=Release /p:Platform=x64
msbuild ShowTractor.sln /p:Configuration=Release /p:Platform=arm64
