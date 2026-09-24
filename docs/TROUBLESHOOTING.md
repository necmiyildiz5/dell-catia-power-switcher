# Troubleshooting / Sorun giderme

## Compatibility first / Önce uyumluluk

This utility is designed for **Windows 11 Dell computers with Dell Optimizer**. It cannot apply the Dell thermal mode if Dell Optimizer and its `do-cli.exe` tool are absent. It also requires both **Ultimate Performance** and **Power saver** plans.

Bu araç **Dell Optimizer kurulu Windows 11 Dell bilgisayarları** içindir. Dell Optimizer ve `do-cli.exe` yoksa Dell ısıl modunu değiştiremez. Ayrıca **Ultimate Performance** ve **Power saver** planlarının ikisi de gerekir.

## Ultimate Performance is missing / Ultimate Performance görünmüyor

Many Windows 11 computers do not show this plan by default. Open **Command Prompt as Administrator** and run:

```cmd
powercfg -duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61
```

Windows will return a new Power Scheme GUID. Then run:

```cmd
powercfg /list
```

Confirm that **Ultimate Performance** appears, then restart Perfo / Saver. The command duplicates the built-in Ultimate Performance scheme; it does not overclock the computer.

Birçok Windows 11 bilgisayarda bu plan varsayılan olarak görünmez. **Komut İstemi’ni Yönetici olarak** açıp yukarıdaki komutu çalıştırın. Windows yeni bir Power Scheme GUID döndürür. Ardından `powercfg /list` ile **Ultimate Performance** planının göründüğünü doğrulayın ve Perfo / Saver’ı yeniden açın. Bu komut yerleşik planı çoğaltır; bilgisayarı overclock etmez.

Microsoft documents the `/duplicatescheme` command here: https://learn.microsoft.com/en-us/windows-hardware/design/device-experiences/powercfg-command-line-options

## Dell Optimizer is not installed / Dell Optimizer yüklü değil

Dell Optimizer is not included on every Dell computer. Download it from Dell’s official driver page, ideally after entering your Service Tag so Dell filters for your model:

- Dell driver downloads: https://www.dell.com/support/home/en-us/drivers
- Dell Optimizer official package: https://www.dell.com/support/home/en-us/drivers/driversdetails?driverid=C28MR

Install it, restart Windows if requested, open Dell Optimizer once, then confirm that **Thermal Management** and its command-line tool are available. Dell Optimizer support varies by model; if Dell’s site does not list it for your Service Tag, this tool cannot control the Dell thermal profile.

Dell Optimizer her Dell bilgisayarda hazır gelmez. Önce Dell destek sayfasında Service Tag girin; cihazınıza uygun sürücü listelenecektir. Kurulumdan sonra gerekirse Windows’u yeniden başlatın, Dell Optimizer’ı bir kez açın ve **Thermal Management** bölümünün bulunduğunu doğrulayın. Dell’in sayfası Service Tag için uygulamayı listelemiyorsa bu araç Dell ısıl profilini kontrol edemez.

## Dell error 9 / Dell hata 9

Close Dell Optimizer and retry. If it continues, update Dell Optimizer from the official Dell page above.

## Energy saver / Enerji tasarrufu

Windows exposes this switch at **Settings → System → Power & battery → Energy saver**. If the helper fails, open this page once, switch it manually, retry, and attach `son-islem.txt` to an issue after removing personal paths.
