$ErrorActionPreference = "Stop"
Import-Module "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\Tools\Microsoft.VisualStudio.DevShell.dll"
Enter-VsDevShell -VsInstallPath "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise" -SkipAutomaticLocation
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
        Set-AuthenticodeSignature -Certificate $certificate -FilePath $exePath -TimestampServer http://timestamp.sectigo.com
    }
    MakeAppx pack /d $unpackedPath /p $msixPath /o
    SignTool sign /a /v /fd SHA256 /tr http://timestamp.sectigo.com /td SHA256 /sha1 $certificate.Thumbprint $msixPath
}
