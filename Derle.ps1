$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$wpf = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\WPF'
& $compiler /nologo /target:winexe /platform:x64 /optimize+ "/win32manifest:$PSScriptRoot\App.manifest" "/out:$PSScriptRoot\PerfoSaver-v5.exe" /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:Microsoft.CSharp.dll "/r:$wpf\UIAutomationClient.dll" "/r:$wpf\UIAutomationTypes.dll" "/r:$wpf\WindowsBase.dll" "$PSScriptRoot\PerfoSaver.cs"
if ($LASTEXITCODE -ne 0) { throw 'Derleme başarısız.' }
& $compiler /nologo /target:winexe /platform:x64 /optimize+ "/win32manifest:$PSScriptRoot\Helper.manifest" "/out:$PSScriptRoot\PerfoSaver-EnergyHelper.exe" /r:System.Windows.Forms.dll /r:System.Drawing.dll /r:Microsoft.CSharp.dll "/r:$wpf\UIAutomationClient.dll" "/r:$wpf\UIAutomationTypes.dll" "/r:$wpf\WindowsBase.dll" "$PSScriptRoot\PerfoSaver.cs"
if ($LASTEXITCODE -ne 0) { throw 'Energy saver yardımcısı derlenemedi.' }

