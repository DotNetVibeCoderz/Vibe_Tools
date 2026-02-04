# DataExporter 🚀

DataExporter is a powerful database-to-file export tool designed to handle various export scenarios including data masking, compression, and multiple formats.

## Features ✨

- **Modern UI**: Built with Avalonia UI (Dark Theme).
- **Hybrid Mode**: Run as a GUI app or a CLI tool for automation.
- **Multi-Database Support**: Architecture ready for SQL Server, MySQL, SQLite, etc. (Default: SQLite).
- **Multiple Formats**: Export to **JSON** and **CSV**.
- **Data Masking**: Automatically hides sensitive fields (e.g., Email, Salary).
- **Compression**: Auto-zip output files to save space.
- **Logging**: Detailed execution logs using Serilog (Visible in UI and Log file).

## How to Run 🏃‍♂️

### GUI Mode (Default)
1. Ensure you have .NET 8 SDK installed.
2. Run:
   ```bash
   dotnet run
   ```
3. The graphical interface will open.
   - Enter connection string (default provided).
   - Write SQL query.
   - Select options (Format, Compression, Masking).
   - Click "Start Export Job".

### CLI Mode (Automation)
To run in headless/automation mode without UI:
```bash
dotnet run -- --cli
```
or
```bash
dotnet run -- -c
```

## Project Structure 📂

- **Core**: Interfaces (`IDataSource`, `IExporter`).
- **Models**: Configuration objects (`ExportJob`).
- **Services**: Business logic (`SqliteDataSource`, `MaskingService`, `CompressionService`, `CsvExporter`).
- **ViewModels**: MVVM logic for Avalonia (`MainWindowViewModel`).
- **Views**: UI Layouts (`MainWindow.axaml`).
- **CliRunner.cs**: Logic for CLI execution.
- **Program.cs**: Application entry point and mode switcher.

---
*Created with ❤️ by Jacky the Code Bender (Gravicode Studios)*
