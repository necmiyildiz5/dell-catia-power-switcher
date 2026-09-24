# Troubleshooting / Sorun giderme

## Required power plans / Gerekli güç planları

Open Command Prompt as administrator and list the plans:

```cmd
powercfg /list
```

Perfo / Saver expects plans whose names are **Ultimate Performance** and **Power saver**. Names may be localized by Windows. If your computer has a customized plan, edit `PerfoSaver.cs` before building and test locally.

## Dell CLI / Dell komut satırı

The application looks for Dell Optimizer's `do-cli.exe`. Dell Optimizer needs to be installed and up to date. When Dell returns error 9, close its window and retry.

## Energy saver / Enerji tasarrufu

Windows exposes this switch at **Settings → System → Power & battery → Energy saver**. The application opens this page and operates the **Always use energy saver** control. Keep Settings open while the profile is applying.

If this step fails, switch it once manually, retry, and attach `son-islem.txt` to a GitHub issue after removing personal paths.
