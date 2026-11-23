#!/usr/bin/env bash
# Kullanım: bash static-relink.sh /yol/archives x86_64  (veya i686)
# Windows Git Bash, GNU make ve LLVM-MinGW bin dizinleri PATH üzerinde olmalı.
set -eu
here=$(cd "$(dirname "$0")" && pwd)
archives=$(cd "$1" && pwd)
case "$2" in
    i686) arch=x86 ;;
    x86_64) arch=x86_64 ;;
    *) exit 2 ;;
esac
target="$2-w64-mingw32"
D=$(mktemp -d)
echo "Test dizini: $D"
cd "$D"
tar xf "$archives/lame-3.100.tar.gz"
tar xf "$archives/ffmpeg-4.4.5.tar.xz"
cp "$here/static-relink-probe.c" probe.c
cd "$D/lame-3.100"
if [[ $arch == x86 ]]; then
    vlc=vlc-79128878ddb2c280bbb6c89c76a46b31a80ade1c
    tar -xOf "$archives/$vlc.tar.gz" "$vlc/contrib/src/lame/sse.patch" > "$D/sse.patch"
    patch -p1 < "$D/sse.patch"
fi
make --version | head -1
"$target-gcc" --version | head -1
./configure --host="$target" --disable-shared --enable-static \
    --disable-frontend --disable-decoder >"$D/lcfg.log" 2>&1
make -j12 -C libmp3lame >"$D/lmake.log" 2>&1
mkdir -p "$D/prefix/include/lame" "$D/prefix/lib"
cp include/lame.h "$D/prefix/include/lame/"
cp libmp3lame/.libs/libmp3lame.a "$D/prefix/lib/"
cd "$D/ffmpeg-4.4.5"
./configure --arch="$arch" --target-os=mingw32 --cross-prefix="$target-" \
    --disable-everything --disable-autodetect --disable-network --disable-programs \
    --disable-doc --disable-debug --disable-x86asm --enable-gpl --enable-static \
    --disable-shared --disable-avdevice --disable-avformat --disable-avfilter \
    --disable-swscale --disable-swresample --disable-postproc \
    --enable-libmp3lame --enable-encoder=libmp3lame \
    --extra-cflags="-I$(cygpath -m "$D/prefix/include")" \
    --extra-ldflags="-L$(cygpath -m "$D/prefix/lib")" >"$D/fcfg.log" 2>&1
make -j12 libavcodec/libavcodec.a libavutil/libavutil.a >"$D/fmake.log" 2>&1
cd "$D"
CC="$target-gcc"
F=ffmpeg-4.4.5
$CC -c -I$F probe.c -o probe.o
link() {
 $CC -o "$1.exe" probe.o -Wl,--start-group "$F/libavcodec/libavcodec.a" "$F/libavutil/libavutil.a" "$2" -Wl,--end-group -lbcrypt -luser32 -lole32 -luuid -lws2_32 -lpsapi -lsecur32 -lm
 "./$1.exe" >"$1.out" 2>"$1.err"
 cat "$1.out"
}
link baseline prefix/lib/libmp3lame.a
sha256sum probe.o "$F/libavcodec/libavcodec.a" "$F/libavutil/libavutil.a" > fixed.sha256
L=lame-3.100/libmp3lame
sed -i '/^lame_encode_buffer(lame_global_flags/,/^}/s/^    return lame_encode_buffer_template/    fputs("HDK-relink-called\\n", stderr);\n    return lame_encode_buffer_template/' "$L/lame.c"
grep HDK-relink "$L/lame.c"
make -j12 -C "$L" >changed.log 2>&1
link changed "$L/.libs/libmp3lame.a"
sha256sum -c fixed.sha256
if grep -q HDK-relink-called baseline.err; then exit 1; fi
grep -c HDK-relink-called changed.err
sha256sum probe.o "$F/libavcodec/libavcodec.a" "$F/libavutil/libavutil.a" prefix/lib/libmp3lame.a "$L/.libs/libmp3lame.a"
