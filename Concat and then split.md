# 拼接视频源再裁剪的方法

## FFmpeg

### `concat` 滤镜 + `trim` 滤镜（适合格式不同或需精准帧控制）

```bash
ffmpeg -i source1.mp4 -i source2.mp4 -filter_complex \
"[0:v][0:a][1:v][1:a]concat=n=2:v=1:a=1[vconcat][aconcat]; \
[vconcat]trim=start=10:end=30,setpts=PTS-STARTPTS[vout]; \
[aconcat]atrim=start=10:end=30,asetpts=PTS-STARTPTS[aout]" \
-map "[vout]" -map "[aout]" -c:v libx264 -c:a aac output.mp4
```

2. 命令行：

```bash
ffmpeg -f concat -safe 0 -i files.txt -ss 00:00:10 -to 00:00:30 -c copy output.mp4
```

* **官方文档来源：**
* FFmpeg Concat Filter: [https://ffmpeg.org/ffmpeg-filters.html#concat](https://www.google.com/search?q=https://ffmpeg.org/ffmpeg-filters.html%23concat)
* FFmpeg Concat Demuxer: [https://ffmpeg.org/ffmpeg-formats.html#concat](https://www.google.com/search?q=https://ffmpeg.org/ffmpeg-formats.html%23concat)


---

## VapourSynth (`vspipe`)

### 脚本 (`script.vpy`)

```python
import vapoursynth as vs
core = vs.core

# 载入多个源（不用这个，用项目里的序列生成方法）
clip1 = core.lsmas.LWLibavSource("source1.mp4")
clip2 = core.lsmas.LWLibavSource("source2.mp4")

# 拼接视频 (加号 '+' 或 core.std.Splice)
src = clip1 + clip2

# 裁剪指定帧区间 (例如从 300 帧裁剪到 1200 帧)
src = src[300:1200]

src.set_output()
```

* **官方文档来源：**
* VapourSynth Splice API: [http://www.vapoursynth.com/doc/functions/video/splice.html](https://www.google.com/search?q=http://www.vapoursynth.com/doc/functions/video/splice.html)
* VapourSynth Python Reference: [http://www.vapoursynth.com/doc/pythonreference.html](https://www.google.com/search?q=http://www.vapoursynth.com/doc/pythonreference.html)

---

### 3. AviSynth (`avs2yuv` / `avs2pipemod`)

### 脚本 (`script.avs`)

```avisynth
# 载入多个源
clip1 = FFVideoSource("source1.mp4")
clip2 = FFVideoSource("source2.mp4")

# 拼接视频 (++ 为 UnalignedSplice，+ 为 AlignedSplice)
src = clip1 ++ clip2

# 裁剪指定帧区间 (帧数 300 至 1200)
Trim(src, 300, 1200)
```

* **官方文档来源：**
* AviSynth Trim: [http://avisynth.nl/index.php/Trim](http://avisynth.nl/index.php/Trim)
* AviSynth Splice: [http://avisynth.nl/index.php/Splice](http://avisynth.nl/index.php/Splice)
* avs2pipemod GitHub: [https://github.com/myrsloik/avs2pipemod](https://www.google.com/search?q=https://github.com/myrsloik/avs2pipemod)