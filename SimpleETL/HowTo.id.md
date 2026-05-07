# Panduan Penggunaan SimpleETL

Selamat datang di SimpleETL! Panduan ini akan menjelaskan cara menggunakan desainer visual *drag-and-drop*, cara mengelola *flow variables* (variabel dinamis), serta memberikan contoh pengisian nilai (konfigurasi) untuk komponen-komponen yang paling sering digunakan.

---

## 1. Memulai di Designer
1. **Canvas (Area Kerja)**: Bagian tengah adalah tempat Anda merangkai alur data (pipeline). Tarik (*drag*) komponen dari panel sebelah kiri (atau klik saja) untuk menambahkannya ke Canvas.
2. **Urutan Penting**: Komponen akan dieksekusi secara berurutan dari atas ke bawah.
3. **Panel Properties (Properti)**: Klik komponen apa saja yang ada di Canvas untuk menampilkan dan mengubah pengaturannya di panel sebelah kanan.

---

## 2. Menggunakan Flow Variables
Variabel memungkinkan Anda membuat *pipeline* yang dinamis. Anda bisa mendefinisikannya di panel **"Flow Variables"** (sebelah kanan, klik untuk membuka).

- **Membuat Variabel**: Klik `Add Variable`, tentukan namanya (contoh: `BaseUrl`), pilih tipe datanya (`String`), dan berikan nilai awal (contoh: `https://api.example.com`).
- **Menggunakan Variabel**: Di hampir semua kolom isian (URL, Query, Scripts, File Path), Anda bisa menyisipkan variabel menggunakan tanda kurung kurawal ganda `{{NamaVariabel}}`.
  - Contoh: `{{BaseUrl}}/users` akan otomatis berubah menjadi `https://api.example.com/users` saat *pipeline* dijalankan.

---

## 3. Contoh Konfigurasi Komponen

Berikut adalah contoh nilai yang bisa Anda masukkan ke dalam panel *Properties* untuk masing-masing komponen utama.

### A. Sources (Ekstraksi / Pengambilan Data)

**1. SQL Server Source**
- **Connection String**: `Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;` *(Anda juga bisa menggunakan variabel seperti `{{DbConn}}`)*
- **Query / Table Name**: `SELECT id, name, email FROM Users WHERE status = 'active'`

**2. CSV Source**
- **Connection String / Path**: `C:\data\input_sales.csv` *(atau path relatif `./data/input.csv`)*
- *Catatan: Pastikan file CSV tersebut memiliki baris judul (header).*

**3. REST API Source**
- **API URL**: `https://jsonplaceholder.typicode.com/users`
- **HTTP Method**: `GET`
- **Payload**: *(Biarkan kosong untuk metode GET)*

---

### B. Transformations (Pemrosesan Data)

**1. Filter**
- **Code Editor (Condition)**: `row.age > 18 && row.status == 'active'`
- *Penjelasan*: Sistem akan mengevaluasi kondisi berbasis JavaScript ini untuk setiap baris (`row`). Jika hasilnya `true`, baris tersebut akan diteruskan.

**2. Set Variable Value**
- **Select Variable**: Pilih variabel yang ingin diubah (contoh: `CurrentDate`).
- **New Value**: `2023-10-25` *(Anda bisa memanggil variabel lain di sini, contoh: `Prefix-{{VarLain}}`)*

**3. C# Script**
Memungkinkan modifikasi tingkat lanjut. Data pipeline akan masuk ke dalam skrip dengan nama variabel `Input` (bertpe `IEnumerable<IDictionary<string, object>>`).
```csharp
var list = Input.ToList();
foreach(var row in list)
{
    // Mengubah kolom yang ada atau membuat kolom baru
    row["FullName"] = row["FirstName"].ToString() + " " + row["LastName"].ToString();
    
    // Membaca variabel dinamis dari Designer
    row["ProcessedBy"] = Variables["SystemUser"];
}
return list;
```

**4. Python Script**
Data *pipeline* masuk sebagai `data` (List yang berisi sekumpulan Dictionary).
```python
def transform(data):
    for row in data:
        # Membuat kolom baru
        row["tax"] = float(row["price"]) * 0.1
        
        # Membaca variabel dinamis dari Designer
        row["currency"] = Variables["CurrencyCode"]
    return data
```

**5. PowerShell Script**
Menjalankan perintah bawaan OS Windows.
- **Command / Script**:
  ```powershell
  Write-Host "Memulai pipeline ETL untuk tanggal: {{CurrentDate}}"
  Copy-Item -Path "C:\temp\file.txt" -Destination "C:\archive\"
  ```

**6. LLM Action (OpenAI)**
*(Membutuhkan `OPENAI_API_KEY` di System Environment Variables komputer Anda agar bisa berjalan sungguhan).*
- **Model**: `gpt-3.5-turbo`
- **System Prompt**: `You are a data normalizer assistant.`
- **Prompt Instruction**: `Normalize the names in the provided data to Title Case.`

---

### C. Destinations (Pemuatan / Penyimpanan Data)

**1. SQL Destination**
- **Connection String**: `Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;`
- **Query / Table Name**: `ProcessedUsers`
- *Catatan: Sistem akan otomatis mencocokkan *key* dari data pipeline Anda ke kolom-kolom di dalam tabel SQL tersebut dan mengeksekusi `INSERT INTO ProcessedUsers` secara massal (batch).*

**2. CSV Destination**
- **Connection String / Path**: `C:\data\output\processed_data_{{CurrentDate}}.csv`
- *Catatan: Menggunakan variabel dinamis pada nama file akan sangat berguna untuk men-generate nama file laporan harian.*

**3. REST API Destination**
- **API URL**: `https://api.mycompany.com/v1/data-ingest`
- **HTTP Method**: `POST`
- **Payload**:
  ```json
  {
      "batch_id": "{{BatchId}}",
      "timestamp": "{{CurrentTime}}",
      "records": {{data}}
  }
  ```
  *Penjelasan*: Kata kunci spesial `{{data}}` (tanpa tanda kutip ganda) akan secara otomatis diganti/di-inject dengan *JSON Array String* berisi seluruh data hasil olahan dari *pipeline* Anda!

---

## 4. Eksekusi & Pemantauan (Monitoring)
- **Run**: Klik tombol hijau `Run` di menu atas untuk memulai eksekusi alur data.
- **Stop**: Klik tombol merah `Stop` jika ada proses yang memakan waktu terlalu lama atau ingin dibatalkan.
- **Output Logs**: Pantau kotak terminal hitam di bagian bawah Canvas. Ia menyediakan log informasi secara *real-time*, menghitung jumlah data, sekaligus melacak sumber *error* jika terjadi kesalahan.

*Selamat membangun pipeline data dengan mudah bersama SimpleETL!*
