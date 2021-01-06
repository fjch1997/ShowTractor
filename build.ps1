msbuild ShowTractor.sln /t:Restore /p:Configuration=Release
dotnet test ShowTractor.Tests
dotnet test ShowTractor.Plugins.Tmdb.Tests
msbuild ShowTractor.sln /p:Configuration=Release /p:Platform=x86
msbuild ShowTractor.sln /p:Configuration=Release /p:Platform=x64
msbuild ShowTractor.sln /p:Configuration=Release /p:Platform=arm64
