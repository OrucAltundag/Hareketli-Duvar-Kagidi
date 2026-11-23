#include <stdio.h>
#include <string.h>
#include "libavcodec/avcodec.h"
int main(void) {
    const AVCodec *codec = avcodec_find_encoder_by_name("libmp3lame");
    if (!codec) return 1;
    AVCodecContext *ctx = avcodec_alloc_context3(codec);
    if (!ctx) return 2;
    ctx->sample_fmt = AV_SAMPLE_FMT_S16P;
    ctx->sample_rate = 44100;
    ctx->channels = 2;
    ctx->channel_layout = AV_CH_LAYOUT_STEREO;
    ctx->bit_rate = 128000;
    if (avcodec_open2(ctx, codec, NULL) < 0) return 3;
    AVFrame *frame = av_frame_alloc();
    AVPacket *pkt = av_packet_alloc();
    if (!frame || !pkt) return 4;
    frame->nb_samples = ctx->frame_size;
    frame->format = ctx->sample_fmt;
    frame->channel_layout = ctx->channel_layout;
    frame->sample_rate = ctx->sample_rate;
    if (av_frame_get_buffer(frame, 0) < 0) return 5;
    for (int ch = 0; ch < 2; ++ch)
        memset(frame->data[ch], 0, frame->nb_samples * sizeof(short));
    int packets = 0, bytes = 0;
    for (int i = 0; i <= 12; ++i) {
        if (avcodec_send_frame(ctx, i == 12 ? NULL : frame) < 0) return 6;
        int rc;
        while ((rc = avcodec_receive_packet(ctx, pkt)) == 0) {
            if (pkt->size < 4 || pkt->data[0] != 255 || (pkt->data[1] & 224) != 224) return 7;
            bytes += pkt->size;
            packets++;
            av_packet_unref(pkt);
        }
        if (rc != (i == 12 ? AVERROR_EOF : AVERROR(EAGAIN))) return 8;
    }
    printf("packets=%d bytes=%d\n", packets, bytes);
    av_frame_free(&frame);
    av_packet_free(&pkt);
    avcodec_free_context(&ctx);
    return packets > 0 ? 0 : 9;
}
