# JunkCleaner

## Bahasa Indonesia
### Ringkasan
JunkCleaner adalah aplikasi desktop WPF sederhana untuk Windows yang membantu memindai dan membersihkan file sampah (temporary files), cache browser, serta mengosongkan Recycle Bin. Antarmuka menggunakan Material Design untuk tampilan yang modern.

### Fitur Utama
- **Scan junk**: Menghitung total ukuran file sementara dan cache browser.
- **Clean all**: Menghapus file temp, cache browser (Chrome/Edge), dan mengosongkan Recycle Bin.
- **Log aktivitas**: Menampilkan proses dan status pembersihan secara real-time.

### Arsitektur Singkat
- **`CleanerEngine.cs`**: Mesin utama untuk scan & clean (temp, cache, recycle bin, dan scan file besar).
- **`MainViewModel.cs`**: MVVM ViewModel untuk binding UI, status, dan log.
- **`MainWindow.xaml`**: UI utama berbasis MaterialDesign.

### Cara Menjalankan
1. Buka solusi `JunkCleaner.sln` di Visual Studio.
2. Pastikan target framework **.NET 8.0 Windows** tersedia.
3. Jalankan project dengan tombol **Start**.

### Catatan
- Aplikasi ini cocok untuk demo/pembelajaran. Pastikan Anda memahami risiko saat menghapus file.
- Untuk menambah area pembersihan lain, edit `CleanerEngine` dan tambahkan metode scan/clean baru.

---

## English
### Overview
JunkCleaner is a simple Windows WPF desktop app that scans and removes junk files (temporary files), browser cache, and empties the Recycle Bin. The UI uses Material Design for a modern look.

### Key Features
- **Junk scan**: Calculates total size of temp files and browser cache.
- **Clean all**: Deletes temp files, browser cache (Chrome/Edge), and empties the Recycle Bin.
- **Activity log**: Displays real-time progress and status messages.

### Short Architecture
- **`CleanerEngine.cs`**: Core engine for scanning & cleaning (temp, cache, recycle bin, large file scan).
- **`MainViewModel.cs`**: MVVM ViewModel for UI binding, status, and logs.
- **`MainWindow.xaml`**: Main UI using MaterialDesign.

### How to Run
1. Open `JunkCleaner.sln` in Visual Studio.
2. Make sure **.NET 8.0 Windows** is installed.
3. Run the project with the **Start** button.

### Notes
- This app is best for demo/learning purposes. Always be careful when deleting files.
- To extend cleaning areas, add new scan/clean methods in `CleanerEngine`.
