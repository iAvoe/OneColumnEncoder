# 1cenc Import and Check System

This document explains the video import and check system.

---

## 1. Terms

| Term | Explanation |
|---|---|
| Source | The video file you import. The program mainly cares about the picture stream, not audio. |
| Multi-file modes | Queue, Concat, and Repart together. |
| ffprobe | A tool that reads video parameters such as resolution, frame rate, pixel format, color, and field order. |
| Checklist | The program checks each source item by item: metadata, progressive, bit-depth, constant frame rate, YUV420, chroma, etc. |
| Error / Warning | Error disables the Start Encoding button. Warning only reminds you and does not disable it. |
| Reject non-whitelisted | If any selected file is not in the whitelist, the whole import is rejected and an error is shown. |
| Reference file | The baseline file in multi-file modes. Other files are compared with it. |
| CFR / VFR | CFR is constant frame rate. VFR is variable frame rate. Many tools do not support VFR. |
| Progressive / interlaced | Progressive is usually fine. Interlaced is an old scanning method and modern tools usually do not support it. |
| Parameter fingerprint | A summary of video parameters used to decide whether multiple files are exactly consistent. |
| Audio mux | Copy audio directly or re-encode it. Concat and Repart default to re-encoding as AAC 320 kbps to avoid mute or sync issues. |

## 2. Rules Common to All Modes

| Rule | Explanation | Effect |
|---|---|---|
| Route priority | Queue > Concat > Repart > Single Video | When multiple modes are active at the same time, the mode to use is decided in this order |
| Dialog extension filter | Whitelist of about 90 entries; scripts support `.avs`, `.vpy`, `.ini` | The file dialog only shows files with video extensions |
| Effect of All files | Every dialog has an All files option | Any file can still be force-selected |
| Checklist | Metadata, progressive, bit-depth, CFR, square pixels, color, chroma, YUV420, etc. | Every source is checked; errors disable the Start Encoding button, warnings do not |
| Metadata | Must not be unknown, unspecified, or reserved | Abnormal metadata usually means the source may be damaged |
| Progressive | Empty field order, progressive, or unknown counts as a pass | Failure is usually a warning |
| bit-depth | 8 and 10 are supported; SVT-AV1 supports up to 10-bit; other encoders support up to 12-bit | Extra-high bit-depths may be unsupported |
| CFR | Average frame rate must be non-empty, non-zero, and equal to the real frame rate | Queue Mode performs an extra check |
| YUV420 | Pixel format must contain 420 | Failure → warning; becomes an error when SVT-AV1 is selected |
| Square pixels | Pixels must not be rectangular | Failure → warning with a suggestion to add a fix-up filter |
| Chroma sampling | The sampling position must be supported by the tools | SVT-AV1 supports fewer positions; failure is a warning |
| Mode card availability | Without ffprobe, all video source cards are disabled; when certain upstream tools are selected, the multi-file mode cards are disabled | Modes are proactively disabled when tools are incomplete |
| Pre-start checks | Queue/Repart check for missing sources and duplicate outputs; all routes have overwritten confirmation; route support is checked | Missing files, duplicate output names, or unsupported routes cause errors |
| Script-to-video validation | Performed for Single Video/Queue; skipped for Concat/Repart | Concat scripts carry their own clip list; Repart does not require a script source |
| Drag-and-drop import | Video files can be dropped onto the main window; 1 file goes to Single Video mode, several files go to Queue mode | Equivalent to a file dialog import; an already imported source is confirmed first |

## 3. Check Differences Across the Four Import Modes

| Mode | Import method | Minimum sources | Second extension check | Cross-file strictness | Script requirement | Audio default |
|---|---|---|---|---|---|---|
| Single Video | Single file | 1 | None | None | Single script source | — |
| Queue Mode | Multi-select, Blu-ray PLAYLIST | More than 0 | Yes; non-whitelisted files cause an error | Medium | Script queue folder | — |
| Concat Mode | Multi-select | At least 2 | Yes; non-whitelisted files cause an error | Fairly strict | Script-embedded clip list | Re-encode as AAC 320 kbps |
| Repart Mode | File import, chapter folder | More than 0 | Yes; non-whitelisted files cause an error | Strict | No script source required | Re-encode as AAC 320 kbps |

## 4. Multi-file Modes: Whether Sources Can Be Used Together

| Check | Queue Mode | Concat Mode | Repart Mode | Interpretation |
|---|---|---|---|---|
| Reference file | Typical-stream mode votes by duration² ÷ frame count²; First-stream mode takes the first successfully analyzed file | First successfully analyzed file | Group with the largest total bytes; ties broken by count | Items differing from the baseline stream are excluded |
| Resolution | Width participates in comparison | Mismatch disables the Start Encoding button | Both width and height cause hard exclusion | Concat and Repart are sensitive to resolution |
| Frame rate | Average frame rate and real frame rate participate in comparison | Mismatch is a warning | Non-CFR is excluded | Inconsistent frame rates may cause stuttering or failure |
| VFR | CFR is required | Suggests adding a fix-up filter | Excluded directly | Queue is strict, Concat reminds you, Repart hard-excludes |
| Interlaced | Checklist | Checklist | Flagged as interlaced, prompts to keep or exclude; cancelling aborts the import | Repart will ask you |
| Codec / pixel format | Checked via parameter fingerprint | Inconsistency is a warning | Inconsistency causes hard exclusion | Repart is the strictest |
| Color / chroma / bit-depth | Participate in comparison | Affect warnings | Hard exclusion | The stricter the mode, the easier it is to be excluded on parameter mismatch |
| File stability | — | — | Checks file size and modification time | Repart checks whether files have changed |
| Total frame count | Queue result is usable | Accumulated; missing frame counts → error | Obtained per file; failures are excluded | Concat needs the total length |

## 5. Mode-Specific Rules

### 5.1 Single Video Mode

| Item | Rule |
|---|---|
| Import | Select only one video |
| Post-selection extension check | None (files selected via All files are accepted) |
| Cross-file compatibility | None |
| Analysis condition | Both the tool path and the source path must be non-empty |
| Analysis failure | Produces an error that blocks starting |
| Special upstream tools | When certain upstream tools are used, they override the script configuration |
| Duration filter | Not applicable |
| Start | An error is reported if the upstream request is empty |
| Script-to-video validation | Yes |

### 5.2 Queue Mode

| Item | Rule |
|---|---|
| Import | Multi-file dialog; Blu-ray PLAYLIST supported |
| Non-whitelisted extensions | Error and abort the import |
| Empty result | A warning is shown if no file was selected |
| Mixed folders | May cause an error |
| Per-file analysis | Each file is checked item by item |
| Single-file failure | Skipped with an error shown |
| All files failed | Cannot continue |
| Filter modes | When there is more than 1 file, prompts for First-stream or Typical-stream |
| Acceptance condition | Closing the filter accepts everything; otherwise candidates must match the reference |
| Actual requirements | Same checklist status, same width, same normalized average and real frame rates |
| Not required | Height does not participate in comparison, but color information does |
| Reference selection | Typical-stream mode votes by duration² ÷ frame count²; First-stream mode takes the first file |
| Result saving | Accepted and excluded results are written to the queue file |
| Duration filter | Queue only; 30 seconds by default; a warning is shown if filtering leaves nothing |
| Queue file check | Missing files, no entries, or invalid content all cause errors |
| Pre-start checks | Missing sources, duplicate outputs |
| Script validation | Script-to-video matching is enforced |

### 5.3 Concat Mode

| Item | Rule |
|---|---|
| Non-whitelisted extensions | Error and abort the import |
| Minimum count | Fewer than 2 files is an error |
| Same extension | All files must share the same extension, case-insensitive |
| VFR | Warning per file, does not block |
| Resolution mismatch | Blocks; the import aborts, and re-analysis also reports an error |
| Codec / pixel format mismatch | Warning |
| Frame rate mismatch | Warning |
| No video stream or missing width/height | Error |
| Single-file probe failure | May interrupt |
| All files failed | Cannot continue |
| Frame counting | Total frame count is accumulated; if any file lacks a frame count, the total length is incomplete |
| Reference file | The first successfully analyzed file |
| Re-analysis | Fewer than 2 files is an error |
| Start count | At least 2 files are confirmed again before starting |
| Script validation | Skipped |
| Audio mux | Re-encoded as AAC 320 kbps by default |

### 5.4 Repart Mode

#### Pre-check and Stage-by-stage Exclusion

| Stage | Check | Result | Explanation |
|---|---|---|---|
| Pre-check | Missing ffprobe | Cannot continue | No pot to cook with |
| Pre-check | No sources | Cannot continue | No rice to cook |
| File check | File does not exist | Excluded | File was deleted or moved |
| Lightweight analysis | Analysis failed | Excluded | File cannot be read |
| In-depth analysis | Analysis failed | Excluded | Video parameters cannot be read |
| Stream check | No video stream | Excluded | No picture stream |
| Stream check | Invalid width/height | Excluded | Abnormal dimensions |
| Stream check | Not CFR | Excluded | Must be constant frame rate |
| Stream check | Interlaced | Prompts to keep or exclude; cancelling aborts the import | Interlaced video asks you, as it may be pseudo-interlaced |
| Reference comparison | Parameter fingerprint mismatch | Excluded | Video parameters must match exactly |
| Frame count check | Invalid frame count | Excluded | Frame count cannot be determined |
| Frame count check | File was modified | Excluded | File size or modification time changed after analysis |
| Frame count check | Frame count estimate unverified | Prompts for confirmation; cancelling aborts the import | Asks you when the count is imprecise |

#### Parameter Fingerprint Details

| Category | Contents |
|---|---|
| Codec | Encoder, profile, level |
| Dimensions | Width, height, coded width, coded height |
| Pixels | Pixel format, bit-depth |
| Field order | Progressive or interlaced |
| Aspect ratio | Pixel aspect ratio |
| Frame rate | Average frame rate, real frame rate |
| Color | Color range, color space, transfer characteristics, primaries, chroma location |
| Extradata | Hash of encoder extradata |
| String handling | Case and whitespace are ignored |

#### Frame Count Acquisition Order

In speed order:

| Order | Method |
|---|---|
| 1 | Use cached frame count |
| 2 | Estimate from duration, with spot-check verification |
| 3 | Lossless remux for exact counting |
| 4 | Full frame counting, slower |
| 5 | Finally, binary search |

#### Plan and Edit Checks

| Item | Rule |
|---|---|
| Empty plan | Error and close |
| Stale plan | Files changed; warning and close |
| Whether files were modified | Checks file size and modification time |
| Deleting sources | Keep at least 1 source |
| Deleting outputs | Keep at least 1 output |
| Applying and closing | Cannot save without analysis or without outputs |
| Split-line range | Must not go out of bounds |
| Minimum clip | 1 frame |
| Start | Route support, valid requests, missing sources, duplicate outputs, overwrite confirmation |
| Script source | Not required |
| Script-to-video validation | Skipped |
| Audio mux | Re-encoded as AAC 320 kbps by default |

## 6. Main Window Drag-and-Drop Import

The main window (MainUI) accepts video files dragged in from the file explorer; the import rules are
equivalent to the file dialog import. The view layer (MainUI) owns the OLE drag events and the
"you can release now" feedback, the rule layer (MainVM) owns the import decision.

| Item | Rule |
|---|---|
| Drop area | The whole main window (including the scroll area) is a drop target |
| 1 video dropped | Imported into Single Video mode |
| Several videos dropped | Imported into Queue mode (multi-file mode) |
| Folders dropped | Folders are ignored; a warning is shown when no usable file is found |
| Extension check | Single Video mode does not re-check the extension (any file is accepted); Queue mode checks the whitelist and reports an error that aborts the import when a file is not whitelisted |
| Target card disabled | The import is refused with a message (ffprobe missing, or the selected upstream does not support queue mode) |
| A source is already imported | A confirmation dialog is shown; confirming replaces the source and continues, cancelling keeps the current source |
| Hover feedback | While a payload hovers the window, a highlight overlay with the target mode (Single Video / Queue) is shown to signal that the mouse can be released |
| After the import | Same as a file dialog import: the source analysis runs automatically |
| Not supported | Only video files are handled; script sources still have to be imported with the file dialog |
