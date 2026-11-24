# ParkFlow - Guía de Configuración y Ejecución para el Equipo

Esta guía contiene los pasos exactos para que cualquier integrante del equipo clone o descargue el proyecto y lo ejecute en su máquina local sin problemas.

---

## 1. Requisitos Previos en la Máquina

1. **.NET SDK 10.0** instalado (verificar con `dotnet --version` en la terminal).
2. **SQL Server** instalado y en ejecución (cualquiera de las siguientes opciones):
   - SQL Server Developer Edition / Enterprise.
   - SQL Server Express (`localhost\SQLEXPRESS`).
   - SQL Server LocalDB (`(localdb)\mssqllocaldb`).
   - Opcional: **SQL Server Management Studio (SSMS)** o **Azure Data Studio**.

---

## 2. Base de Datos: ¿Correr Script o Dejar que EF Core la Cree?

Tienes **dos opciones**, ambas totalmente funcionales:

### Opción A (Recomendada y más rápida): Inicialización Automática
No necesitas ejecutar ningún script manualmente. La aplicación web tiene configurado en `Program.cs`:
- `context.Database.EnsureCreated()`: Crea la base de datos `ParkFlow` y todas las tablas si no existen.
- `InicializadorDatos.Inicializar()`: Siembra automáticamente los 5 roles, los 5 usuarios con turnos, las 30 celdas, la caja activa y vehículos de prueba.

### Opción B: Ejecución Manual del Script en SSMS
Si prefieres crear y revisar la base de datos antes de arrancar:
1. Abre SSMS y conéctate a tu instancia de SQL Server.
2. Abre el archivo: `ProyectoParkFlow\LibParkFlow\Script.sql`.
3. Ejecuta todo el script (`F5`). Creará la base de datos `ParkFlow`, todas las tablas, relaciones, roles, usuarios, asignaciones de turno y celdas.

---

## 3. Configurar la Cadena de Conexión (Connection String)

Cada compañero debe adaptar el nombre de su servidor SQL Server si no coincide con `localhost\DEV1`.

### Lugares donde se ajusta:

1. **Frontend Web:** `ProyectoParkFlow\ParkFlow_ASPNETCORE\appsettings.json`
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "server=TU_SERVIDOR;database=ParkFlow;Integrated Security=True;TrustServerCertificate=true;"
     }
   }
   ```

2. **Web API:** `ProyectoParkFlow\ParkFlow.Api\appsettings.json`
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "server=TU_SERVIDOR;database=ParkFlow;Integrated Security=True;TrustServerCertificate=true;"
     }
   }
   ```

3. **Biblioteca Central:** `ProyectoParkFlow\LibParkFlow\Nucleo\Configuraciones.cs`
   *(Usado por las pruebas de consola y como respaldo por defecto)*
   ```csharp
   public static string CadenaConexionPorDefecto = "server=TU_SERVIDOR;database=ParkFlow;Integrated Security=True;TrustServerCertificate=true;";
   ```

### Ejemplos comunes de `server=...`:
- Instancia por defecto estándar: `server=localhost;` o `server=.;`
- SQL Server Express: `server=localhost\\SQLEXPRESS;`
- Instancia con nombre propio: `server=localhost\\TU_INSTANCIA;`
- SQL Server LocalDB: `server=(localdb)\\mssqllocaldb;`
- Si usan usuario y contraseña en vez de Windows Authentication:
  `server=localhost;database=ParkFlow;User Id=sa;Password=tu_clave;TrustServerCertificate=true;`

---

## 4. Ejecución del Proyecto

### Opción 1: Desde Visual Studio
1. Abre la solución `ProyectoParkFlow.slnx`.
2. Haz clic derecho sobre el proyecto **`ParkFlow_ASPNETCORE`** y selecciona **"Establecer como proyecto de inicio"** (*Set as Startup Project*).
3. Presiona **`F5`** (o `Ctrl + F5`).
4. El navegador se abrirá en `http://localhost:5188` (o `https://localhost:7213`).

### Opción 2: Desde Terminal / Consola / VS Code
Abre una terminal en la carpeta raíz del proyecto y ejecuta:

```powershell
dotnet run --project ParkFlow_ASPNETCORE
```

Abre tu navegador en la URL indicada en consola (usualmente [http://localhost:5188](http://localhost:5188)).

---

## 5. Ejecutar las Pruebas Unitarias

Para comprobar que todas las reglas y patrones GoF funcionan en tu entorno:

```powershell
dotnet test PruebasUnitarias
```
Debe indicar **15 pruebas correctas (100% éxito)**.

---

## 6. Credenciales de Acceso por Rol

En la pantalla inicial puedes hacer clic directamente en los botones de rol para que se autocompleten las credenciales:

| Rol | Código Operativo | Contraseña | Pantalla de Destino |
| :--- | :---: | :---: | :--- |
| **Administrador** | `AD-1001` | `parkflow2026` | Dashboard completo y todos los módulos |
| **Operador** | `OP-2094` | `parkflow2026` | Dashboard, Registro Ingreso, Celdas y Lavado |
| **Cajero** | `CJ-3310` | `parkflow2026` | Liquidación & Caja, Cobro y Salidas |
| **Lavador** | `LV-4125` | `parkflow2026` | Tablero Kanban de Control de Lavado |
| **Cliente** | `CL-5001` | `parkflow2026` | Portal de Reservas y Estado de Vehículo |

---

## 7. Resumen de Mejoras y Correcciones Recientes

1. **Autenticación Estricta (Login):**
   - Ahora valida rigurosamente código y contraseña contra la base de datos SQL Server mediante el servicio `IUsuariosNegocio`. Si se ingresa una contraseña incorrecta, se bloquea el acceso con mensaje visible en rojo.
2. **Eliminación Física de Reservas:**
   - El botón "Eliminar" en la pantalla de reservas activas ejecuta el borrado físico (`DELETE FROM Reservas`) en la base de datos y libera la celda asociada dejándola en estado `DISPONIBLE`.
3. **Portal Completo de Cliente (HU-05):**
   - Incluye las 4 vistas del cliente: `Mi Panel` (monitoreo de vehículos, celdas y estado de lavado), `Mi Lavado` (barra de progreso en vivo de 3 fases), `Mis Reservas` (autogestión y cancelación) y `Mi Cuenta` (pago en línea con Tarjeta/Transferencia y consulta de comprobantes emitidos).
   - Incluye el formulario para que el cliente registre o vincule nuevos vehículos a su perfil.
4. **Limpieza Visual y Cero JS:**
   - La aplicación opera **100% en C#, ASP.NET Core Razor Pages y Entity Framework Core**, con **0% JavaScript** para la lógica de negocio. Se eliminaron textos técnicos redundantes en los encabezados.
5. **Control de Roles y Lavado (HU-03 / RN-05):**
   - El rol **Lavador** solo ve y avanza las tarjetas asignadas en el tablero Kanban; no puede elegir autos arbitrarios para lavar.
   - Los lavados se solicitan desde el ingreso por el Operador (`[x] Solicitar servicio de lavado`) o por el Cliente.
6. **Manejo Dinámico de Efectivo en Caja:**
   - El cajero puede ingresar el monto en efectivo recibido y el sistema calcula el cambio exacto. Si se deja en blanco, asume pago exacto y nunca se bloquea por saldo insuficiente.

---

## 8. Preguntas Frecuentes y Solución de Problemas

* **¿Por qué en "Liquidación & Caja" dice "No hay cuentas abiertas"?**  
  Significa que tu archivo `appsettings.json` no se está comunicando con tu instancia local de SQL Server (por ejemplo, si tienes `SQLEXPRESS` y el archivo dice `localhost\DEV1`). Ajusta la cadena de conexión en el paso 3 y reinicia la app.
* **¿Por qué un carro recién ingresado tiene tarifa de parqueadero en $0?**  
  Según la **Regla de Negocio RN-01**, los primeros 5 minutos son un período de gracia de cortesía ($0). A partir del minuto 5 se liquida la primera fracción ($750) y a los 60 minutos la hora completa ($3.000).
* **¿Por qué la celda sigue ocupada tras pagar?**  
  Según la **Regla de Negocio RN-04**, tras realizar el pago el vehículo puede continuar unos minutos en el establecimiento. La celda solo se libera cuando el cajero presiona **"Autorizar Salida"**.
