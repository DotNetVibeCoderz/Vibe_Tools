# Network Scanner CLI

![License](https://img.shields.io/badge/license-MIT-blue.svg) ![Platform](https://img.shields.io/badge/platform-Cross%20Platform-green.svg) ![Language](https://img.shields.io/badge/language-C%23-purple.svg)

[Bahasa Indonesia](#deskripsi-proyek) | [English](#project-description)

---

## Deskripsi Proyek

**Network Scanner CLI** adalah alat pemindai jaringan sederhana namun kuat yang dibangun menggunakan C# dan .NET. Aplikasi ini berjalan di terminal (Console) dan dirancang untuk menjadi lintas platform (Windows, Linux, macOS). Alat ini membantu administrator jaringan atau pengguna biasa untuk menemukan perangkat yang terhubung dalam jaringan lokal, memeriksa port yang terbuka, dan bahkan menyalakan komputer dari jarak jauh menggunakan Wake-on-LAN.

Dibuat dengan ❤️ oleh **Jacky the Code Bender**.

### Fitur Utama

1.  **Pemindaian Subnet (Subnet Scan)**:
    *   Memindai rentang IP (misalnya `192.168.1.1` hingga `192.168.1.254`).
    *   Mendeteksi status Online/Offline.
    *   Estimasi Sistem Operasi (OS) berdasarkan nilai TTL (Time To Live).
    *   Menampilkan hostname dan latency (ping).
    *   Opsi untuk memindai port pada host yang ditemukan.
    *   **Ekspor Hasil**: Menyimpan hasil pemindaian ke dalam file JSON.

2.  **Pemindaian Port Host (Specific Host Port Scan)**:
    *   Memeriksa ketersediaan host tertentu.
    *   Memindai port umum (seperti 21, 22, 80, 443, 3389, dll) untuk melihat layanan yang berjalan.

3.  **Wake-on-LAN (WOL)**:
    *   Mengirimkan "Magic Packet" ke alamat MAC target untuk menyalakan komputer yang mendukung fitur WOL.

4.  **Antarmuka Menarik**:
    *   Menggunakan library `Spectre.Console` untuk tampilan terminal yang modern, berwarna, dan interaktif.

### Cara Menjalankan

1.  Pastikan Anda telah menginstal **.NET SDK** (versi 8.0 atau lebih baru disarankan).
2.  Buka terminal atau command prompt di folder proyek.
3.  Jalankan perintah berikut:

```bash
dotnet run
```

4.  Ikuti menu interaktif yang muncul di layar.

---

## Project Description

**Network Scanner CLI** is a simple yet powerful network scanning tool built using C# and .NET. It runs in the terminal (Console) and is designed to be cross-platform (Windows, Linux, macOS). This tool assists network administrators or casual users in discovering connected devices on a local network, checking open ports, and remotely waking up computers using Wake-on-LAN.

Created with ❤️ by **Jacky the Code Bender**.

### Key Features

1.  **Subnet Scan**:
    *   Scans an IP range (e.g., `192.168.1.1` to `192.168.1.254`).
    *   Detects Online/Offline status.
    *   Operating System (OS) estimation based on TTL (Time To Live) values.
    *   Displays hostname and latency (ping).
    *   Option to scan open ports on discovered hosts.
    *   **Export Results**: Save scan results to a JSON file.

2.  **Specific Host Port Scan**:
    *   Checks the availability of a specific host.
    *   Scans common ports (such as 21, 22, 80, 443, 3389, etc.) to identify running services.

3.  **Wake-on-LAN (WOL)**:
    *   Sends a "Magic Packet" to a target MAC address to wake up WOL-enabled computers.

4.  **Beautiful Interface**:
    *   Utilizes the `Spectre.Console` library for a modern, colorful, and interactive terminal UI.

### How to Run

1.  Ensure you have the **.NET SDK** installed (version 8.0 or newer recommended).
2.  Open a terminal or command prompt in the project folder.
3.  Run the following command:

```bash
dotnet run
```

4.  Follow the interactive menu displayed on the screen.

---

### Dependencies
*   [Spectre.Console](https://spectreconsole.net/) - For the rich console UI.
*   [Newtonsoft.Json](https://www.newtonsoft.com/json) - For JSON serialization.

### Disclaimer
This tool is intended for educational and network administration purposes only. Please use it responsibly and only on networks you own or have permission to scan.

---
*© 2024 Gravicode Studios*
