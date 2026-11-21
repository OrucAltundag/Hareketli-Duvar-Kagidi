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
