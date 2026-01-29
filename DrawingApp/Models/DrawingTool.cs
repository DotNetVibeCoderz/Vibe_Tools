namespace DrawingApp.Models;

/// <summary>
/// Enum untuk berbagai tool menggambar
/// </summary>
public enum DrawingTool
{
    Pencil,         // Menggambar bebas dengan garis tipis
    Brush,          // Kuas dengan berbagai style
    Line,           // Menggambar garis lurus
    Rectangle,      // Menggambar persegi/kotak
    Circle,         // Menggambar lingkaran/elips
    Arrow,          // Menggambar panah
    Polygon,        // Menggambar poligon
    Fill,           // Bucket fill - mengisi warna
    Text,           // Menambahkan teks
    Eraser,         // Menghapus gambar
    Select,         // Memilih area
    ColorPicker,    // Mengambil warna dari gambar
    None            // Tidak ada tool yang dipilih
}

/// <summary>
/// Style untuk brush
/// </summary>
public enum BrushStyle
{
    Normal,         // Brush biasa
    Oil,            // Oil painting style
    Watercolor,     // Watercolor effect
    Calligraphy     // Calligraphy style
}
