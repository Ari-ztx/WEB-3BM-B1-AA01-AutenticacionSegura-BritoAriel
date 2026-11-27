# Autenticación segura para un taller

Actividad AA01 de Programación Web I. La aplicación se centra en registro, inicio de sesión, recuperación de contraseña, perfil y gestión de roles. Utiliza ASP.NET Core MVC, Identity, Entity Framework Core y SQL Server. El CRUD de mantenimientos no forma parte de esta actividad.

## 1. Herramientas necesarias

- JetBrains Rider con soporte para proyectos ASP.NET Core.
- SDK de .NET 10. Comprueba su instalación con `dotnet --list-sdks` en la terminal de Rider.
- SQL Server o SQL Server Express LocalDB en Windows. El ejemplo usa LocalDB e inicio de sesión integrado de Windows.
- `dotnet-ef` de la versión principal 10, compatible con los paquetes de Entity Framework Core del proyecto.
- Una cuenta de correo remitente con acceso SMTP y un buzón real donde recibir el mensaje de recuperación. Consulta los datos y políticas del proveedor.

## 2. Abrir el proyecto en Rider

Abre Rider y selecciona **Open**. Elige la carpeta del proyecto o el archivo `Deber_AutenticacionSegura.csproj`. Espera a que Rider cargue el proyecto y restaure los paquetes. Abre la terminal integrada y verifica que estés dentro de la carpeta que contiene el archivo `.csproj`.

En la terminal puedes ejecutar:

```powershell
dotnet restore
dotnet build
```

Si falta la herramienta de migraciones, instálala una vez o actualiza una instalación previa para usar la versión principal 10:

```powershell
dotnet tool install --global dotnet-ef --version 10.*
```

Si ya está instalada, utiliza `dotnet tool update --global dotnet-ef --version 10.*`. Puede ser necesario reiniciar Rider para que encuentre la herramienta. Verifica con `dotnet ef --version`.

## 3. Preparar SQL Server

La cadena de desarrollo en `appsettings.json` usa `(localdb)\MSSQLLocalDB` y la base `AutenticacionSeguraRiderDb`. Si utilizas otra instancia, configura tu cadena de conexión con User Secrets, por ejemplo:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\MSSQLLocalDB;Database=AutenticacionSeguraRiderDb;Trusted_Connection=True;TrustServerCertificate=True"
```

El ejemplo `TrustServerCertificate=True` es para desarrollo local. En producción usa cifrado y un certificado confiable. Si deseas preparar la base manualmente, ejecuta `crear-base-datos.sql` en SQL Server Management Studio con una cuenta autorizada. El script crea solo la base vacía; no crea las tablas. También puedes dejar que EF cree la base si la cuenta de la conexión tiene permisos suficientes.

**Aplica las migraciones incluidas antes de iniciar la aplicación:**

```powershell
dotnet ef database update
```

Esta operación crea las tablas de Identity y registra qué migraciones se aplicaron. La aplicación no aplica las migraciones automáticamente al iniciarse. Si la conexión falla, revisa que la instancia esté activa, que la cadena apunte a ella y que el usuario tenga permisos.

## 4. Configurar administrador inicial y correo

Desde la terminal de la carpeta del proyecto, reemplaza cada texto entre `<` y `>` por un valor propio. Usa una cuenta ficticia para la demostración y una contraseña que cumpla la política. Estos comandos guardan la configuración en User Secrets, fuera del código fuente:

```powershell
dotnet user-secrets set "Admin:FullName" "Administrador de prueba"
dotnet user-secrets set "Admin:Email" "<correo-del-administrador>"
dotnet user-secrets set "Admin:Password" "<contraseña-segura-del-administrador>"
dotnet user-secrets set "Smtp:Host" "smtp.gmail.com"
dotnet user-secrets set "Smtp:Port" "587"
dotnet user-secrets set "Smtp:UserName" "<usuario-smtp>"
dotnet user-secrets set "Smtp:Password" "<clave-smtp-o-contraseña-de-aplicación>"
dotnet user-secrets set "Smtp:FromEmail" "<correo-remitente-autorizado>"
dotnet user-secrets set "Smtp:FromName" "Taller de prueba"
dotnet user-secrets set "App:BaseUrl" "https://localhost:7232"
```

El servidor SMTP utiliza STARTTLS; 587 es un puerto habitual, pero debes confirmar los datos con tu proveedor. Algunos servicios requieren una contraseña de aplicación, habilitar SMTP o usar como remitente la cuenta autorizada. La dirección `App:BaseUrl` debe coincidir con la dirección HTTPS donde se ejecuta el proyecto, porque se usa para el enlace de recuperación.

### Configuración de Gmail

1. En la cuenta de Google que enviarás desde la aplicación, activa la verificación en dos pasos. Si está disponible, crea una **contraseña de aplicación** desde la configuración de seguridad de Google. No uses ni compartas aquí tu contraseña principal.
2. En tu equipo, configura `Smtp:Host` como `smtp.gmail.com`, `Smtp:Port` como `587` y `Smtp:UserName` con el correo completo de esa cuenta. Configura `Smtp:FromEmail` con ese mismo correo y `Smtp:Password` con la contraseña de aplicación. El transporte usa STARTTLS. Guarda estos valores localmente con User Secrets; no los envíes por chat ni los subas al repositorio.
3. Si no aparece la opción de contraseña de aplicación, revisa la política de tu cuenta. Google indica que puede no estar disponible para ciertas cuentas de trabajo o estudio, cuentas con Protección Avanzada o verificación en dos pasos configurada solo con llaves de seguridad. Consulta al administrador o utiliza un proveedor compatible autorizado.
4. Tras configurar y ejecutar el proyecto, registra una cuenta con acceso a un buzón de prueba y solicita la recuperación. Comprueba que el mensaje llegue y que el enlace funcione; revisar la configuración no sustituye esta prueba.

El administrador inicial se crea a partir de esta configuración. El registro público asigna Cliente; no proporciona una opción para elegir Administrador. No hay credenciales ni buzones reales incluidos en el proyecto. User Secrets sirve para desarrollo y no cifra los valores; las credenciales escritas en la terminal también pueden permanecer en el historial. No las envíes por chat ni las subas a GitHub.

## 5. Ejecutar en Rider

Selecciona el perfil **https** del proyecto y ejecútalo con el botón Run. Alternativamente, desde la terminal:

```powershell
dotnet run --launch-profile https
```

Abre `https://localhost:7232`. Si el navegador no confía en el certificado de desarrollo, revisa el aviso y, para configurar el certificado local de .NET, ejecuta `dotnet dev-certs https --trust`. Si cambias el puerto, actualiza también `App:BaseUrl`. Crea una cuenta Cliente con un buzón de prueba accesible e inicia sesión con el administrador configurado para comprobar las funciones administrativas.

## 6. Seguridad: qué observar

- Identity almacena un hash de contraseña con sal; no guarda la contraseña legible. Las opciones de Identity en `Program.cs` establecen una política de contraseña y bloqueo temporal.
- Los roles restringen acciones desde el servidor con `[Authorize]`; ocultar una opción del menú no protege por sí solo una ruta.
- Los formularios POST se protegen con antiforgery. Las cookies se configuran con HttpOnly, Secure y SameSite.
- Sin Recordarme se usa una cookie de sesión; al marcarlo, una persistente según la caducidad configurada. La sesión se comparte entre pestañas del mismo perfil, pero no con una ventana de incógnito independiente. La restauración de sesiones depende también del navegador.
- El enlace de recuperación tiene vencimiento y debe quedar invalidado al cambiar la contraseña. La recepción y uso deben comprobarse con correo real.
- `localhost` apunta al equipo en el que se abre el enlace. Para una prueba local, abre el correo y el enlace en el mismo computador donde se ejecuta la aplicación.

## 7. Pruebas para completar

**Estado inicial de esta tabla: pendiente de comprobación.** Registra fecha, pasos, resultado observado y evidencia; no marques una prueba como aprobada hasta ejecutarla.

| N.º | Prueba | Resultado esperado |
| --- | --- | --- |
| 1 | Registrar Cliente válido | Usuario guardado en SQL Server con rol Cliente. |
| 2 | Correo duplicado o contraseña inválida | Registro rechazado con validación. |
| 3 | Inicio de sesión y cierre | Acceso permitido y posterior solicitud de autenticación tras salir. |
| 4 | Contraseña incorrecta repetida | Mensaje genérico y bloqueo temporal según la política. |
| 5 | Solicitar recuperación | Llega a un buzón real un enlace funcional con vencimiento. |
| 6 | Usar token válido | Se cambia la contraseña y se inicia sesión con la nueva. |
| 7 | Token inválido, vencido o reutilizado | Restablecimiento rechazado. |
| 8 | Cliente abre una URL administrativa | El servidor deniega el acceso. |
| 9 | Administrador modifica rol o estado | El cambio se guarda y aplica; no se puede quitar o deshabilitar al último administrador activo. |
| 10 | Cliente intenta autoasignarse Administrador | Operación rechazada por el servidor. |
| 11 | Cambiar contraseña con la actual incorrecta | La contraseña no cambia. |
| 12 | Actualizar y abrir otra pestaña | Se conserva la sesión en el mismo perfil. |
| 13 | Abrir una ventana de incógnito independiente | No hereda la sesión normal. |
| 14 | Probar Recordarme y cerrar sesión | La cookie persistente respeta su vigencia; tras salir se requiere iniciar sesión. |

Para el correo, verifica la bandeja de entrada y la carpeta de correo no deseado. La evidencia debe mostrar recepción y uso sin exponer el token completo, claves o datos personales. Guardar solo un enlace en consola o simular su envío no cumple la actividad.

## 8. Entrega

Crea un repositorio **nuevo**, exclusivo de esta actividad, con el formato `WEB-[PARALELO]-B1-AA01-AutenticacionSegura-[ApellidoNombre]`. Incluye código, migraciones, script SQL y este README. Revisa los archivos y el historial antes de publicar; no subas contraseñas, claves SMTP ni tokens. Completa `REGISTRO_IA_BORRADOR.md` con tus correcciones y verificaciones reales y expórtalo a PDF. Envía los elementos solicitados mediante el formulario institucional y comprueba que los enlaces sean accesibles.

En un despliegue público se necesita una URL HTTPS accesible, certificado válido, configuración de conexión cifrada, un proveedor de correo autorizado y gestión segura de secretos y claves de protección de datos. No se debe utilizar `localhost` como enlace para otras personas. Las variables de entorno usan `__` para separar niveles; por ejemplo, `Smtp__Password`.

## Fuentes

- [Configuración de ASP.NET Core Identity — Microsoft Learn](https://learn.microsoft.com/aspnet/core/security/authentication/identity-configuration)
- [Confirmación y recuperación de contraseña — Microsoft Learn](https://learn.microsoft.com/aspnet/core/security/authentication/accconfirm)
- [Secretos de aplicación en desarrollo — Microsoft Learn](https://learn.microsoft.com/aspnet/core/security/app-secrets)
- [SQL Server y EF Core — Microsoft Learn](https://learn.microsoft.com/ef/core/providers/sql-server/)
- [Configuración SMTP de Gmail — Ayuda de Google](https://support.google.com/mail/answer/7104828)
- [Contraseñas de aplicaciones — Ayuda de Google](https://support.google.com/mail/answer/185833)


