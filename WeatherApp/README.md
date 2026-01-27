# WeatherApp 🌤️

[English Version](#english) | [Versi Bahasa Indonesia](#indonesia)

---

<a name="english"></a>
## 🇺🇸 English

**WeatherApp** is a simple cross-platform desktop weather app built with **C#** and **Avalonia UI**. It fetches real-time weather data from **Open‑Meteo (no API key required)** and shows temperature, condition, wind speed, and coordinates for a selected city.

### ✨ Features
- Real-time weather for preset cities (Jakarta, Surabaya, Bandung, Tokyo, London, New York, Moscow)
- Displays temperature, condition (WMO code mapping), wind speed, and coordinates
- Dynamic background and icons based on weather and day/night
- Clean glassmorphism-style UI

### 🧰 Tech Stack
- **C# (.NET 10)**
- **Avalonia UI 11.x**
- **Newtonsoft.Json**
- **Open‑Meteo API** (free, no key)

### ▶️ How to Run
1. Open `WeatherApp.sln` with Visual Studio / Rider.
2. Restore NuGet packages.
3. Run the project (`F5` or `dotnet run`).

### 📁 Project Structure (important files)
- `MainWindow.axaml` – UI layout
- `MainWindow.axaml.cs` – UI logic & data binding
- `WeatherService.cs` – API call + weather code mapping
- `Models.cs` – data models

---

<a name="indonesia"></a>
## 🇮🇩 Bahasa Indonesia

**WeatherApp** adalah aplikasi cuaca desktop lintas platform yang dibuat dengan **C#** dan **Avalonia UI**. Aplikasi ini mengambil data cuaca real-time dari **Open‑Meteo (tanpa API key)** dan menampilkan suhu, kondisi, kecepatan angin, serta koordinat kota yang dipilih.

### ✨ Fitur
- Cuaca real-time untuk kota-kota preset (Jakarta, Surabaya, Bandung, Tokyo, London, New York, Moscow)
- Menampilkan suhu, kondisi (mapping WMO code), kecepatan angin, dan koordinat
- Background dan ikon berubah sesuai cuaca serta siang/malam
- UI clean dengan efek glassmorphism

### 🧰 Teknologi
- **C# (.NET 10)**
- **Avalonia UI 11.x**
- **Newtonsoft.Json**
- **Open‑Meteo API** (gratis, tanpa key)

### ▶️ Cara Menjalankan
1. Buka `WeatherApp.sln` dengan Visual Studio / Rider.
2. Restore paket NuGet.
3. Jalankan project (`F5` atau `dotnet run`).

### 📁 Struktur Project (file penting)
- `MainWindow.axaml` – layout UI
- `MainWindow.axaml.cs` – logika UI & data
- `WeatherService.cs` – panggilan API + mapping kode cuaca
- `Models.cs` – model data

---

### 👨‍💻 Author & Credits
Created with ❤️ by **Jacky the Code Bender**

Powered by **Gravicode Studios** (Lead: Kang Fadhil)

---

### ☕ Support Me (Traktiran Pulsanya 😄)
Kalau aplikasi ini bermanfaat, boleh dong traktiran pulsanya biar makin semangat ngoding 🚀

👉 https://studios.gravicode.com/products/budax
