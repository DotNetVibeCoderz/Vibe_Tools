# LPRNet - License Plate Recognition System

## Deskripsi / Description

**Bahasa Indonesia**
LPRNet adalah aplikasi pengenalan pelat nomor (License Plate Recognition) modern yang dibangun menggunakan C#, Avalonia UI, dan Entity Framework Core. Aplikasi ini dirancang untuk mendeteksi pelat nomor dari input gambar/video, melakukan OCR (Optical Character Recognition), dan menyimpan log pembacaan ke database SQLite.

Proyek ini mencakup implementasi "Mock/Simulation" yang menggunakan **OpenCvSharp** dan **Tesseract** (opsional) untuk mendemonstrasikan alur kerja LPR tanpa memerlukan lisensi komersial (seperti SimpleLPR) agar dapat langsung dijalankan dan dikompilasi.

**English**
LPRNet is a modern License Plate Recognition application built with C#, Avalonia UI, and Entity Framework Core. It is designed to detect license plates from image/video inputs, perform OCR (Optical Character Recognition), and store reading logs into a SQLite database.

This project includes a "Mock/Simulation" implementation using **OpenCvSharp** and **Tesseract** (optional) to demonstrate the LPR workflow without requiring commercial licenses (like SimpleLPR), ensuring it can be compiled and run immediately.

## Fitur / Features

- **Cross-Platform UI:** Menggunakan Avalonia UI (Windows, Linux, macOS).
- **Plate Detection Simulation:** Menggunakan algoritma pemrosesan citra dasar dengan OpenCvSharp (Grayscale -> Threshold -> Contours).
- **Database Logging:** Menyimpan riwayat pelat nomor dengan waktu dan tingkat kepercayaan (Confidence) ke SQLite.
- **Search:** Fitur pencarian riwayat pelat nomor.
- **Mock Engine:** Jika tidak ada input kamera nyata, aplikasi akan menghasilkan gambar sintetis untuk simulasi deteksi.

## Persyaratan / Requirements

- .NET 8.0 SDK
- (Opsional) Tesseract Language Data (`eng.traineddata`) jika ingin mengaktifkan OCR nyata (saat ini menggunakan simulasi hasil).

## Cara Menjalankan / How to Run

1.  Pastikan .NET 8.0 SDK terinstal.
2.  Buka terminal di folder proyek.
3.  Jalankan perintah:
    ```bash
    dotnet run
    ```
4.  Klik tombol "Capture / Simulate Feed" untuk mensimulasikan pembacaan pelat.

## Catatan Teknis / Technical Notes

Aplikasi ini menggunakan `SimpleLprService` yang sebenarnya merupakan wrapper simulasi. Untuk menggunakan **SimpleLPR** versi komersial:
1.  Beli lisensi SimpleLPR SDK.
2.  Tambahkan referensi DLL native SimpleLPR.
3.  Ubah logika di `Services/LprService.cs` untuk memanggil engine SimpleLPR yang sesungguhnya.

This app uses `SimpleLprService` which is actually a simulation wrapper. To use the commercial **SimpleLPR** version:
1.  Purchase the SimpleLPR SDK license.
2.  Add references to the SimpleLPR native DLLs.
3.  Modify the logic in `Services/LprService.cs` to call the actual SimpleLPR engine.

## Struktur Proyek / Project Structure

- `Views/`: Antarmuka pengguna (Avalonia XAML).
- `ViewModels/`: Logika presentasi (MVVM).
- `Services/`: Logika pemrosesan LPR (OpenCvSharp).
- `Models/`: Definisi data (PlateRecord).
- `Database/`: Konfigurasi Entity Framework SQLite.

---
Created by **Jacky the Code Bender** (Gravicode Studios).
