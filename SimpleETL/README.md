# SimpleETL

SimpleETL is an advanced data pipeline application with a visual drag-and-drop designer. Built with C# .NET 8 Blazor Hybrid WPF, it provides a powerful platform for modern data engineering.

## Features
- **Visual Drag-and-Drop Designer**: Interactive canvas to build data pipelines (Sources, Transformations, Destinations).
- **Extensive Connectivity**: Supports SQL Server, CSV, REST API, JSON, etc.
- **Advanced Transformations**: Filter, Join, C# Scripting, and Mockups for ML/LLM actions (e.g. GPT-4 data transformation).
- **Job Scheduling & Executions**: Internal job scheduler mockups and robust engine logic.
- **Embedded REST API**: Trigger and monitor your pipelines via HTTP (`http://localhost:5000/api/flows`).
- **Monitoring & Logs**: Built-in dashboard to monitor successes, failures, and execution times.
- **Modern UI**: Light/Dark theme support using MudBlazor.

## How to Run
1. Ensure you have .NET 8 SDK installed.
2. Clone or download the repository.
3. Open a terminal in the project directory.
4. Run `dotnet run` or open the solution in Visual Studio and click Start.
5. The SQLite database `simpleetl.db` will be auto-generated in the application root on first run.

## API Endpoints
- `GET /api/flows` - Get all saved Data Flows.
- `POST /api/flows/{id}/trigger` - Run the specified flow asynchronously.
- `GET /api/logs` - Retrieve job logs.

---

# Bahasa Indonesia

SimpleETL adalah aplikasi pipeline data canggih dengan desainer visual drag-and-drop. Dibuat menggunakan C# .NET 8 Blazor Hybrid WPF, aplikasi ini menyediakan platform yang kuat untuk rekayasa data modern.

## Fitur Utama
- **Desainer Drag-and-Drop Visual**: Canvas interaktif untuk membangun pipeline data (Sumber, Transformasi, Destinasi).
- **Konektivitas Ekstensif**: Mendukung SQL Server, CSV, REST API, JSON, dll.
- **Transformasi Lanjutan**: Filter, Join, C# Scripting, dan Mockup untuk ML/LLM (misalnya transformasi data dengan GPT-4).
- **Penjadwalan & Eksekusi Job**: Mendukung jadwal cron (mockup internal scheduler) dan logika engine.
- **Embedded REST API**: Picu dan pantau pipeline Anda melalui HTTP (`http://localhost:5000/api/flows`).
- **Monitoring & Log**: Dashboard bawaan untuk memantau status sukses, gagal, dan waktu eksekusi.
- **UI Modern**: Dukungan tema Terang/Gelap menggunakan MudBlazor.

## Cara Menjalankan
1. Pastikan Anda telah menginstal SDK .NET 8.
2. Clone atau unduh repositori.
3. Buka terminal pada direktori proyek.
4. Jalankan `dotnet run` atau buka solusi di Visual Studio dan klik Start.
5. Database SQLite `simpleetl.db` akan dibuat secara otomatis di root aplikasi pada saat dijalankan pertama kali.

## Endpoint API
- `GET /api/flows` - Mendapatkan daftar semua Data Flow.
- `POST /api/flows/{id}/trigger` - Menjalankan flow yang ditentukan secara asinkron.
- `GET /api/logs` - Mengambil log pekerjaan (job logs).

*Dibuat oleh Jacky The Code Bender dari Gravicode Studios.*
