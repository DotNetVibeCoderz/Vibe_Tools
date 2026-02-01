# LocalSearch

![License](https://img.shields.io/badge/license-MIT-blue.svg) ![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey) ![Built With](https://img.shields.io/badge/built%20with-AvaloniaUI%20%26%20Lucene.NET-purple)

**LocalSearch** is a high-performance local file search engine built with **C#** and **Avalonia UI**. It utilizes **Lucene.Net** for powerful text indexing and searching capabilities, allowing you to find files instantly based on their content, not just their filenames.

---

## 🇬🇧 English

### Features
- **High-Speed Indexing**: Uses Lucene.Net inverted index technology for lightning-fast lookups.
- **Deep Content Search**: Reads and indexes content from various file formats:
  - Text files (`.txt`, `.md`, `.json`, `.xml`, `.log`, etc.)
  - Source codes (`.cs`, `.js`, `.py`, `.go`, `.rs`, etc.)
  - PDF Documents (`.pdf`) via PdfPig.
- **Semantic/Fuzzy Search**: Optional search mode that handles typos and finds similar words (e.g., "serch" finds "search").
- **Modern UI**: Clean, dark-themed interface built with Avalonia UI (works on Windows, Linux, and macOS).
- **Instant Preview**: Shows relevance score and text snippets where the keyword was found.
- **Direct Access**: Double-click any result to open the file in your default application.

### Getting Started

#### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)

#### Installation & Run
1. Clone this repository.
2. Navigate to the project directory.
3. Run the application:
   ```bash
   dotnet run
   ```

#### Usage
1. **Indexing**: 
   - Enter the full path of the directory you want to scan in the top input box (e.g., `C:\MyDocuments` or `/home/user/docs`).
   - Click **Index Now**. Wait for the status to say "Indexing complete".
2. **Searching**:
   - Type your keywords in the search box.
   - Check **"Semantic (Fuzzy)"** if you want to allow approximate matches.
   - Click **Search**.
3. **Open File**:
   - Double-click on any row in the result grid to open the file.

---

## 🇮🇩 Bahasa Indonesia

### Deskripsi
**LocalSearch** adalah mesin pencari file lokal berkinerja tinggi yang dibuat dengan **C#** dan **Avalonia UI**. Aplikasi ini menggunakan **Lucene.Net** untuk kemampuan indexing dan pencarian teks yang sangat kuat, memungkinkan Anda menemukan file secara instan berdasarkan isinya, bukan hanya nama filenya.

### Fitur Utama
- **Indexing Super Cepat**: Menggunakan teknologi *inverted index* Lucene.Net untuk pencarian kilat.
- **Pencarian Isi File**: Membaca dan mengindeks konten dari berbagai format file:
  - File Teks (`.txt`, `.md`, `.json`, `.xml`, `.log`, dll.)
  - Kode Sumber (`.cs`, `.js`, `.py`, `.go`, `.rs`, dll.)
  - Dokumen PDF (`.pdf`) menggunakan PdfPig.
- **Pencarian Semantik/Fuzzy**: Opsi pencarian yang mentoleransi *typo* (salah ketik) dan menemukan kata yang mirip (contoh: "pencarrian" akan tetap menemukan "pencarian").
- **UI Modern**: Antarmuka tema gelap yang bersih dibuat dengan Avalonia UI (berjalan di Windows, Linux, dan macOS).
- **Pratinjau Instan**: Menampilkan skor relevansi dan potongan teks (snippet) di mana kata kunci ditemukan.
- **Akses Langsung**: Klik dua kali pada hasil pencarian untuk membuka file di aplikasi default Anda.

### Cara Memulai

#### Prasyarat
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)

#### Instalasi & Menjalankan
1. Clone repository ini.
2. Masuk ke direktori project.
3. Jalankan aplikasi dengan perintah:
   ```bash
   dotnet run
   ```

#### Cara Penggunaan
1. **Indexing (Pengindeksan)**: 
   - Masukkan *path* (alamat) folder lengkap yang ingin Anda scan di kotak input bagian atas (contoh: `D:\DokumenKerja` atau `/home/user/docs`).
   - Klik tombol **Index Now**. Tunggu hingga status menunjukkan "Indexing complete".
2. **Pencarian**:
   - Ketik kata kunci di kotak pencarian.
   - Centang **"Semantic (Fuzzy)"** jika Anda ingin pencarian yang lebih luwes (bisa mendeteksi typo).
   - Klik **Search**.
3. **Buka File**:
   - Klik dua kali (double-click) pada baris hasil pencarian untuk membuka file tersebut.

---

### Created By
**Jacky The Code Bender**  
*Gravicode Studios*  
[https://studios.gravicode.com](https://studios.gravicode.com)
