# Sincronización de Datos (Carga / Descarga) y Lugares Sensibles

> Documentación de referencia sobre cómo y dónde la app sincroniza datos con el
> servidor. Mantener actualizada ante cualquier cambio en los flujos de
> login, descarga, subida o cambio de compañía.
>
> **IMPORTANTE (regla de mantenimiento):** cada vez que se modifique
> `RestApiAppMobileApi`, `RestApiCoreAcount`, los ViewModels de
> login / download / upload / about (switch de compañía) o el paso final de
> facturación (`08PrinterInvoiceViewModel`), hay que actualizar este archivo.

## Clases núcleo

Toda la sincronización pasa por una sola clase:

**`Services/Api/RestApiAppMobileApi.cs`** — dos métodos:

| Método | Dirección | Endpoint | Descripción |
|--------|-----------|----------|-------------|
| `GetDataDownload(bool onlyQuantityNew)` | DESCARGA | `app_mobile_api/getDataDownload` | Trae datos del servidor |
| `SendDataAsync()` | SUBIDA | `app_mobile_api/setDataUpload` | Envía datos locales al servidor |

El login contra el servidor es aparte:

**`Services/Api/RestApiCoreAcount.cs`** → `LoginMobile(nickname, password)` →
endpoint `core_acount/loginMobile`. Solo autentica; **no** sincroniza datos.

Endpoints definidos en `Services/SystemNames/Constantes.cs`
(`UrlRequestLogin`, `UrlRequestDownload`, `UrlUpload`). La URL base se compone
con `VariablesGlobales.CompanyKey`.

## Comportamiento de `GetDataDownload(onlyQuantityNew)`

- `onlyQuantityNew == true` (descarga "suave"): **no borra nada**. Solo inserta
  items nuevos y actualiza cantidades de items existentes. Se usa cuando hay
  transacciones locales pendientes (contador ≠ 0) para no perder trabajo.
- `onlyQuantityNew == false` (descarga "dura"): **borra TODO lo local**
  (clientes, items, créditos, amortizaciones, parámetros, compañía,
  transacciones master/detail, server transactions, menú, catálogos,
  indicadores), reinserta todo lo del servidor, reinicia el contador de
  autoincremento (`ParemeterEntityIDAutoIncrement`) a `-1` y recarga
  `VariablesGlobales.TbCompany`.

## Comportamiento de `SendDataAsync()`

- Recopila clientes/items **modificados** (`PosMeTakeModificados` /
  `PosMeTakeModificado`) y **todas** las transacciones master/detail.
- Serializa en `txtData` y hace POST junto con credenciales.
- **No borra nada por sí mismo.** El borrado local post-éxito y el reseteo del
  contador (`ZeroCounter`) los realiza cada ViewModel que la invoca.

## El "contador de transacciones" (clave de control)

`HelperCore.GetCounter()` / `HelperCore.ZeroCounter()` gobiernan casi todas las
decisiones:

- **Contador ≠ 0** (hay movimientos locales sin sincronizar):
  - La descarga es "suave" (`GetDataDownload(true)`).
  - El cambio de compañía queda **bloqueado**.
- **Contador == 0**:
  - La descarga es "dura" (`GetDataDownload(false)`).
  - Se permite cambiar de compañía.

## Lugares donde se invoca (puntos sensibles)

| # | Lugar | Archivo | Sube | Descarga | Notas |
|---|-------|---------|------|----------|-------|
| 1 | Login **sin** recordar | `ViewModels/PosMeZMasterLoginViewModel.cs` (`OnLoginClicked`, `Remember == false`) | No | Sí (condicional) | Valida credenciales contra SQLite local. Tras autenticar, si contador == 0 y hay red → `GetDataDownload(false)` (descarga inmediata). |
| 2 | Login **con** recordar | `ViewModels/PosMeZMasterLoginViewModel.cs` (`OnLoginClicked`, `Remember == true`) | No | Sí (condicional) | Llama `LoginMobile` (auth). Si el usuario es distinto y contador == 0, borra todo lo local y reinicia contador. Tras autenticar, si contador == 0 y hay red → `GetDataDownload(false)` (descarga inmediata). |
| 3 | Menú → Descargar datos | `ViewModels/PosMeDownloadViewModel.cs` (`OnDownloadClicked`) | No | Sí | contador ≠ 0 → `GetDataDownload(true)`; contador == 0 → `GetDataDownload(false)`. Requiere switch activo + red. |
| 4 | Menú → Subir datos | `ViewModels/Upload/UploadViewModel.cs` (`OnUploadCommand`) | Sí | No | contador == 0 → no sube. Si ok, borra items/clientes/transacciones locales y `ZeroCounter`. Requiere switch activo + red. |
| 5 | Cambio de compañía (AboutPage) | `ViewModels/AboutViewModel.cs` (`CambiarCompania`) | No | Sí | Combo solo habilitado si contador == 0. Cambia `CompanyKey` → `LoginMobile` → `GetDataDownload(false)`. Si falla, revierte `CompanyKey` y usuario. |
| 6 | Fin de factura (subida opcional) | `ViewModels/Invoices/08PrinterInvoiceViewModel.cs` (`OnSubirCommand`) | Sí | Sí (solo si la subida fue exitosa) | Solo si parámetro `MOBILE_UPLOAD_AFTER_INVOICE == true`. `SendDataAsync` → **si subida OK**: borra locales + `ZeroCounter` → luego `GetDataDownload(true/false)` según contador. **Si la subida falla (error o respuesta nula): NO se descarga.** Muestra indicador de espera (`ShimmerView` + `ActivityIndicator` + `LoadingMessage`) ligado a `IsBusy`, y bloquea reentrada mientras corre. |

## Resumen por dirección

- **Suben datos:** Menú Upload (#4), fin de factura condicional (#6).
- **Descargan datos:** Login con/sin recordar si contador == 0 (#1, #2), Menú
  Download (#3), cambio de compañía (#5), fin de factura condicional (#6).

## Descarga automática tras login (#1, #2)

Al finalizar cualquier login (con o sin recordar), antes de iniciar el servicio
GPS y navegar al `MainPage`, se consulta el contador de transacciones:

- **contador == 0 y hay red** → `GetDataDownload(false)` (descarga dura
  inmediata), para que el usuario entre con los datos frescos del servidor.
- **contador != 0** → no se descarga, para no perder los movimientos locales
  pendientes de subir.

## Indicador de espera durante la sincronización (#6)

La ventana 8 de la factura (`08PrinterInvoicePage.xaml`) muestra un
`ShimmerView` con `IsLoading="{Binding IsBusy}"` y, en su `LoadingView`, un
`ActivityIndicator` más un `Label` ligado a `LoadingMessage`. Durante
`OnSubirCommand`:

- Al iniciar: `IsBusy = true` y `LoadingMessage = "Subiendo datos..."`.
- Antes de descargar: `LoadingMessage = "Descargando datos..."`.
- Al terminar: `LoadingMessage = ""` e `IsBusy = false`.
- Guard de reentrada: si `IsBusy` ya está activo, el comando se ignora (evita
  disparar subidas/descargas en paralelo por doble toque).

Mensajes en `Services/SystemNames/Mensajes.cs`: `MensajeSubiendoDatos`,
`MensajeDescargandoDatos`.

### Botón "Subir Datos" y confirmación (#6)

- La acción de subir ya **no** está en la toolbar; es un `Button` dentro de la
  página (`08PrinterInvoicePage.xaml`) con color propio (`#E67E22`, naranja) y
  texto blanco, para diferenciarlo visualmente del resto.
- El botón abre un popup de confirmación personalizado con estilo de marca
  (`DXPopup` de DevExpress, `x:Name="ConfirmarSubidaPopup"`), **no** el
  `DisplayAlert` nativo. El popup tiene bordes redondeados, ícono circular,
  título en color `Primary`, mensaje y dos botones (Cancelar / Subir).
- Flujo: el botón ejecuta `MostrarConfirmacionSubidaCommand` → abre el popup
  (`MostrarConfirmacionSubida = true`). En el code-behind,
  `CancelarSubida_Clicked` cierra el popup sin hacer nada;
  `ConfirmarSubida_Clicked` cierra el popup y ejecuta `SubirCommand`.
- `OnSubirCommand` ya **no** muestra diálogo propio; la confirmación es
  responsabilidad del popup. Mantiene el guard de reentrada por `IsBusy`.

## Parámetros de servidor relevantes

- `MOBILE_UPLOAD_AFTER_INVOICE` — habilita subida/descarga automática al cerrar
  una factura (#6).
- `APP_MOBILE_SWITCH_COMPANY` — JSON con las compañías disponibles para el combo
  de cambio de compañía (#5).
