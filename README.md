# Hareketli Duvar Kâğıdı

Windows 11 x64 için yerel videoları masaüstü simgelerinin arkasında oynatan, C# ve WPF tabanlı duvar kâğıdı uygulaması.

**Sürüm 1.0.1** · **Geliştirici OrucAltundag**

## Öne çıkan özellikler

- Her monitörde bağımsız video, görüntü modu ve Başlat/Durdur kontrolü.
- MP4 ve WebM dosyalarıyla döngüsel oynatma.
- Monitör bağlantısı, çözünürlük ve ekran konumu değişikliklerinin izlenmesi.
- Video ve monitör tercihlerinin yerel olarak saklanması.
- Önceki oynatma tercihinin uygulama açılışında geri yüklenmesi.
- Sistem tepsisi ve kullanıcı tarafından değiştirilebilen Windows ile başlat ayarı.

## Çoklu monitör ve görüntü modları

Windows ekran düzenini **Genişlet** olarak ayarlayın. Her monitör için farklı video seçebilir, oynatmayı diğer ekranlardan bağımsız yönetebilirsiniz. Ayarlar monitörün aygıt kimliğine göre tutulur.

| Mod | Davranış |
| --- | --- |
| Doldur | Varsayılandır. En-boy oranını korur, ekranı kaplar ve gerekirse görüntüyü kırpar. |
| Sığdır | Videonun tamamını gösterir. Kenarlarda boş bant oluşabilir. |
| Uzat | Ekranı tamamen kaplar. Videonun en-boy oranı değişebilir. |

## Kurulum ve gereksinimler

Windows 11 x64 (derleme 22000 veya üzeri) gerekir. Hazır kurulum paketi .NET çalışma zamanını ve native VLC bileşenlerini içerir. Ayrıca .NET SDK veya VLC kurulumu gerektirmez.

Henüz GitHub sürümü yayımlanmamıştır. Kurulum dosyaları yayımlandığında [Releases sayfasında](https://github.com/OrucAltundag/Hareketli-Duvar-Kagidi/releases) bulunabilir.

## Video ekleme ve kullanım

1. Uygulamayı açın ve listeden bir monitör seçin.
2. **MP4 / WebM seç…** düğmesiyle yerel video ekleyin.
3. Görüntü modunu seçip **Başlat** düğmesine basın.
4. Diğer monitörler için aynı adımları uygulayın.
5. Seçili monitörün videosunu durdurmak için **Durdur** düğmesini kullanın.

Eklenen videolar uygulamanın yönettiği koleksiyona kopyalanır. Orijinal dosya taşınsa bile bu kopya kullanılabilir. Videolar ve JSON ayarları `%LocalAppData%\HareketliDuvarKagidi` altında tutulur. Bağlantısı kesilen monitörün ayarları saklanır.

## Sistem tepsisi ve otomatik başlangıç

Pencerenin **X** düğmesi yönetim penceresini gizler. Oynatmanın arka planda devam etmesi amaçlanır. Sistem tepsisindeki menüden **Aç**, **Tümünü durdur** ve **Çıkış** işlemlerine ulaşabilirsiniz.

**Windows ile başlat** ayarı varsayılan olarak açıktır. Geçerli kullanıcının başlangıç kaydını kullanır ve yönetici izni istemez. Otomatik açılışta yönetim penceresi gizlenir, kayıtlı oynatma tercihleri geri yüklenir. Kullanıcının durdurduğu ekranlar durdurulmuş kalır.

## Kullanılan teknolojiler

- C#, .NET 10 ve WPF
- LibVLCSharp 3.9.4
- VideoLAN.LibVLC.Windows 3.0.23
- Windows API ve native HWND video yüzeyleri
- Sistem tepsisi için Windows Forms NotifyIcon
- JSON ayar saklama ve Inno Setup kurulum betikleri

## Kaynak koddan derleme

Windows üzerinde .NET 10 SDK ve Git gerekir. NuGet bağımlılıkları derleme sırasında geri yüklenir.

```powershell
git clone https://github.com/OrucAltundag/Hareketli-Duvar-Kagidi.git
cd Hareketli-Duvar-Kagidi
dotnet build -c Release
dotnet run -c Release
```

Windows x64 için self-contained yayın oluşturmak üzere

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=false -p:PublishTrimmed=false -o dist/1.0.1/win-x64
```

Inno Setup 6 kuruluysa aşağıdaki betik yayın paketini ve kurulum dosyasını oluşturur.

```powershell
powershell -ExecutionPolicy Bypass -File installer/Build.ps1
```

Çıktı `dist/1.0.1/HareketliDuvarKagidi-1.0.1-win-x64-Setup.exe` yolundadır. Derleme çıktıları ve kurulum dosyaları Git geçmişine dahil edilmez.

## Ekran görüntüleri

Henüz gerçek uygulama ekran görüntüsü eklenmemiştir.

## Bilinen sınırlamalar

- Masaüstüne bağlanma Explorer'ın WorkerW/Progman yapısına bağlıdır. Explorer yeniden başlatıldığında otomatik onarım uygulanmaz.
- Yansıtılmış ekranlarda bağımsız videolar için Windows ekran düzeni Genişlet olarak değiştirilmelidir.
- Video döngüsünde tam kesintisizlik garanti edilmez. Sonuç codec, dosya ve donanıma bağlıdır.
- 1.0.0 kurulum testinde iki monitörde oynatma ve X sonrası tepsi davranışıyla ilgili sorun bildirilmiştir. Sorunlar sonraki incelemede yeniden üretilememiştir. 1.0.1 sınırlı tanılama kaydı içerir.
- 1.0.1 regresyon testinde iki monitörde kare ilerlemesi, döngü, bağımsız modlar ve tepsi komutları doğrulanmıştır. Yeni kurulum paketinin uçtan uca kurulumu ve Windows oturumuyla otomatik başlangıç doğrulanmamıştır.
- Üç veya daha fazla fiziksel monitör, karma DPI ve tüm bağlantı değiştirme senaryoları kapsamlı donanım testinden geçmemiştir.
- Monitör kimliği bağlantı noktası veya sürücü değişikliğiyle değişebilir.

Tanılama kaydı `%LocalAppData%\HareketliDuvarKagidi\diagnostic.log` yolundadır.

## Lisans ve üçüncü taraf bileşenler

Bu proje için henüz bir lisans seçilmemiştir.

Kullanılan NuGet paketlerinin lisans bilgileri ve dağıtım sınırlamaları [üçüncü taraf bildirimlerinde](THIRD_PARTY_NOTICES.md) açıklanmıştır.
