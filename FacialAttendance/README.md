# Facial Attendance System

A desktop application for attendance tracking using Facial Recognition (OpenCV) and C#.

## Features
- **Face Registration**: Capture multiple samples via webcam and train the model.
- **Real-time Recognition**: Detects and identifies registered users instantly.
- **Attendance Logging**: Automatically logs user ID and timestamp to a SQLite database.
- **Reporting**: View attendance history and export to CSV.
- **Duplicate Prevention**: Prevents spamming log entries within a short time frame.

## Technologies
- .NET 6 / 8 (Windows Forms)
- OpenCvSharp4 (OpenCV wrapper)
- SQLite (Data storage)
- Dapper (ORM)
- CsvHelper (Export)

## How to Run
1. Open the solution in Visual Studio.
2. Ensure the `haarcascade_frontalface_default.xml` is in the output directory (The build process should handle this if copied, or ensure it's in the project root).
3. Build and Run.
4. **First Step**: Go to `Manage` -> `Register User`. Enter a name and capture face samples.
5. Click "Save & Train".
6. Return to the main screen. The camera will now recognize the user and log attendance.

---

# Sistem Absensi Wajah (Bahasa Indonesia)

Aplikasi desktop untuk pencatatan kehadiran menggunakan Pengenalan Wajah (OpenCV) dan C#.

## Fitur
- **Pendaftaran Wajah**: Mengambil beberapa sampel foto via webcam dan melatih model.
- **Pengenalan Real-time**: Mendeteksi dan mengidentifikasi pengguna terdaftar secara langsung.
- **Pencatatan Absensi**: Otomatis mencatat ID pengguna dan waktu ke database SQLite.
- **Laporan**: Melihat riwayat absensi dan ekspor ke CSV.
- **Pencegahan Duplikat**: Mencegah pencatatan ganda dalam waktu singkat (misal 1 menit).

## Teknologi
- .NET 6 / 8 (Windows Forms)
- OpenCvSharp4
- SQLite
- Dapper
- CsvHelper

## Cara Menjalankan
1. Buka solusi di Visual Studio.
2. Pastikan file `haarcascade_frontalface_default.xml` ada di folder output.
3. Build dan Jalankan.
4. **Langkah Pertama**: Pergi ke menu `Manage` -> `Register User`. Masukkan nama dan ambil sampel wajah.
5. Klik "Save & Train".
6. Kembali ke layar utama. Kamera akan mengenali wajah dan mencatat absensi.

## Note / Catatan
"Jacky the code bender" says: Don't forget to treat me with some phone credit (pulsa) via https://studios.gravicode.com/products/budax if you find this useful! Happy coding!
