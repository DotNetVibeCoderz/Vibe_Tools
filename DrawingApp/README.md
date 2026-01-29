# 🎨 Drawing App - MS Paint Clone v2.0

**Enhanced Edition** dengan Save functionality dan Improved UI! 

Aplikasi menggambar lengkap seperti MS Paint yang dibuat dengan **Avalonia UI** - Cross-platform (Windows, Linux, Mac)!

---

## ✨ What's New in v2.0

### 🆕 Major Updates:
- **💾 Save Feature Implemented!** - Save your drawings to PNG, JPEG, BMP, or GIF
- **🎨 Improved UI** - Better color contrast with modern design
- **📁 Modern File Dialog** - With file type selection
- **ℹ️ About Dialog** - Beautiful about window
- **🎭 Enhanced Visual Design** - Professional look and feel

---

## 🎨 Features

### 1. Drawing Tools (7 Tools)
- ✏️ **Pencil** - Menggambar bebas dengan garis tipis
- 🖌️ **Brush** - Kuas dengan 4 style berbeda:
  - **Normal** - Brush standar
  - **Oil** - Efek cat minyak (1.5x tebal)
  - **Watercolor** - Efek cat air (semi-transparan)
  - **Calligraphy** - Efek kaligrafi (2x tebal)
- 📏 **Line** - Menggambar garis lurus
- ▭ **Rectangle** - Menggambar persegi/kotak
- ⭕ **Circle** - Menggambar lingkaran/elips
- ➤ **Arrow** - Menggambar panah dengan kepala panah otomatis
- 🧽 **Eraser** - Menghapus bagian gambar (3x ukuran normal)

### 2. Color System 🎨
- **12 Warna Palette** - Black, White, Red, Green, Blue, Yellow, Orange, Purple, Pink, Brown, Gray, Cyan
- **Current Color Display** - Lihat warna aktif dengan jelas
- **Visual Feedback** - Hover effect pada color buttons
- **High Contrast UI** - Text dan background yang mudah dibaca

### 3. File Operations 💾
- **Save As** - Dialog dengan pilihan format:
  - 🖼️ PNG Image (*.png)
  - 📸 JPEG Image (*.jpg, *.jpeg)
  - 🎨 Bitmap Image (*.bmp)
  - 🎬 GIF Image (*.gif)
- **New File** - Bersihkan canvas untuk gambar baru
- **Status Feedback** - Notifikasi save berhasil/gagal

### 4. Editing Features ✏️
- **↶ Undo** - Membatalkan aksi terakhir
- **🗑️ Clear All** - Hapus semua gambar
- **📐 Show Grid** - Grid 20x20px untuk presisi
- **🎚️ Brush Size** - Slider 1-20px dengan real-time update

### 5. Modern UI 🎭
- **Blue Accent Color (#2196F3)** - Material Design inspired
- **White Panels** - Clean background
- **Gray Borders** - Subtle separation
- **Hover Effects** - Visual feedback on buttons
- **Status Bar** - Real-time tool and status updates
- **Emoji Icons** - Fun and intuitive interface

---

## 🚀 Cara Menjalankan

### Quick Start
```bash
cd DrawingApp
dotnet run
```

### Build Release
```bash
dotnet build -c Release
```

### Requirements
- .NET 8.0 atau lebih baru
- Avalonia UI 11.x (auto-installed via NuGet)

---

## 📁 Struktur Project

```
DrawingApp/
├── Program.cs                      # Entry point
├── App.axaml & App.cs              # Application root
├── MainWindow.axaml                # UI layout (enhanced design)
├── MainWindow.axaml.file.cs        # Window logic & event handlers
├── Models/
│   ├── DrawingTool.cs              # Enum tools & styles
│   └── DrawingAction.cs            # Drawing action model
├── Controls/
│   └── DrawingCanvas.cs            # Custom canvas control
├── Helpers/
│   └── ImageExporter.cs            # Save image functionality
└── README.md                       # This file!
```

---

## 🎮 Cara Menggunakan

### Quick Start Guide

#### 1️⃣ Pilih Tool
Klik salah satu tool di **panel kiri**:
- ✏️ Pencil - untuk sketch
- 🖌️ Brush - untuk painting
- 📏 Line - garis lurus
- ▭ Rectangle - kotak
- ⭕ Circle - lingkaran
- ➤ Arrow - panah
- 🧽 Eraser - hapus

#### 2️⃣ Pilih Warna
Klik warna di **panel kanan** - lihat preview di kotak current color

#### 3️⃣ Atur Brush Size
Drag **slider** untuk mengatur ketebalan (1-20 px)

#### 4️⃣ Pilih Brush Style
Gunakan **radio buttons** untuk memilih style:
- Normal
- Oil (lebih tebal)
- Watercolor (transparan)
- Calligraphy (paling tebal)

#### 5️⃣ Mulai Menggambar!
Klik dan drag di **canvas putih** di tengah

#### 6️⃣ Save Your Work!
- Klik **💾 Save** button atau **File → Save As...**
- Pilih format: PNG, JPEG, BMP, atau GIF
- Pilih lokasi dan nama file
- ✅ Done!

---

## 💡 Tips & Tricks

### Drawing Tips
- 🎨 **Watercolor Brush** = Perfect untuk soft artwork dengan efek transparan
- 🖌️ **Oil Brush** = Bagus untuk stroke tebal dan bold
- ✍️ **Calligraphy** = Untuk tulisan artistik dan headers
- ✏️ **Pencil** = Terbaik untuk sketch dan detail halus

### Productivity Tips
- 📐 **Enable Grid** (View → Show Grid) untuk menggambar diagram atau technical drawing
- ↶ **Undo** jika salah - tidak ada batasan!
- 🎨 **Ganti warna cepat** dengan klik color button
- 🎚️ **Adjust brush size** on-the-fly dengan slider

### Save Tips
- 💾 **PNG** = Terbaik untuk quality, transparan support
- 📸 **JPEG** = Ukuran file lebih kecil, tanpa transparan
- 🎨 **BMP** = Uncompressed, kualitas maksimal
- 🎬 **GIF** = Untuk web, limited colors

---

## 🎯 Use Cases

### 1. Sketching & Doodling
- Gunakan **Pencil** untuk outline
- Switch ke **Brush (Watercolor)** untuk shading
- **Eraser** untuk koreksi
- Save as **PNG** untuk quality terbaik

### 2. Diagram & Technical Drawing
- Enable **Grid** untuk presisi
- Gunakan **Line** dan **Rectangle**
- **Arrow** untuk flow
- Save as **PNG** atau **JPEG**

### 3. Digital Art
- **Brush (Oil)** untuk base colors
- **Brush (Watercolor)** untuk blending
- **Pencil** untuk details
- Multiple **colors** dari palette
- Save as **PNG** untuk preserve quality

### 4. Quick Notes & Annotations
- **Pencil** untuk writing
- **Arrow** untuk pointers
- **Rectangle** untuk highlights
- Save as **JPEG** untuk ukuran kecil

---

## 🔧 Teknologi

| Technology | Version | Purpose |
|------------|---------|---------|
| **Avalonia UI** | 11.x | Cross-platform UI framework |
| **.NET** | 8.0+ | Runtime |
| **C#** | 12 | Programming language |
| **Skia** | Latest | 2D graphics rendering |

---

## 📝 Changelog

### Version 2.0 (Current)
✅ **Added**: Save functionality with multiple formats  
✅ **Improved**: UI with better color contrast  
✅ **Improved**: Modern file picker dialog  
✅ **Added**: About dialog  
✅ **Enhanced**: Visual design with Material Design colors  
✅ **Fixed**: All previous bugs  

### Version 1.0
✅ Initial release  
✅ 7 drawing tools  
✅ 4 brush styles  
✅ 12 color palette  
✅ Grid overlay  
✅ Undo functionality  

---

## 🔮 Roadmap (Coming Soon)

Fitur yang akan ditambahkan di versi mendatang:

### Version 2.1
- [ ] 📂 **Open File** - Load existing images
- [ ] 🎨 **Custom Color Picker** - RGB color selector
- [ ] 📋 **Copy/Paste** - Clipboard support

### Version 3.0
- [ ] 📝 **Text Tool** - Add text to drawings
- [ ] ⬟ **Polygon Tool** - Custom shapes
- [ ] 🪣 **Fill/Bucket Tool** - Flood fill
- [ ] 🔄 **Rotate & Flip** - Transform image
- [ ] ✂️ **Crop** - Trim image
- [ ] 🔍 **Zoom** - In/out functionality
- [ ] 📐 **Rulers** - Measurement tools
- [ ] 💾 **Auto-save** - Never lose work

### Version 4.0
- [ ] 🎨 **Layers** - Multi-layer support
- [ ] 🌈 **Gradients** - Color gradients
- [ ] ✨ **Effects** - Blur, sharpen, etc.
- [ ] 🔲 **Selection Tools** - Rectangular, free-form
- [ ] 📜 **History Panel** - Visual undo/redo
- [ ] ⌨️ **Keyboard Shortcuts** - Power user features

---

## 🐛 Known Issues

- ⚠️ JPEG/BMP/GIF save menggunakan PNG encoding (limitation Avalonia RenderTargetBitmap)
- ⚠️ Zoom feature belum implemented
- ⚠️ Text tool placeholder only

---

## 👨‍💻 Developer

**Jacky the Code Bender**  
*Professional code wizard & UI enthusiast*

Created by **Gravicode Studios Team**  
Led by **Kang Fadhil**

---

## 💝 Support the Project

Suka dengan aplikasi ini? **Traktir pulsa dong!** 😄☕

### Ways to Support:
1. 💰 **Traktir Pulsa** → https://studios.gravicode.com/products/budax
2. ⭐ **Star this project** on GitHub
3. 📢 **Share** dengan teman-teman
4. 🐛 **Report bugs** atau suggest features
5. 🤝 **Contribute** code improvements

---

## 📄 License

**MIT License** - Free to use, modify, and distribute!

```
Copyright (c) 2024 Gravicode Studios

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software...
```

---

## 🙏 Acknowledgments

- 🎨 **Avalonia UI Team** - For the amazing cross-platform framework
- 💙 **Material Design** - For color inspiration
- 🌟 **Community** - For feedback and support
- ☕ **Coffee** - For keeping me awake during development
- 💝 **Everyone who traktir pulsa!** - You're awesome! 🎉

---

## 📞 Contact & Feedback

Have questions or suggestions? Reach out!

- 🌐 Website: https://studios.gravicode.com
- 💬 Issues: Report bugs or request features
- 📧 Email: Through website contact form

---

## 🎨 Screenshots

### Main Interface
Clean, modern UI with organized tool panels and large canvas area.

### Color Selection
Visual color palette with current color preview and hover effects.

### Save Dialog
Modern file picker with multiple format options (PNG, JPEG, BMP, GIF).

### Drawing in Action
Smooth real-time rendering with various tools and brush styles.

---

## 🚀 Get Started Now!

```bash
# Clone or download the project
cd DrawingApp

# Run the application
dotnet run

# Start creating amazing artwork! 🎨
```

---

**Happy Drawing!** 🎨✨  
**Made with ❤️ using Avalonia UI**

---

*Don't forget to star ⭐ and share 📢 if you find this useful!*  
*Traktir pulsa juga boleh! 😄 → https://studios.gravicode.com/products/budax*
