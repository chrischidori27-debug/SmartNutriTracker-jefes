#  SmartNutriTracker :3

<div align="center">

<table>
  <tr>
    <td width="220" align="center" valign="middle">
      <img src="https://i.scdn.co/image/ab67616d0000b27332fdd9cfd9c3b89028ff58e6" width="180" alt="Pibble, un socio más">
    </td>
    <td align="left" valign="middle">
      <h1>SmartNutriTracker</h1>
      <h3>Tu aliado en una alimentación más saludable</h3>
      <p>Una plataforma web para la gestión y el seguimiento nutricional dentro del entorno educativo.</p>
      <strong>Gestiona · Aprende · Cuida tu nutrición</strong>
    </td>
  </tr>
</table>

<br>

<a href="https://dotnet.microsoft.com/">
  <img src="https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white">
</a>
<a href="https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor">
  <img src="https://img.shields.io/badge/Blazor-Interactive_Server-7B2CBF?style=for-the-badge&logo=blazor&logoColor=white">
</a>
<a href="https://learn.microsoft.com/dotnet/csharp/">
  <img src="https://img.shields.io/badge/C%23-Language-239120?style=for-the-badge&logo=csharp&logoColor=white">
</a>
<a href="https://www.microsoft.com/sql-server">
  <img src="https://img.shields.io/badge/SQL_Server-Database-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white">
</a>
<a href="https://github.com/chrischidori27-debug/SmartNutriTracker">
  <img src="https://img.shields.io/badge/GitHub-Repository-181717?style=for-the-badge&logo=github&logoColor=white">
</a>

</div>

---

## 🌱 Acerca del proyecto

**SmartNutriTracker** es una plataforma web desarrollada para apoyar la gestión nutricional dentro del entorno educativo.

Su propósito es ofrecer herramientas que permitan organizar información relacionada con usuarios, alimentos y menús diarios, facilitando el seguimiento y la administración de información nutricional.

El sistema está orientado a la comunidad educativa y contempla diferentes perfiles de acceso, como estudiantes, docentes, nutricionistas y administradores.

## 🎯 Objetivos

- Promover hábitos de alimentación saludable.
- Facilitar la administración de información nutricional.
- Organizar alimentos y menús diarios.
- Gestionar usuarios mediante roles y permisos.
- Ofrecer una interfaz intuitiva, moderna y accesible.
- 
## 📖 Historias de usuario que aborda el proyecto

SmartNutriTracker se desarrolla a partir de un conjunto de historias de usuario que representan las necesidades principales de la comunidad educativa. Estas historias permiten organizar el desarrollo del sistema y establecer las funcionalidades que se buscan implementar.

El proyecto contempla dos áreas principales de trabajo: **Autenticación y Seguridad** y **Gestión de Alimentos**.

### Equipo 1: Autenticación y Seguridad

Esta área se enfoca en proteger el acceso a la plataforma, administrar los perfiles de usuario y mantener un control sobre las operaciones del sistema.

| Código | Historia de usuario | ¿Cómo la aborda SmartNutriTracker? |
|---|---|---|
| HU-01 | Registro de usuarios con roles | Mediante un formulario de registro que solicita nombre, correo, contraseña y rol. Los datos se validan y almacenan en SQL Server, protegiendo las contraseñas mediante hash. |
| HU-02 | Inicio de sesión | Mediante la validación del correo electrónico y la contraseña, verificando las credenciales almacenadas y el estado de la cuenta. |
| HU-03 | Autenticación mediante JWT | Se contempla la incorporación de tokens JWT para gestionar la autenticación y proteger el acceso a los recursos. |
| HU-04 | Gestión de roles y permisos | Mediante los roles Administrador, Docente, Estudiante y Nutricionista, que permiten diferenciar las responsabilidades de cada perfil. |
| HU-05 | Registro de auditoría | Mediante la tabla `LogsAuditoria`, diseñada para almacenar eventos y operaciones realizadas dentro del sistema. |

### Equipo 3: Gestión de Alimentos

Esta área busca facilitar la administración de información nutricional y la organización de menús diarios para la comunidad educativa.

| Código | Historia de usuario | ¿Cómo la aborda SmartNutriTracker? |
|---|---|---|
| HU-14 | Registrar alimentos | Mediante un módulo destinado al registro de alimentos y sus respectivos valores nutricionales, como calorías, proteínas, carbohidratos, grasas y fibra. |
| HU-15 | Editar y eliminar alimentos | Mediante operaciones de actualización y eliminación de registros de alimentos almacenados en la base de datos. |
| HU-16 | Crear menús diarios | Mediante la organización de alimentos en menús asociados a una fecha y a un usuario responsable. |
| HU-17 | Consultar resumen nutricional | Mediante el cálculo y la presentación de los valores nutricionales de los alimentos incluidos en cada menú. |

### Estado de implementación

Las historias de usuario se encuentran en distintas etapas de desarrollo. La siguiente clasificación permite diferenciar las funcionalidades disponibles de aquellas que todavía requieren implementación.

| Historia | Estado |
|---|---|
| HU-01 | Implementada: registro y validación de usuarios. |
| HU-02 | Parcial: validación de credenciales disponible en el servicio; falta completar la sesión de autenticación. |
| HU-03 | Pendiente: integración de JWT. |
| HU-04 | Parcial: roles almacenados y validados en el registro; falta completar la autorización por permisos. |
| HU-05 | Pendiente: tabla preparada, falta implementar el registro de eventos. |
| HU-14 | Pendiente: módulo de registro de alimentos. |
| HU-15 | Pendiente: edición y eliminación de alimentos. |
| HU-16 | Pendiente: creación y administración de menús. |
| HU-17 | Pendiente: cálculo y presentación del resumen nutricional. |

Estas historias constituyen la base funcional del proyecto y orientan su evolución hacia una plataforma integral de gestión nutricional educativa.

## ✨ Funcionalidades del sistema

| Funcionalidad | Descripción |
|---|---|
| Registro de usuarios | Permite registrar nuevos usuarios. |
| Gestión de contraseñas | Almacenamiento de contraseñas protegidas mediante hash. |
| Roles de usuario | Diferenciación de perfiles y responsabilidades. |
| Gestión de alimentos | Módulo destinado a administrar alimentos e información nutricional. |
| Menús diarios | Módulo destinado a organizar menús nutricionales. |
| Auditoría | Base de datos preparada para almacenar registros de eventos. |

> El proyecto se encuentra en desarrollo. Algunas funcionalidades están implementadas y otras forman parte de las siguientes etapas de construcción.

## 🧰 Tecnologías utilizadas

| Tecnología | Utilización |
|---|---|
| C# | Lenguaje principal de programación. |
| .NET 10 | Plataforma de desarrollo. |
| Blazor Web App | Construcción de la interfaz web. |
| Interactive Server | Interactividad de los componentes Blazor. |
| Entity Framework Core 10 | Acceso y comunicación con la base de datos. |
| SQL Server LocalDB | Almacenamiento de información. |
| Git y GitHub | Control de versiones y colaboración. |
| Visual Studio | Entorno de desarrollo. |

## 🗄️ Base de datos

El proyecto utiliza **SQL Server LocalDB** como motor de base de datos.

**Nombre de la base de datos:** `SmartNutriTracker`

### Principales tablas

| Tabla | Descripción |
|---|---|
| Roles | Almacena los roles disponibles. |
| Usuarios | Contiene la información de los usuarios. |
| Alimentos | Almacena alimentos y sus datos nutricionales. |
| MenusDiarios | Registra los menús diarios. |
| MenuAlimentos | Relaciona los alimentos con los menús. |
| LogsAuditoria | Almacena registros de auditoría. |

### Roles del sistema

| Rol | Responsabilidad |
|---|---|
| Administrador | Control general del sistema. |
| Docente | Seguimiento de estudiantes. |
| Estudiante | Consulta y registro de información. |
| Nutricionista | Gestión nutricional y alimentos. |

## 🚀 Instalación y ejecución

### Requisitos previos

- Visual Studio 2022 o una versión compatible.
- SDK de .NET 10.
- SQL Server LocalDB.
- SQL Server Management Studio (opcional).
- Git.

### 1. Clonar el repositorio

```bash
git clone https://github.com/chrischidori27-debug/SmartNutriTracker.git
```

### 2. Abrir el proyecto

Ingresa a la carpeta del proyecto y abre `SmartNutriTracker.csproj` con Visual Studio.

### 3. Configurar la conexión a SQL Server

Abre `appsettings.json` y verifica que la cadena de conexión corresponda a tu instancia de SQL Server LocalDB:

```json
{
  "ConnectionStrings": {
    "ConexionSQL": "Server=(localdb)\\MSSQLLocalDB;Database=SmartNutriTracker;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Asegúrate de que la base de datos y sus tablas estén creadas antes de ejecutar la aplicación.

### 4. Restaurar dependencias

```bash
dotnet restore
```

### 5. Compilar el proyecto

```bash
dotnet build
```

### 6. Ejecutar la aplicación

```bash
dotnet run
```

También puedes iniciar el proyecto directamente desde Visual Studio.

## 📁 Estructura del proyecto

```text
SmartNutriTracker/
│
├── Components/
│   ├── Layout/
│   │   ├── MainLayout.razor
│   │   └── NavMenu.razor
│   │
│   ├── Pages/
│   │   ├── Home.razor
│   │   ├── Login.razor
│   │   └── Registro.razor
│   │
│   ├── App.razor
│   └── Routes.razor
│
├── Modelos/
│   ├── Usuario.cs
│   ├── Role.cs
│   └── SmartNutriTrackerContext.cs
│
├── Servicios/
│   └── UsuarioServicio.cs
│
├── wwwroot/
│   ├── app.css
│   └── favicon.png
│
├── appsettings.json
├── Program.cs
└── SmartNutriTracker.csproj
```

## 🔐 Seguridad

SmartNutriTracker contempla medidas de protección de información:

- Contraseñas almacenadas mediante hash.
- Validación de datos durante el registro.
- Comprobación de correos electrónicos duplicados.
- Validación de los roles permitidos en el registro público.
- Separación de responsabilidades mediante servicios y modelos.

## 👥 Equipo de desarrollo

Proyecto académico desarrollado como parte de la asignatura de **Programación Web III**.

| Información | Detalle |
|---|---|
| Institución | Universidad del Valle (UNIVALLE) |
| Proyecto | SmartNutriTracker |
| Asignatura | Programación Web III |
| Repositorio | [GitHub - SmartNutriTracker](https://github.com/chrischidori27-debug/SmartNutriTracker) |

## 📌 Estado del proyecto

<div align="center">

<img src="https://img.shields.io/badge/Version-1.0-22C55E?style=for-the-badge">
<img src="https://img.shields.io/badge/Estado-En%20desarrollo-2563EB?style=for-the-badge">

<br><br>


</div>
