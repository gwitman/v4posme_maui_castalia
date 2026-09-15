# Project Structure

```
v4posme_maui/                      # Main MAUI application
├── Models/                        # Data entities and DTOs
├── Views/                         # XAML pages (organized by feature)
│   ├── Abonos/                    # Payment/amortization UI
│   ├── Customers/                 # Customer management UI
│   ├── Invoices/                  # Invoice workflow (multi-step)
│   ├── Items/                     # Product catalog UI
│   ├── More/                      # Reports, visits, returns
│   ├── Printers/                  # Receipt/voucher printing
│   └── Upload/                    # Data upload UI
├── ViewModels/                    # MVVM view models (mirrors Views/)
│   └── BaseViewModel.cs           # Base class (INotifyPropertyChanged)
├── Services/
│   ├── Api/                       # REST API clients (RestApi*.cs)
│   ├── Repository/                # Data access interfaces + implementations
│   ├── Helpers/                   # Business logic helpers
│   ├── HelpersPrinters/           # Printer formatting (ESC/POS commands)
│   ├── Converters/                # XAML value converters
│   ├── SystemNames/               # Constants, enums, VariablesGlobales
│   ├── DataBase.cs                # SQLite initialization
│   ├── BluetoothService.cs        # BLE printer communication
│   ├── GPSService.cs              # Location tracking
│   ├── NavigationService.cs       # Shell navigation abstraction
│   └── PermissionsService.cs      # Runtime permission handling
├── Resources/                     # Fonts, images, icons, splash
├── Platforms/                     # Android-specific code
├── MauiProgram.cs                 # App bootstrap and DI registration
└── App.xaml(.cs)                  # Shell and route registration

v4posme_maui.Tests/                # Unit & property-based tests (xUnit + FsCheck)
```

## Naming Conventions

| Prefix/Pattern | Meaning | Example |
|----------------|---------|---------|
| `Tb*` | Database entity model | `TbTransactionMaster` |
| `Api_*` | API response model | `Api_CoreAcount_LoginMobileResponse` |
| `Dto*` | Data transfer object | `DtoCatalogItem` |
| `ViewTempDto*` | Temporary UI state | `ViewTempDtoInvoice` |
| `IRepository*` | Repository interface | `IRepositoryTbUser` |
| `Repository*` | Repository implementation | `RepositoryTbUser` |
| `RestApi*` | REST API client class | `RestApiCoreAcount` |
| `*ViewModel` | MVVM view model | `PosMeItemsViewModel` |
| `*Page` | XAML page | `InvoicePage` |
| `PosMe*` | App-specific method/class | `PosMeInsert`, `PosMeFindFirst` |
| `Type*` | Enum type | `TypeTransaction`, `TypePayment` |
| `_fieldName` | Private field | `_isBusy` |
| Numeric prefix | Sequential workflow step | `01CustomersViewModel`, `02InvoiceViewModel` |

## Architecture Patterns

- **MVVM** with `BaseViewModel` implementing `INotifyPropertyChanged`
- **Repository pattern** with `IRepositoryFacade<T>` generic base interface
- **Unity DI** for service resolution (registered in `MauiProgram.cs`)
- **Global state** via `VariablesGlobales` static class (user session, company, active DTOs)
- **Shell navigation** with route registration in `App.xaml.cs`
- **Offline-first** — SQLite local storage with server sync via REST API
- **Feature-based folders** for Views and ViewModels (Abonos, Invoices, Customers, etc.)
