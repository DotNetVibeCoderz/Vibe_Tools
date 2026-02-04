# CardFormatter

**CardFormatter** is a cross-platform MicroSD management tool. It allows you to format cards, verify their true capacity (to detect fake cards), and securely erase data.

## Features

- **Format**: Supports FAT32, exFAT, and NTFS.
- **Verify Capacity**: Detects counterfeit cards by writing and verifying data across the entire volume.
- **Secure Erase**: Overwrites data to prevent recovery.
- **Cross-Platform**: Runs on Windows, Linux, and macOS (requires .NET Runtime).

## How to Run

1. Ensure .NET 8.0 SDK or Runtime is installed.
2. Open a terminal in the project folder.
3. Run the application:
   ```bash
   dotnet run
   ```
   *Note: Formatting drives usually requires Administrator/Root privileges.*
   *   **Windows**: Run Command Prompt/PowerShell as Administrator.
   *   **Linux/Mac**: Run `sudo dotnet run`.

## Warning
This tool deletes data. Use with caution. Always verify you have selected the correct drive letter/path before confirming operations.

---

# CardFormatter (Bahasa Indonesia)

**CardFormatter** adalah alat manajemen MicroSD lintas platform. Aplikasi ini memungkinkan Anda untuk memformat kartu memori, memverifikasi kapasitas sebenarnya (untuk mendeteksi kartu palsu), dan menghapus data secara aman.

## Fitur

- **Format**: Mendukung FAT32, exFAT, dan NTFS.
- **Verifikasi Kapasitas**: Mendeteksi kartu palsu dengan menulis dan memverifikasi data di seluruh volume.
- **Penghapusan Aman (Secure Erase)**: Menimpa data sehingga tidak dapat dipulihkan.
- **Lintas Platform**: Berjalan di Windows, Linux, dan macOS (membutuhkan .NET Runtime).

## Cara Menjalankan

1. Pastikan .NET 8.0 SDK atau Runtime sudah terinstal.
2. Buka terminal di folder proyek.
3. Jalankan aplikasi:
   ```bash
   dotnet run
   ```
   *Catatan: Memformat drive biasanya memerlukan hak akses Administrator/Root.*
   *   **Windows**: Jalankan Command Prompt/PowerShell sebagai Administrator.
   *   **Linux/Mac**: Jalankan `sudo dotnet run`.

## Peringatan
Alat ini menghapus data. Gunakan dengan hati-hati. Selalu pastikan Anda telah memilih drive yang benar sebelum mengonfirmasi operasi.

---
**Created by Jacky the Code Bender @ Gravicode Studios**
