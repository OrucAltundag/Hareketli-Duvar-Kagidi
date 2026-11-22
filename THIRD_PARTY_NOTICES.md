# Üçüncü taraf bileşen bildirimleri

Bu belge projenin kendi lisansını belirlemez. Kontrol tarihi 21 Kasım 2026.

## Doğrulanan bağımlılıklar

| Bileşen | Dağıtılan sürüm | Doğrulanan bilgi |
| --- | --- | --- |
| LibVLCSharp | 3.9.4 | NuGet metaverisinde LGPL-2.1-or-later, aynı etiketin LICENSE metni eklendi. |
| VideoLAN.LibVLC.Windows | 3.0.23 | NuGet metaverisinde LGPL-2.1-or-later. Native DLL, eklenti, Lua ve diğer veri dosyalarını içerir. |
| Microsoft.NETCore.App | 10.0.12 | Self-contained runtime. Paket içindeki lisans ve üçüncü taraf bildirimleri eklendi. |
| Microsoft.WindowsDesktop.App | 10.0.12 | Paket lisansı ve aynı sürüm WPF/Windows Forms kaynak bildirimleri eklendi. |

NuGet paket listesinde ek geçişli bağımlılık görülmedi. Native dosyaların ve frameworklerin bağımlılıkları bu NuGet listesinden ibaret değildir.

## Pakete eklenen belgeler

Installer tüm yayın klasörünü kopyalar. Build.ps1, bu belgeyi ve licenses klasörünü yayın klasörüne ekler ve beklenen bildirimlerin varlığını denetler.

- LibVLCSharp LICENSE metni.
- VLC 3.0.23 COPYING.LIB (LGPL), COPYING (GPL), AUTHORS ve THANKS dosyaları.
- .NET Runtime lisansı ve üçüncü taraf bildirimleri.
- .NET Desktop paket lisansı, WPF ve Windows Forms lisansları ve üçüncü taraf bildirimleri.
- Inno Setup 6.7.4 lisans metni.
- SOURCES.json içinde belge kaynağı, sürüm ve SHA-256 değerleri.
- VLC-BINARY-INVENTORY.json içinde dağıtılan x64/x86 native paket dosyaları, göreli yollar, boyut ve SHA-256 değerleri.

GPL metninin eklenmesi uygulamaya veya her native eklentiye GPL lisansı atandığı anlamına gelmez. LGPL metni de tüm native bileşenlerin lisansını tek başına doğrulamaz.

## Release yayımlamaya engel kalan doğrulamalar

**Dağıtım uygunluğu henüz eksiksiz doğrulanmamıştır.**

1. NuGet native paketinde her eklenti ve bağlı contrib kütüphanesi için ayrı telif/lisans envanteri bulunmadı. Binary envanteri lisans eşleştirmesinin yerini tutmaz. FFmpeg ve diğer contrib bileşenlerinin tam sürüm, yapılandırma, telif ve lisans metinleri kullanılan binary derlemeye göre eşleştirilmelidir.
2. LibVLC, LibVLCSharp ve LGPL kapsamındaki bağlı bileşenler için dağıtılan ikililere karşılık gelen kaynakların, değişikliklerin ve derleme betiklerinin erişilebilirliği doğrulanmalı, gerekli kaynak sağlama yöntemi Release dağıtımıyla birlikte hazırlanmalıdır. Aşağıdaki üst kaynak bağlantıları bu yükümlülüğün tamamlandığına dair garanti değildir.
3. LGPL kapsamındaki hata ayıklama/değiştirme ve dinamik kütüphane değiştirme haklarını kısıtlayan bir dağıtım koşulu eklenmemelidir. Bu inceleme kapsamında ayrı bir EULA oluşturulmadı.
4. Inno Setup 6.7.4 yerel lisans metni eklendi. Kurulum motorunun telif ve web adresleri korunmalıdır. Derleyici çıktısı ticari kullanım için ayrı bir kullanım bildirimi göstermektedir. Ticari dağıtım düşünülürse derleyicinin kullanım şartları ayrıca doğrulanmalıdır.

Bu eksikler giderilmeden yalnızca lisans metinleri eklendiği için Release dağıtımına uygunluk iddia edilmemelidir.

## Üst kaynaklar

- [LibVLCSharp 3.9.4](https://github.com/videolan/libvlcsharp/tree/3.9.4)
- [VLC 3.0.23 kaynakları](https://github.com/videolan/vlc/tree/3.0.23)
- [LibVLC NuGet paketleme deposu](https://code.videolan.org/videolan/libvlc-nuget)
- [VideoLAN dağıtım açıklaması](https://docs.videolan.me/vlc-user/en/support/faq/legalconcerns.html)
- [Native NuGet paketi](https://www.nuget.org/packages/VideoLAN.LibVLC.Windows/3.0.23)
- [Inno Setup lisansı](https://jrsoftware.org/files/is/license.txt)
## 22 Kasım 2026 tarihli native dağıtım incelemesi

850 dosyanın tümü NuGet arşivindeki build/x64 ve build/x86 girdileriyle bayt düzeyinde eşleşti. Bunların 650'si DLL'dir. Her mimarideki 325 DLL'nin tamamı, VideoLAN'ın SHA-256 değerleri kontrol edilen [Windows x64](https://download.videolan.org/pub/videolan/vlc/3.0.23/win64/) ve [Windows x86](https://download.videolan.org/pub/videolan/vlc/3.0.23/win32/) ZIP dağıtımlarındaki karşılıklarıyla aynıdır.

Her iki libvlccore.dll içindeki sürüm 3.0.23-2-0-g79128878dd olarak okunmuştur. [Tam VLC revizyonu, kaynaklar ve derleme betikleri](https://code.videolan.org/videolan/vlc/-/tree/79128878ddb2c280bbb6c89c76a46b31a80ade1c) erişilebilirdir. Paket içindeki licenses/VLC-SOURCE-MAP.json, her göreli dosya için doğrudan kaynak adaylarını ve okunabilen kaynak başlıklarındaki telif/lisans satırlarını listeler. Kaynak başlıklarındaki bilgiler yalnızca ilgili kaynak dosyasına aittir. Koşullu derleme seçenekleri, üretilen dosyalar ve statik bağımlılıklar bu listeyi aşabilir.

**FFmpeg için GPL etkin.** İki libavcodec_plugin.dll içinde de libavcodec 58.134.100 ve FFmpeg configure satırında --enable-gpl --enable-postproc --enable-static --disable-shared okunmuştur. FFmpeg bu yapılandırmada GPL kapsamındadır. Yapılandırma ayrıca GSM, OpenJPEG ve LAME desteği bildirir. VLC revizyonunun contrib/src/ffmpeg/rules.mak tarifinde 4.4.5 seçilidir. [FFmpeg kaynak arşivi](https://ffmpeg.org/releases/ffmpeg-4.4.5.tar.xz) indirildi ve tarifin SHA512SUMS değeriyle eşleşti. Kaynak paketten alınan lisans ve katkıcı belgelerinin kökeni SOURCES.json içindedir. [FFmpeg lisans açıklaması](https://ffmpeg.org/legal.html)

**Diğer doğrulamalar.** libbluray 1.4.0 kaynak arşivi de VLC tarifinin SHA-512 değeriyle eşleşti. İki BD-J JAR adı aynı sürümü taşıyor. JAR sınıflarının yeniden derleme eşleşmesi açık. HRTF dosyası VLC revizyonundaki share/hrtfs kopyasıyla özdeş ve veri içinde 3D Sound Labs 2017 telif bildirimi ile LGPL-2.1-or-later koşulu bulunuyor.

| Koşul | Bu dağıtım için gereken işlem |
| --- | --- |
| GPLv2 §1–3 | Telif ve lisans metinlerini koruyun. GPL kapsamındaki ikililerle eksiksiz karşılık gelen kaynakları, değişiklikleri ve derleme/kurulum betiklerini §3 kapsamındaki geçerli bir yöntemle sunun. İndirme dağıtımında aynı yerden eşdeğer kaynak erişimi sağlanabilir. Uygulama ile eklentilerin birleşik eser sayılıp sayılmadığı ayrıca değerlendirilmelidir. |
| LGPLv2.1 §4–6 | Kütüphane dağıtımında kaynak ve bildirimleri sağlayın. Kütüphaneyi kullanan çalışmanın dağıtımında §6 seçeneklerinden uygulanabilir olanı karşılayın. Uyumlu değiştirilmiş DLL ile çalışabilme veya yeniden bağlamaya yeterli nesne/kaynak sağlama yolunu doğrulayın. Kullanıcının değişiklikleri ve hata ayıklamak için tersine mühendisliği engellenmemelidir. |
| Diğer lisanslar | İzinli lisanslarda ilgili bileşenin koşullarına göre telif, izin ve garanti reddi metinlerini taşıyın. Veri dosyalarının kendi bildirimlerini koruyun. Bileşen kanıtı olmadan toplu bir lisans atanmamıştır. |

**Açık liste.** VLC-SOURCE-MAP.json içindeki tüm modüller için tam statik bağımlılık kapanışı ve kullanılan yamaların derleme kaydı açık. Özellikle FFmpeg'e bağlanan GSM/OpenJPEG/LAME, çekirdekteki kriptografi kodu, iki libbluray JAR'ı ve libvlc.lib/libvlccore.lib üretim girdileri henüz eksiksiz eşleştirilmemiştir. Arşiv doğrulaması, derleyici girdilerinin tamamının veya yeniden bağlanabilirliğin doğrulanması değildir. Yayından önce iki mimarinin gerçek CI işlerinden contrib manifestleri, yama listeleri ve derleme kayıtları alınmalı, karşılık gelen kaynak paketi hazırlanmalı ve seçilen lisansların birlikte dağıtım koşulları incelenmelidir.

## v1.0.1 kaynak sağlama planı

Kurulum dizinindeki `sources/HareketliDuvarKagidi-1.0.1-sources-partial.zip` toplanan kaynakları ve bildirimleri içerir. Aynı dizindeki `.sha256` dosyası bu ZIP'in özetidir. ZIP içindeki `SourceArchives.json` her arşivin erişim adresini ve SHA-256 değerini verir. VLC contrib tariflerinden doğrulanan arşivlerin SHA-512 değerleri de kayıtlıdır.

VLC revizyonunun arşivindeki `contrib/src/<bileşen>/` dizinleri, ilgili `rules.mak` tariflerini ve yamaları içerir. Windows derleme betikleri `extras/package/win32/` altındadır. FFmpeg ve diğer bağımlılıklar için arşivleri tariflerdeki adlarla kullanın. Koşullu tariflerin hangi dallarının çalıştığı ve araç zincirinden hazır gelen kütüphaneler iki Release işinin kayıtlarıyla henüz doğrulanmamıştır. ZIP, doğrulanmış tam bir yeniden derleme veya yeniden bağlama kiti olarak sunulmamaktadır.

GSM 1.0.13, OpenJPEG 2.5.0 ve LAME 3.100 kaynak arşivlerinden alınan telif ve lisans metinleri `licenses/` altındadır. `SOURCES.json` bu arşivlerin adreslerini, SHA-512 özetlerini ve metinlerin arşiv içindeki yollarını içerir.

1. VideoLAN'dan [Win32 işi 2403066](https://code.videolan.org/videolan/vlc/-/jobs/2403066) ve [Win64 işi 2403067](https://code.videolan.org/videolan/vlc/-/jobs/2403067) için contrib manifestlerini, uygulanan yamaları, statik bağlama girdilerini ve BD-J derleme hedeflerini isteyin. İki mimariyi ayrı ayrı VLC-BINARY-INVENTORY.json ile eşleştirin.
2. GPL/LGPL kapsamındaki dağıtılan ikililere karşılık gelen kaynakları, yamaları ve derleme/kurulum betiklerini sürümlü bir kaynak arşivinde toplayın. Statik bağlı kütüphaneler için seçilen yeniden bağlama yönteminin gerektirdiği girdileri ekleyin. Uyumlu değiştirilmiş DLL ve yeniden bağlanmış eklentiyle oynatmayı sınayın. GPL etkin FFmpeg ile uygulamanın birlikte dağıtım koşullarını netleştirin.
3. Kaynak arşivini temiz bir dizinde açıp belgelendirilmiş komutlarla derleyin. Her bileşenin sürümünü, bildirimini ve kaynak yolunu kontrol edin. Kontrol tamamlandığında kaynak arşivini kendi SHA-256 özetiyle Setup.exe'nin yanında aynı Release'e ekleyin. Alıcı yalnızca Setup.exe'yi indirerek kurabilmeli ve aynı sayfadan karşılık gelen kaynaklara erişebilmelidir.
