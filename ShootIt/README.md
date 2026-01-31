# ShootIt 📸

## English
**ShootIt** is a simple yet powerful screen capture tool developed by **Jacky the Code Bender** from **Gravicode Studios**.
Designed as a hybrid Console/GUI application using **C#** and **Avalonia UI**, it aims to provide cross-platform capabilities for screen capturing needs.

### 🚀 Features (MVP)
*   **Full Screen Capture**: Instantly capture your entire desktop.
*   **Region Capture**: Drag and select specific areas of your screen to capture via an overlay.
*   **Auto-Save**: Images are automatically saved with timestamps in the `Screenshots` folder.
*   **Modern UI**: Built with Avalonia UI Fluent Theme.

### 🛠️ Prerequisites
*   .NET 8.0 SDK or later.
*   **Windows**: Recommended for full compatibility (uses GDI+).
*   **Linux/macOS**: Requires `libgdiplus` installed to support `System.Drawing.Common`, or further modification to use native capture APIs.

### ▶️ How to Run
1.  Open your terminal in the project directory.
2.  Restore dependencies and run:
    ```bash
    dotnet run
    ```
3.  A GUI window will appear. Use the buttons to capture screens.

### ⚠️ Technical Note
This project uses `System.Drawing.Common` for the capture logic. While convenient for Windows, cross-platform users might face warnings or need additional libraries. Future updates could implement `SkiaSharp` or platform-specific APIs for better compatibility.

---

## Bahasa Indonesia
**ShootIt** adalah alat penangkap layar yang simpel namun canggih, dikembangkan oleh **Jacky the Code Bender** dari **Gravicode Studios**.
Dibuat sebagai aplikasi hybrid Console/GUI menggunakan **C#** dan **Avalonia UI**, aplikasi ini bertujuan untuk memenuhi kebutuhan screen capture.

### 🚀 Fitur (MVP)
*   **Full Screen Capture**: Tangkap seluruh layar desktop secara instan.
*   **Region Capture**: Tarik dan pilih area tertentu di layar menggunakan overlay transparan.
*   **Simpan Otomatis**: Gambar otomatis disimpan dengan timestamp di dalam folder `Screenshots`.
*   **Tampilan Modern**: Dibangun menggunakan Tema Fluent Avalonia UI.

### 🛠️ Prasyarat
*   .NET 8.0 SDK atau yang lebih baru.
*   **Windows**: Sangat direkomendasikan untuk kompatibilitas penuh (menggunakan GDI+).
*   **Linux/macOS**: Membutuhkan instalasi `libgdiplus` untuk mendukung `System.Drawing.Common`, atau modifikasi kode untuk menggunakan API native.

### ▶️ Cara Menjalankan
1.  Buka terminal di direktori project.
2.  Pulihkan dependensi dan jalankan:
    ```bash
    dotnet run
    ```
3.  Jendela GUI akan muncul. Gunakan tombol yang tersedia untuk mengambil screenshot.

### ⚠️ Catatan Teknis
Project ini menggunakan `System.Drawing.Common` untuk logika pengambilan gambar. Meskipun praktis untuk Windows, pengguna cross-platform mungkin menemui peringatan (warning) atau membutuhkan library tambahan. Update di masa depan dapat mengimplementasikan `SkiaSharp` atau API spesifik platform untuk kompatibilitas yang lebih baik.

---

### ☕ Support Us / Dukung Kami
If you find this useful, treat Jacky a coffee (or credit)!
Jika aplikasi ini bermanfaat, boleh dong traktir Jacky pulsa!

👉 [Traktir di sini / Donate here](https://studios.gravicode.com/products/budax)

**Made with ❤️ by Gravicode Studios**
