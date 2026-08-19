# AGENTS.md

## Qué es
App de escritorio Avalonia para administración de licencias ("ControlLicencias").
C#, .NET 10, un solo `ControlLicencias.csproj`. Sin archivo `.sln`, sin tests, sin repositorio git.

## Build / ejecutar
- `dotnet build` o `dotnet run` desde este directorio (no hay `.sln` ni proyecto de tests).
- Requiere el SDK de .NET 10. No hay linter/formatter/typecheck más allá de `dotnet build`.
- No hay tests; la única verificación es que compile.

## Arquitectura (NO es MVVM)
- No hay view models ni bindings compilados. Cada `Views/*.axaml` se empareja con un code-behind `.axaml.cs`;
  los controles se resuelven con `this.FindControl<T>("Name")` y los datos se cargan en handlers `Loaded`/click.
- XAML se carga en runtime (`AvaloniaXamlLoader.Load(this)`), NO con el compilador XAML.
- Entrada: `Program.cs` -> `App.axaml.cs` (`LoginWindow`) -> `MainWindow`.
  `MainWindow` alterna `ClientesView` / `LicenciasView` / `ConfigView` / `UsuariosAdminView`
  dentro de un `ContentControl` mediante botones del sidebar.
- `Models/`: POCOs que reflejan el JSON del API + propiedades de UI de solo lectura
  (`EstadoTexto`, `EstadoColor`, `EstadoBg`, `EstadoLicencia*`).
- `Services/ApiService.cs`: wrapper estático de `HttpClient` (`Get/Post/Put/Delete`).
  Los cuerpos de request se escriben en camelCase manualmente; las respuestas se deserializan case-insensitively.

## UI / componentes propios
- Las listas usan `DataGrid` (paquete `Avalonia.Controls.DataGrid`), NO `ListBox` manual.
  Columnas ordenables, `DataGridTemplateColumn` para badges de estado y botones de acción.
  En los handlers de acción lee el ítem con `btn.DataContext is T`.
- Notificaciones: usa `Helpers/ToastService.Show(msg, ToastType.*)` en las vistas principales
  en vez de escribir en `LblEstado`. El host se registra en `MainWindow` (`ToastService.Register`).
- Tema claro/oscuro: `Helpers/ThemeManager.Toggle()` + persistencia en `Helpers/Settings`
  (guardado en `%AppData%\ControlLicencias\settings.json`). Cargado al arrancar en `App.axaml.cs`.
- Estilos/tema viven en `App.axaml` (diccionarios Light/Dark + clases: `PrimaryBtn`, `NavBtn`,
  `GhostBtn`, `DangerBtn`, `IconBtn`, `CloseBtn`, `Border.Card`). Reutilízalos en vez de estilos inline.
- Estados de carga: panel `PanelLoading` (ProgressBar indeterminado) que se alterna en `CargarAsync`.
- Ventanas frameless: agregan `OnPointerPressed` -> `BeginMoveDrag` (ver `LoginWindow`).

## Dependencia del backend
- Consume un API ASP.NET Core separado en el directorio hermano `C:\C#\Api_licencias`;
  la URL base sale de `conexion_api.txt` (por defecto `http://localhost:5000`), requiere SQL Server DB `Licencias`.
- `conexion_api.txt` está `CopyToOutputDirectory=Always`; `Config.ApiUrl` lanza excepción en runtime si falta.
- Los errores del API devuelven `{ "error": "..." }`; `ApiService` los relanza como `ApiException.Message`.

## Convenciones
- Texto de UI, identificadores y comentarios en español.
- El estado de sesión es estático en `Helpers/Sesion.cs`; el token se aplica con `ApiService.SetToken`.
