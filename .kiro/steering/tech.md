# Tech Stack & Build

## Framework & Language

- .NET 8 / C# with nullable reference types and implicit usings
- .NET MAUI (Multi-platform App UI) — currently targeting `net8.0-android`
- Minimum Android API: 21 (Android 5.0)

## Key Libraries

| Library | Purpose |
|---------|---------|
| DevExpress MAUI | UI controls (DataGrid, CollectionView, Editors, Charts, Scheduler) |
| CommunityToolkit.Maui | Alerts, toasts, MVVM helpers |
| Unity (5.11.x) | Dependency injection container |
| sqlite-net-pcl / SQLitePCLRaw | Local SQLite database |
| Newtonsoft.Json | JSON serialization |
| ZXing.Net.MAUI | Barcode/QR code scanning and generation |
| SkiaSharp | Graphics rendering |
| Plugin.BLE | Bluetooth communication (printers) |
| FsCheck + xUnit | Property-based testing |

## Build & Commands

```bash
# Restore dependencies
dotnet restore

# Build Android target
dotnet build v4posme_maui/v4posme_maui.csproj -f net8.0-android

# Run tests
dotnet test v4posme_maui.Tests/v4posme_maui.Tests.csproj

# Publish APK (Release)
dotnet publish v4posme_maui/v4posme_maui.csproj -f net8.0-android -c Release
```

## Configuration Notes

- DevExpress, CommunityToolkit, SkiaSharp, and ZXing all require explicit initialization in `MauiProgram.cs`
- Unity container registrations are also in `MauiProgram.cs`
- `CodePagesEncodingProvider` is registered for printer encoding support
- Theme management: light theme enforced (`ThemeManager.ApplyThemeToSystemBars = false`)
