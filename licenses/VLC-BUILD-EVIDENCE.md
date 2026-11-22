# Windows v1.0.1 dağıtımındaki son teknik kontroller

## Bağlanan kod ve yamalar

VideoLAN'ın [Win32 sembol arşivi](https://download.videolan.org/pub/videolan/vlc/3.0.23/win32/vlc-3.0.23-win32-debugsym.7z) ve [Win64 sembol arşivi](https://download.videolan.org/pub/videolan/vlc/3.0.23/win64/vlc-3.0.23-win64-debugsym.7z) kullanıldı. Her satır için dağıtılan DLL'nin PE `.gnu_debuglink` bölümündeki dosya adı ve CRC32, ilgili DBG dosyasıyla eşleştirildi. Aşağıdaki işlevlerin her biri iki mimaride de COFF sembol tablosunda DLL'nin kendi `.text` bölümünde tanımlı.

| DLL | İkiliye bağlanmış kodun kanıtı | DBG CRC32 (x86 / x64) |
| --- | --- | --- |
| libavcodec_plugin.dll | GSM `gsm_create`, OpenJPEG `opj_create_decompress`, LAME `lame_init` | 8b3aa726 / 4a6cb44b |
| liblibbluray_plugin.dll | libbluray `bd_open`, FreeType `FT_Init_FreeType`, libxml2 `xmlReadMemory` | 98d28edc / 84ea9e1e |
| libvlccore.dll | libgcrypt `gcry_check_version`, libgpg-error `gpg_strerror` | 9c1dad7e / f6c9e4dc |

İki libavcodec DBG dosyasında da `DXVA_PicParams_HEVC_Rext` tipi bulundu. Temel FFmpeg 4.4.5 arşivinin `libavcodec/dxva2_hevc.c` dosyasında bu tip yok. VLC revizyonundaki `contrib/src/ffmpeg/0001-avcodec-dxva2_hevc-add-support-for-parsing-HEVC-Rang.patch` aynı tipi ekliyor. Bu, değişikliğin derlemeye yansıdığına dair somut bir izdir. Uygulanan tüm yamaların sırası ve araç zincirinin sağladığı girdiler hâlâ açık.

## BD-J sınıflarını yeniden üretme

Tüm 650 DLL için ayrı sembol eşleşmeleri `VLC-DEBUGLINK-MAP.json` dosyasındadır. `contrib_path_components`, eşleşen DBG içindeki `/contrib/contrib-win32/<dizin>/` veya `/contrib/contrib-win64/<dizin>/` yollarından çıkarılan dizin adlarıdır. Boş dizi, o biçimde bir yol görülmediğini belirtir. Bu alan bir bağlayıcı haritası değildir. Örneğin GSM'nin bağlantısı, dizin adı yerine yukarıdaki işlev tanımıyla gösterildi.

Kaynak ZIP'ine 21 arşiv eklendi. Bunlar arasında gettext, iconv, Fontconfig, FluidLite, SDL/SDL_image, libssh2 ve pupnp bulunur. 20 arşivin SHA-512 değeri bu VLC revizyonundaki tarife eşleşti. FluidLite, tarifte sabitlenen `238997654efb20e736512847f3f5f6d618de9423` revizyonundan alındı. Yayın paketinin eksiksiz karşılık gelen kaynak iddiası açık kalmaktadır.

### BD-J derleme girdileri

Kaynak ZIP'indeki libbluray 1.4.0 arşivi kullanıldı. Dağıtılan manifestler Ant 1.10.9 ve Debian OpenJDK 11.0.29 bildiriyor. Karşılaştırmada Windows x64 Temurin 11.0.29+7 kullanıldı. `src/libbluray/bdj/meson.build`, `bdj-jar` hedefini Ant'a devrediyor. `build.xml` içindeki `dist` hedefi ASM ve BD-J kaynaklarını derleyip AWT sınıflarını ayrı JAR'a ayırıyor.

1. Boş bir sınıf çıktı dizini oluşturun. `contrib/asm/src` altındaki Java dosyalarının sıralı listesini bir javac argüman dosyasına yazın.
2. `javac -g -source 6 -target 6 -XDignore.symbol.file -encoding UTF-8 -classpath CLASSES -d CLASSES @asm.args` ile derleyin.
3. Aynı komutu `src/libbluray/bdj/{java,java-j2se,java-build-support}` altındaki Java dosyalarından oluşturulan listeyle tekrarlayın. İki aşamada aynı CLASSES dizinini kullanın.
4. Her dağıtılan JAR'daki her `.class` girdisini çıktı dizinindeki aynı göreli yolla bayt düzeyinde karşılaştırın.

Her mimaride AWT JAR'ındaki 34 ve ana JAR'daki 754 sınıfın tamamı eşleşti. Sınıflar dışında yalnızca manifest ve dizin girdileri vardı. Böylece dağıtılan çalıştırılabilir Java içeriğinin kaynak karşılığı ve yeniden derleme yolu gösterildi. JAR kapsayıcısının zaman damgaları, sıkıştırması ve özgün CI komut satırı karşılaştırma kapsamı dışındadır. VLC'nin libbluray için taşıdığı GDI32 yaması yerel Meson bağlantı ayarını değiştirir.

## Değiştirilmiş kütüphaneyle oynatma

Yalıtılmış Windows x64 testinde yayımlanmış `HareketliDuvarKagidi.dll` doğrudan kullanıldı. Kaynak ZIP'indeki LibVLCSharp 3.9.4'ün masaüstü `Shared/**/*.cs` kaynaklarından aynı ad ve sürümle test DLL'si derlendi (.NET 10, DESKTOP tanımı). `LibVLC.Version` sonucuna `wrapper-test` eklendi. Kopyalanan native libvlc.dll ve libvlccore.dll içindeki eşit uzunluktaki sürüm etiketi de yalnızca test kopyasında `AuditRun` yapıldı.

Test sürücüsü bu native dizini `Core.Initialize` ile seçti ve yayımlanmış uygulama sınıfı `WallpaperPlayback.Start` üzerinden bir MP4 oynattı. İki etiket birlikte okundu (`3.0.23 AuditRun wrapper-test`), bir video çıkışı ve 15 gösterilen kare ölçüldü. Değiştirilmiş yönetilen kütüphane kaynak ZIP'inden üretildi. Native değişiklik sürüm etiketi düzeyindedir. Test, kurulu uygulamanın tamamını veya statik FFmpeg/LAME eklentisini yeniden bağlama deneyi değildir.

## Açık dağıtım kararları

- Her iki mimari için fiili contrib/araç zinciri manifesti, tam statik bağlantı girdileri ve uygulanan yama listesi veya aynı girdileri gösteren yeniden üretilebilir derleme kaydı gerekir. Referans işler [2403066](https://code.videolan.org/videolan/vlc/-/jobs/2403066) ve [2403067](https://code.videolan.org/videolan/vlc/-/jobs/2403067).
- Her statik LGPL bileşeni için seçilen dağıtım yoluna uygun değiştirilebilir kütüphane, derleme betikleri ve gerekiyorsa yeniden bağlanabilir nesneler sağlanmalıdır. Bir bileşeni değiştirip eklentiyi yeniden bağlayarak gerçek oynatmayla sınamak hâlâ gereklidir.
- GPL etkin FFmpeg'in VLC eklentisine statik katıldığı teknik olarak doğrulandı. Uygulama, VLC ve eklentinin birlikte dağıtımında eserin kapsamı, dağıtım koşulları ve karşılık gelen kaynakların yeterliliği için hukuki değerlendirme gerekir. Uygulama için lisans seçimi yapılmadı.
