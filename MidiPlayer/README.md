# Jacky's Retro Midi Player

Halo! Ini adalah Midi Player retro buatan Jacky the code bender.

## Fitur
- 🎵 Playback Midi File (.mid)
- 🎛️ SoundFont Synthesis (High Quality Audio)
- 📼 Playlist Support (Open/Save JSON)
- 🔊 Volume & Tempo Control
- 🖥️ Cross-Platform (Windows, Linux, Mac) via .NET 8 & Avalonia UI
- 🕹️ Tampilan Retro Dark Mode

## Cara Menjalankan
1. Pastikan .NET 8 SDK terinstall.
2. Buka terminal di folder project.
3. Jalankan command:
   ```bash
   dotnet run
   ```

## Note for Linux/Mac Users
Aplikasi ini menggunakan output audio system. Jika di Linux tidak ada suara, pastikan `pulseaudio` atau `alsa` terkonfigurasi dengan baik, karena library NAudio digunakan sebagai audio driver.

## Credits
- **MeltySynth** untuk Midi rendering.
- **Avalonia UI** untuk Interface.
- **NAudio** untuk Audio Output.

Jangan lupa traktir pulsanya ya bos! 😄
https://studios.gravicode.com/products/budax
