# MediaCenter

**[English]**
MediaCenter is a simple yet powerful media player application built using C# (Windows Forms) and LibVLCSharp. It supports a wide range of audio and video formats, network streaming, and visualization.

**[Bahasa Indonesia]**
MediaCenter adalah aplikasi pemutar media yang sederhana namun canggih, dibuat menggunakan C# (Windows Forms) dan LibVLCSharp. Aplikasi ini mendukung berbagai format audio dan video, streaming jaringan, serta visualisasi audio.

---

## Features / Fitur

### English
*   **Universal Playback**: Plays almost any media file (MP4, MKV, MP3, FLAC, AVI, etc.) thanks to the VLC engine.
*   **Network Streaming**: Supports playing media from URLs (HLS, Radio, etc.).
*   **Audio Visualizations**: Includes built-in visualizers like Spectrum and Scope.
*   **Snapshots**: Capture screenshots from playing videos easily.
*   **Media Info**: View metadata information (Artist, Title, Duration).
*   **Keyboard Shortcuts**: Fully controllable via keyboard.

### Bahasa Indonesia
*   **Pemutaran Universal**: Memutar hampir semua file media (MP4, MKV, MP3, FLAC, AVI, dll) berkat mesin VLC.
*   **Streaming Jaringan**: Mendukung pemutaran media dari URL (HLS, Radio, dll).
*   **Visualisasi Audio**: Termasuk visualisasi bawaan seperti Spektrum dan Scope.
*   **Snapshot**: Mengambil tangkapan layar dari video yang sedang diputar dengan mudah.
*   **Info Media**: Melihat informasi metadata (Artis, Judul, Durasi).
*   **Pintasan Keyboard**: Dapat dikontrol sepenuhnya melalui keyboard.

---

## Keyboard Controls / Kontrol Keyboard

| Action / Aksi | Shortcut / Pintasan |
| :--- | :--- |
| **Play / Pause** | `Space` |
| **Stop** | `Esc` |
| **Seek Forward (+10s)** | `Right Arrow` / `Ctrl + Right` |
| **Seek Backward (-10s)** | `Left Arrow` / `Ctrl + Left` |
| **Volume Up** | `Up Arrow` |
| **Volume Down** | `Down Arrow` |
| **Take Snapshot** | `Ctrl + S` |

---

## Requirements / Persyaratan

*   .NET 8.0 (or compatible .NET Core version)
*   Windows OS (x64/x86)
*   LibVLCSharp
*   LibVLCSharp.WinForms

## How to Run / Cara Menjalankan

1.  Open the solution in Visual Studio or your preferred C# IDE.
2.  Ensure NuGet packages are restored (`LibVLCSharp` and `LibVLCSharp.WinForms`).
3.  Build and Run the project.
4.  Use **File -> Open** to play a local file or **File -> Open Network Stream** to play from a URL.

---

## Credits / Kredit

This project uses **LibVLCSharp**, a .NET binding for **libvlc** (the engine of VLC media player).
Proyek ini menggunakan **LibVLCSharp**, binding .NET untuk **libvlc** (mesin pemutar media VLC).

Created by **Jacky the Code Bender** (Gravicode Studios).
