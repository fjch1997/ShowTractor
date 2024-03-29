﻿$ErrorActionPreference = "Stop"
Import-Module "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\Tools\Microsoft.VisualStudio.DevShell.dll"
Enter-VsDevShell -VsInstallPath "C:\Program Files\Microsoft Visual Studio\2022\Enterprise" -SkipAutomaticLocation
$certificate = (Get-Item Cert:\CurrentUser\My\* -CodeSigningCert)
Write-Host "Signing will be using" $certificate.FriendlyName
foreach ($msixFile in Get-ChildItem "ShowTractor.WinUI\ShowTractor.WinUI (Package)\AppPackages\ShowTractor.WinUI (Package)*\*.msix")
{
    $msixPath = $msixFile.FullName
    Write-Host "Signing Package $msixPath"
    $unpackedPath = $msixFile.FullName.Substring(0, $msixFile.FullName.Length - 5)
    MakeAppx unpack /o /p $msixPath /d $unpackedPath
    Get-ChildItem $unpackedPath\
    foreach ($exeFile in Get-ChildItem "$unpackedPath\**\*.exe")
    {
        $exeName = $exeFile.Name
        $exePath = $exeFile.FullName
        Write-Host "Signing $exeName"
        Set-AuthenticodeSignature -Certificate $certificate -TimestampServer http://timestamp.sectigo.com -FilePath $exePath
    }
    # Change the package identity name so as to not conflict with debug installation.
    (Get-Content "$unpackedPath\AppxManifest.xml").Replace("f0d5fe17-7d4a-4795-9ab2-55b4be65317e", "ff67f6ff-3707-446e-a79d-3e95f4d04f68") | Out-File "$unpackedPath\AppxManifest.xml" -Encoding utf8
    MakeAppx pack /d $unpackedPath /p $msixPath /o
    SignTool sign /a /v /fd SHA256 /tr http://timestamp.sectigo.com /td SHA256 /sha1 $certificate.Thumbprint $msixPath
}
# Sign plugins
Set-AuthenticodeSignature -Certificate $certificate -TimestampServer http://timestamp.sectigo.com -FilePath ShowTractor.Plugins.Tmdb\bin\Release\net6.0\ShowTractor.Plugins.Tmdb.dll
