# Compressor - File Explorer

Aplikasi File Explorer sederhana yang dibangun menggunakan **Avalonia UI** dan **.NET**. Aplikasi ini memiliki fitur navigasi folder, manajemen file, serta kompresi dan ekstraksi arsip.

## Fitur Utama

*   **Navigasi Folder:** Pohon direktori (Tree View) di sebelah kiri untuk navigasi antar drive dan folder.
*   **Daftar File:** Menampilkan isi folder, termasuk nama, tanggal modifikasi, tipe, dan ukuran.
*   **Operasi File Dasar:**
    *   Membuat Folder Baru.
    *   Membuat File Baru (Txt, Doc, Xls).
    *   Menghapus File/Folder.
*   **Kompresi (Compress):** Mengkompres file atau folder ke dalam format ZIP. (UI mendukung pilihan Rar/7Zip/Cab, namun engine saat ini hanya support ZIP).
*   **Ekstraksi (Extract):** Mengekstrak file arsip (ZIP, RAR, 7z, TAR, GZip) ke folder lokal.
*   **UI Modern:** Menggunakan ikon vektor dan tata letak responsif.

## Perbaikan Terkini (Fixes)

*   **Navigasi Diperbaiki:** Masalah di mana mengklik folder di navigasi kiri tidak memperbarui daftar file di sebelah kanan telah diperbaiki. Sekarang aplikasi menggunakan *event handler* yang kuat untuk memastikan sinkronisasi antara Tree View dan File List.
*   **Lazy Loading:** Navigasi folder menggunakan *lazy loading* agar performa tetap ringan saat membuka drive dengan banyak subfolder.
*   **Icons:** Menambahkan ikon visual untuk Folder dan File di daftar.

## Cara Menjalankan

1.  Pastikan .NET SDK telah terinstal.
2.  Buka terminal di folder project.
3.  Jalankan perintah:
    ```bash
    dotnet run
    ```

## Struktur Kode

*   **ViewModels/MainWindowViewModel.cs:** Mengatur logika utama aplikasi (MVVM).
*   **Views/MainWindow.axaml:** Definisi antarmuka pengguna (UI).
*   **Views/MainWindow.axaml.cs:** *Code-behind* untuk menangani event UI spesifik seperti Selection Changed.
*   **Models/DirectoryNode.cs:** Model untuk item pada Tree View.
*   **Models/FileSystemItem.cs:** Model untuk item pada File List.

---

# Compressor - File Explorer (English)

A simple File Explorer application built with **Avalonia UI** and **.NET**. It features folder navigation, file management, and archive compression/extraction.

## Key Features

*   **Folder Navigation:** Tree View on the left for navigating drives and folders.
*   **File List:** Displays folder contents, including name, date modified, type, and size.
*   **Basic File Operations:**
    *   Create New Folder.
    *   Create New File (Txt, Doc, Xls).
    *   Delete File/Folder.
*   **Compress:** Compress files or folders into ZIP format.
*   **Extract:** Extract archive files (ZIP, RAR, 7z, TAR, GZip) to local folder.
*   **Modern UI:** Uses vector icons and responsive layout.

## Recent Fixes

*   **Navigation Fixed:** Fixed an issue where clicking a folder in the left navigation did not update the file list on the right. The app now uses a robust event handler to ensure synchronization between the Tree View and File List.
*   **Lazy Loading:** Folder navigation uses lazy loading for better performance.
*   **Icons:** Added visual icons for Folders and Files in the list.

## How to Run

1.  Ensure .NET SDK is installed.
2.  Open terminal in the project folder.
3.  Run command:
    ```bash
    dotnet run
    ```
