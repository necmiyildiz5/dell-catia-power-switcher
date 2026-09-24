# Perfo / Saver for Dell + CATIA

**Türkçe aşağıda / Turkish below.**

A small Windows utility for Dell workstations used with CATIA. Select a profile instead of opening Dell Optimizer, Control Panel, and Windows Quick Settings separately.

| Profile | Windows power plan | Dell Optimizer thermal mode | Windows Energy saver |
| --- | --- | --- | --- |
| **PERFO** | Ultimate Performance | Ultra Performance | Off |
| **SAVER** | Power saver | Quiet | On |

> **Beta status.** The Windows power-plan and Dell Quiet/Ultra changes were tested on the original Dell computer. Energy saver automation has been updated but needs validation on more Windows 11 and Dell configurations. Please open an issue with your Dell model, Windows version, Dell Optimizer version, and the contents of `son-islem.txt` if it fails.

## Download and run

1. Open the latest GitHub **Release** and download `PerfoSaver-v5.zip`.
2. Extract it to a permanent folder, for example `C:\Tools\PerfoSaver`.
3. Run `PerfoSaver-v5.exe` and approve the Windows administrator prompt. Dell Optimizer needs administrator rights for thermal-mode changes.
4. Choose **PERFO** before a demanding CATIA session, or **SAVER** for everyday use.

The package must keep `PerfoSaver-v5.exe` and `PerfoSaver-EnergyHelper.exe` in the same folder. The helper is intentionally not elevated: Windows exposes the Energy saver switch to the normal desktop session.

## What you need

- Windows 11
- Dell Optimizer installed, including the command-line interface (`do-cli.exe`)
- The **Ultimate Performance** and **Power saver** plans available in Windows

In Dell Optimizer, disable **Allow Dell Optimizer to synchronize thermal management mode with Windows power mode settings and vice versa**. Otherwise Dell can overwrite the selected profile.

## Keyboard shortcut

Create a shortcut to `PerfoSaver-v5.exe`, open its Properties, and set a shortcut key such as `Ctrl + Alt + P`.

## Troubleshooting

- **Dell error 9:** close Dell Optimizer and try again.
- **Energy saver is not applied:** open **Settings → System → Power & battery → Energy saver** once, then retry. Check `son-islem.txt` in the app folder.
- **A plan is missing:** restore or create the plan in Windows, then see [Troubleshooting](docs/TROUBLESHOOTING.md).
- **Windows Defender warning:** this is an unsigned personal utility. Review the source and build it yourself with `Derle.ps1` if you prefer.

## Build from source

Run PowerShell in the repository folder:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\Derle.ps1
```

This uses the .NET Framework compiler included with Windows. It produces the elevated main app and the non-elevated Energy saver helper.

## Contributing

New Dell models and Windows versions are especially useful. Please read [CONTRIBUTING.md](CONTRIBUTING.md) before opening an issue or pull request.

## License

[MIT](LICENSE)

---

# Perfo / Saver — Dell + CATIA için

Dell iş istasyonunda CATIA kullananlar için küçük bir Windows aracı. Dell Optimizer, Denetim Masası ve Hızlı Ayarlar’ı tek tek açmak yerine bir profil seçersiniz.

| Profil | Windows güç planı | Dell Optimizer ısıl mod | Windows Enerji tasarrufu |
| --- | --- | --- | --- |
| **PERFO** | Ultimate Performance | Ultra Performance | Kapalı |
| **SAVER** | Power saver | Quiet | Açık |

> **Beta durumu.** Windows güç planı ile Dell Quiet/Ultra geçişi ilk Dell bilgisayarında denendi. Energy saver otomasyonu güncellendi; farklı Windows 11 ve Dell kurulumlarında daha fazla doğrulama gerekiyor. Sorun olursa Dell modelinizi, Windows sürümünüzü, Dell Optimizer sürümünüzü ve `son-islem.txt` içeriğini Issue olarak paylaşın.

## İndir ve çalıştır

1. En son GitHub **Release** sayfasından `PerfoSaver-v5.zip` dosyasını indirin.
2. Dosyayı kalıcı bir klasöre çıkarın: örneğin `C:\Tools\PerfoSaver`.
3. `PerfoSaver-v5.exe` dosyasını açın ve Windows yönetici iznine onay verin. Dell Optimizer ısıl mod değişimi için bu izin gerekir.
4. Yoğun CATIA çalışmasından önce **PERFO**, günlük kullanımda **SAVER** seçin.

Paketteki `PerfoSaver-v5.exe` ile `PerfoSaver-EnergyHelper.exe` aynı klasörde kalmalıdır. Yardımcı uygulama bilerek yönetici olarak çalışmaz; Windows Energy saver anahtarını normal masaüstü oturumuna gösterir.

## Gereksinimler

- Windows 11
- Komut satırı aracı (`do-cli.exe`) ile kurulu Dell Optimizer
- Windows’ta **Ultimate Performance** ve **Power saver** planlarının bulunması

Dell Optimizer içinden **Allow Dell Optimizer to synchronize thermal management mode with Windows power mode settings and vice versa** ayarını kapatın. Açık olursa Dell seçilen profili geri değiştirebilir.

## Kısayol tuşu

`PerfoSaver-v5.exe` için bir kısayol oluşturun, Özellikler’i açın ve örneğin `Ctrl + Alt + P` atayın.

## Sorun giderme

- **Dell hata 9:** Dell Optimizer’ı kapatıp yeniden deneyin.
- **Energy saver uygulanmadı:** **Ayarlar → Sistem → Güç ve pil → Energy saver** bölümünü bir kez açın, sonra tekrar deneyin. Uygulama klasöründeki `son-islem.txt` dosyasını inceleyin.
- **Güç planı yok:** Windows’ta planı geri getirin veya oluşturun; [Sorun giderme](docs/TROUBLESHOOTING.md) sayfasına bakın.
- **Windows Defender uyarısı:** Bu imzasız kişisel bir araçtır. İsterseniz kaynak kodu inceleyip `Derle.ps1` ile kendiniz derleyin.

## Kaynak koddan derleme

Depo klasöründe PowerShell açıp şunu çalıştırın:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\Derle.ps1
```

Windows’un kendi .NET Framework derleyicisi kullanılır. Komut, yönetici yetkili ana uygulamayı ve normal yetkili Energy saver yardımcısını üretir.

## Katkı

Farklı Dell modelleri ve Windows sürümleri çok değerlidir. Issue veya pull request açmadan önce [CONTRIBUTING.md](CONTRIBUTING.md) dosyasına bakın.

## Lisans

[MIT](LICENSE)
