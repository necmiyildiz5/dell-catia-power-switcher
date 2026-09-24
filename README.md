# Dell CATIA Power Switcher

**Türkçe aşağıda / Turkish below.**

## ⚠️ Compatibility — read before downloading

This is a **beta utility only for Windows 11 Dell computers that have Dell Optimizer installed**. It will not work as intended unless all of these are available:

- Dell Optimizer, including its `do-cli.exe` command-line tool
- Windows **Ultimate Performance** and **Power saver** power plans
- Permission to approve the Windows administrator prompt

If your PC is not a Dell system, uses Windows 10, does not have Dell Optimizer, or lacks either power plan, **do not use this release**.

## What it switches

| Profile | Windows plan | Dell thermal mode | Energy saver |
| --- | --- | --- | --- |
| **PERFO** | Ultimate Performance | Ultra Performance | Off |
| **SAVER** | Power saver | Quiet | On |

For CATIA work, choose **PERFO**. For everyday use, choose **SAVER**.

## Download and run

1. Download `PerfoSaver-v0.5.0.zip` from the latest [Release](../../releases).
2. Extract it to a permanent folder. Keep `PerfoSaver-v5.exe` and `PerfoSaver-EnergyHelper.exe` together.
3. Run `PerfoSaver-v5.exe` and approve the administrator prompt.
4. Choose PERFO or SAVER.

> **Beta note:** Windows power-plan and Dell Quiet/Ultra changes were tested on the original Dell computer. Energy saver automation needs validation across additional Windows 11 and Dell models.

## Before using

In Dell Optimizer, turn off **Allow Dell Optimizer to synchronize thermal management mode with Windows power mode settings and vice versa**. Otherwise Dell can overwrite the selected profile.

## Troubleshooting

- Dell error 9: close Dell Optimizer and retry.
- Energy saver failed: open **Settings → System → Power & battery → Energy saver** once, then retry.
- A required power plan is missing: see [Troubleshooting](docs/TROUBLESHOOTING.md).
- This unsigned utility may trigger Windows Defender; review the source and build with `Derle.ps1` if preferred.

## Build from source

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\Derle.ps1
```

[MIT License](LICENSE) · [Contributing](CONTRIBUTING.md)

---

# Dell CATIA Power Switcher — Türkçe

## ⚠️ Uyumluluk — indirmeden önce okuyun

Bu araç, **yalnızca Dell Optimizer kurulu Windows 11 Dell bilgisayarları** için hazırlanmış beta bir uygulamadır. Aşağıdakilerin tamamı yoksa amaçlandığı gibi çalışmaz:

- Dell Optimizer ve `do-cli.exe` komut satırı aracı
- Windows’ta **Ultimate Performance** ve **Power saver** güç planları
- Windows yönetici iznini onaylama yetkisi

Bilgisayarınız Dell değilse, Windows 10 kullanıyorsa, Dell Optimizer yoksa veya iki güç planından biri yoksa **bu sürümü kullanmayın**.

## Ne yapar?

| Profil | Windows planı | Dell ısıl mod | Energy saver |
| --- | --- | --- | --- |
| **PERFO** | Ultimate Performance | Ultra Performance | Kapalı |
| **SAVER** | Power saver | Quiet | Açık |

CATIA çalışırken **PERFO**, günlük kullanımda **SAVER** seçin.

## İndir ve çalıştır

1. En son [Release](../../releases) sayfasından `PerfoSaver-v0.5.0.zip` indirin.
2. Kalıcı bir klasöre çıkarın; iki EXE dosyası aynı klasörde kalmalıdır.
3. `PerfoSaver-v5.exe` dosyasını açın ve yönetici iznini onaylayın.
4. PERFO veya SAVER seçin.

## Kullanmadan önce

Dell Optimizer içinden **Allow Dell Optimizer to synchronize thermal management mode with Windows power mode settings and vice versa** ayarını kapatın. Açık kalırsa Dell seçtiğiniz profili değiştirebilir.

## Sorun giderme

- Dell hata 9: Dell Optimizer’ı kapatıp tekrar deneyin.
- Energy saver uygulanmadı: **Ayarlar → Sistem → Güç ve pil → Energy saver** sayfasını bir kez açıp tekrar deneyin.
- Güç planı yok: [Sorun giderme](docs/TROUBLESHOOTING.md) sayfasına bakın.

Bu beta sürümünde Energy saver otomasyonu farklı Dell ve Windows 11 sürümlerinde daha fazla doğrulama gerektirir. Sorun bildirirken Dell modeli, Windows sürümü, Dell Optimizer sürümü ve kişisel yolları silinmiş `son-islem.txt` bilgisini ekleyin.
