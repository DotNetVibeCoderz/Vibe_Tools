# HtmlToPdf Converter

[Bahasa Indonesia]

## Deskripsi
Aplikasi Desktop HtmlToPdf adalah alat sederhana namun kuat untuk mengubah template HTML menjadi dokumen PDF, Word, atau HTML. Aplikasi ini dirancang untuk memudahkan pembuatan dokumen seperti kartu nama, faktur, CV, surat lamaran, dan resep makanan dengan desain yang indah.

## Fitur
- **Template HTML Dinamis**: Letakkan file HTML di folder `Templates`. Aplikasi akan memuatnya secara otomatis.
- **Input Otomatis**: Gunakan format `[$NamaField]` di dalam HTML Anda. Aplikasi akan mendeteksi dan membuat formulir input secara otomatis.
- **Preview Langsung**: Lihat hasil perubahan data secara real-time.
- **Ekspor Fleksibel**: Simpan sebagai PDF (menggunakan engine Chrome/Puppeteer untuk hasil akurat), Word (.doc), atau HTML.
- **Pilihan Kertas**: Mendukung A4, Letter, A3, dll.
- **Fitur Repeat**: Ulangi template beberapa kali (berguna untuk mencetak kartu nama dalam satu halaman).

## Cara Menggunakan
1. Buka aplikasi. Tunggu hingga engine PDF (Chromium) selesai diunduh (hanya sekali).
2. Pilih template dari daftar drop-down.
3. Isi kolom yang muncul di sebelah kiri.
4. (Opsional) Centang "Repeat" jika ingin mencetak beberapa kali.
5. Klik "EXPORT" dan pilih format penyimpanan.

## Menambah Template Sendiri
Buat file `.html` baru di folder `Templates`. Gunakan CSS untuk desain, dan gunakan tag `[$Nama]` untuk bagian yang ingin diubah datanya.
Contoh: `<h1>Halo, [$Nama]</h1>`

---

[English]

## Description
HtmlToPdf Converter Desktop App is a simple yet powerful tool to convert HTML templates into PDF, Word, or HTML documents. It is designed to facilitate the creation of documents like business cards, invoices, CVs, application letters, and recipes with beautiful designs.

## Features
- **Dynamic HTML Templates**: Put HTML files in the `Templates` folder. The app loads them automatically.
- **Automatic Input Generation**: Use the format `[$FieldName]` inside your HTML. The app will detect these and generate input forms automatically.
- **Live Preview**: See the result of data changes in real-time.
- **Flexible Export**: Save as PDF (using Chrome/Puppeteer engine for accurate rendering), Word (.doc), or HTML.
- **Paper Size Selection**: Supports A4, Letter, A3, etc.
- **Repeat Feature**: Repeat the template multiple times (useful for printing business cards on a single page).

## How to Use
1. Open the app. Wait for the PDF engine (Chromium) to download (one-time setup).
2. Select a template from the dropdown list.
3. Fill in the fields that appear on the left.
4. (Optional) Check "Repeat" if you want to print multiple copies.
5. Click "EXPORT" and select the save format.

## Adding Your Own Templates
Create a new `.html` file in the `Templates` folder. Use CSS for design, and use tags like `[$Name]` for dynamic data.
Example: `<h1>Hello, [$Name]</h1>`
