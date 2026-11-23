# SmartNutriTracker · Equipo 4 · Registro de Hábitos

Módulo de **registro de consumo real de alimentos** y **hábitos diarios** del estudiante. Forma parte del proyecto Blazor Server de SmartNutriTracker (.NET 10, EF Core, SQL Server) y reutiliza el inicio de sesión (`UsuarioSesion`) y el catálogo de alimentos (`AlimentoServicio`) de los equipos 1 y 3.

---

## 1. Historias de usuario implementadas

| HU | Descripción | Pantalla |
|---|---|---|
| **HU-19** | Registrar consumo real de alimentos para generar un historial nutricional | `/consumo` |
| **HU-20** | Registrar hábitos diarios: horas de sueño, agua y actividad física | `/habitos` |

### HU-19 · Consumo real de alimentos
- Selección de la **comida del día**: Desayuno, Media mañana, Almuerzo, Merienda o Cena.
- Búsqueda y selección de un alimento del catálogo activo del Equipo 3.
- Cantidad en **porciones** (de 0.5 en 0.5, máximo 20).
- Vista previa en vivo del aporte: calorías, proteínas, carbohidratos y grasas.
- Las calorías y macronutrientes se **calculan automáticamente** (valor del alimento × porciones) y se guardan junto al registro, de modo que el historial no cambia si luego se edita el alimento.
- Resumen del día: totales, barra de progreso frente a la referencia de 2000 kcal y distribución de macronutrientes.
- Historial del día agrupado por comida, con eliminación de registros (con confirmación).
- Gráfico de calorías de los últimos 7 días; al pulsar un día se carga ese día.

### HU-20 · Hábitos diarios
- Horas de sueño, litros de agua y minutos de actividad física, con botones − / + o escritura directa.
- Tipo de actividad (Caminata, Correr, Fútbol, Ciclismo, Natación, Baile, Otro).
- Barras de progreso y estado frente a la meta de cada hábito.
- **Valoración del día**: cuántas de las 3 metas se cumplieron.
- Un registro por estudiante y día: si se guarda de nuevo, se **actualiza**.
- Historial de los últimos 7 días; al pulsar una fila se carga ese día.

### Metas de referencia
| Hábito | Meta |
|---|---|
| Sueño | 8 horas |
| Agua | 2 litros |
| Actividad física | 60 minutos |
| Calorías (resumen del día) | 2000 kcal |

Son valores de referencia fijos. Cuando exista el perfil del estudiante (Equipo 2) pueden personalizarse por edad, IMC y objetivo.

---

## 2. Estructura del módulo

Archivos **nuevos**:

```
SmartNutriTracker/
├── BaseDeDatos/
│   ├── Equipo4_RegistroHabitos.sql          Crea las tablas del módulo
│   └── Equipo4_AlimentosDePrueba.sql        Alimentos de ejemplo (solo si el catálogo está vacío)
└── SmartNutriTracker/
    ├── Modelos/
    │   ├── ConsumoAlimento.cs               Entidad del consumo real
    │   ├── HabitoDiario.cs                  Entidad de hábitos diarios
    │   └── SmartNutriTrackerContext.RegistroHabitos.cs   DbSet de ambas entidades
    ├── Servicios/
    │   ├── ConsumoServicio.cs               Lógica de HU-19
    │   └── HabitoServicio.cs                Lógica de HU-20
    └── Components/Pages/
        ├── ConsumoAlimentos.razor / .razor.css    Pantalla /consumo
        └── HabitosDiarios.razor / .razor.css      Pantalla /habitos
```

Archivos **existentes con cambios mínimos**:

| Archivo | Cambio |
|---|---|
| `Program.cs` | Registro de `ConsumoServicio` y `HabitoServicio` (2 líneas) |
| `Components/Layout/NavMenu.razor` | Sección "REGISTRO DE HÁBITOS" con dos enlaces |

El contexto se amplió con una clase **parcial** (`SmartNutriTrackerContext.RegistroHabitos.cs`), por lo que `SmartNutriTrackerContext.cs` no se modificó.

---

## 3. Modelo de datos

### `ConsumosAlimento`
| Columna | Tipo | Detalle |
|---|---|---|
| Id | INT IDENTITY | Clave primaria |
| UsuarioId | INT | FK → `Usuarios.Id` |
| AlimentoId | INT | FK → `Alimentos.Id` |
| TipoComida | NVARCHAR(20) | Desayuno, Media mañana, Almuerzo, Merienda, Cena |
| Porciones | DECIMAL(10,2) | Mayor a 0 |
| Fecha | DATE | Día del consumo |
| Calorias, Proteinas, Carbohidratos, Grasas | DECIMAL(10,2) | Valores calculados al registrar |
| FechaRegistro | DATETIME2 | Por defecto `SYSDATETIME()` |

Índice por `(UsuarioId, Fecha)` para consultar rápido el historial.

### `HabitosDiarios`
| Columna | Tipo | Detalle |
|---|---|---|
| Id | INT IDENTITY | Clave primaria |
| UsuarioId | INT | FK → `Usuarios.Id` |
| Fecha | DATE | Día del registro |
| HorasSueno | DECIMAL(4,2) | Entre 0 y 24 |
| AguaLitros | DECIMAL(4,2) | Entre 0 y 10 |
| MinutosActividad | INT | Entre 0 y 600 |
| TipoActividad | NVARCHAR(80) | Opcional |
| FechaRegistro | DATETIME2 | Por defecto `SYSDATETIME()` |

Restricción única `(UsuarioId, Fecha)`: un solo registro de hábitos por estudiante y día.

### Relaciones
```
Usuarios 1 ──── N ConsumosAlimento N ──── 1 Alimentos
Usuarios 1 ──── N HabitosDiarios
```

---

## 4. Servicios

### `ConsumoServicio`
| Método | Función |
|---|---|
| `ListarPorDiaAsync(usuarioId, fecha)` | Consumos del día con su alimento |
| `RegistrarAsync(usuarioId, alimentoId, tipoComida, porciones, fecha)` | Valida, calcula calorías y macros, y guarda |
| `EliminarAsync(usuarioId, consumoId)` | Elimina un registro propio |
| `ResumenSemanalAsync(usuarioId, fechaFin)` | Calorías de los últimos 7 días |

### `HabitoServicio`
| Método | Función |
|---|---|
| `ObtenerAsync(usuarioId, fecha)` | Hábitos de un día |
| `ListarRecientesAsync(usuarioId, fechaFin, dias)` | Hábitos de los últimos días |
| `GuardarAsync(usuarioId, fecha, horasSueno, aguaLitros, minutosActividad, tipoActividad)` | Crea o actualiza el registro del día |

Todos los métodos devuelven `(bool Exito, string Mensaje)` en las operaciones de escritura, igual que el resto de servicios del proyecto.

### Validaciones
- No se aceptan fechas futuras.
- Comida del día válida, alimento activo y porciones entre 0 y 20.
- Sueño entre 0 y 24 h, agua entre 0 y 10 L, actividad entre 0 y 600 min.
- Cada usuario solo consulta y elimina **sus propios** registros (se filtra por `UsuarioId` de la sesión).
- Las mismas reglas están como restricciones `CHECK` en SQL Server.

---

## 5. Integración con otros equipos

| Equipo | Dependencia |
|---|---|
| Equipo 1 · Autenticación | Se usa `UsuarioSesion` (`UsuarioId`, `EstaAutenticado`). Sin sesión iniciada, ambas pantallas redirigen a `/login`. |
| Equipo 3 · Alimentos | Se usa `AlimentoServicio.ListarAsync()` y la tabla `Alimentos` para calcular los valores nutricionales. |
| Equipo 2 · Perfil | No es requisito. Más adelante puede aportar metas personalizadas. |
| Equipo 5 y 6 | Pueden leer `ConsumosAlimento` y `HabitosDiarios` para recomendaciones y alertas. |

---

## 6. Cómo ejecutar

1. En SSMS, conectado a `(localdb)\MSSQLLocalDB`, abrir y ejecutar `BaseDeDatos/Equipo4_RegistroHabitos.sql`. Requiere que la base `SmartNutriTracker` ya tenga las tablas `Usuarios` y `Alimentos`.
2. Opcional, si el catálogo está vacío: ejecutar `BaseDeDatos/Equipo4_AlimentosDePrueba.sql`.
3. Abrir `SmartNutriTracker.slnx` en Visual Studio y ejecutar con **F5** (`http://localhost:5025`).
4. Iniciar sesión con un usuario existente (por ejemplo `oliver@smartnutri.com`).
5. En el menú lateral, sección **REGISTRO DE HÁBITOS**: *Consumo de alimentos* y *Hábitos diarios*.

---

## 7. Pruebas manuales

| Caso | Resultado esperado |
|---|---|
| Entrar a `/consumo` sin iniciar sesión | Redirige a `/login` |
| Registrar un consumo válido | Mensaje de éxito, aparece en el historial y suben los totales |
| Registrar sin elegir alimento | Mensaje "Selecciona un alimento." |
| Registrar con fecha futura | No permite la fecha (el selector tiene tope en hoy) y el servicio también lo rechaza |
| Eliminar un registro | Pide confirmación y lo quita del historial y de los totales |
| Cambiar la fecha | Se cargan los consumos de ese día |
| Guardar hábitos del día | Mensaje de éxito y fila en el historial de 7 días |
| Guardar hábitos otra vez el mismo día | Se actualiza, no se duplica |
| Verificar en SQL Server | `SELECT * FROM ConsumosAlimento;` y `SELECT * FROM HabitosDiarios;` muestran los registros del usuario |

---

## 8. Alcance y pendientes

- Incluido: **HU-19** y **HU-20**.
- Relacionado y pendiente del equipo: **HU-21** (historial completo de hábitos, hoy cubierto de forma resumida por los últimos 7 días) y **HU-22** (modificar registros diarios; los hábitos ya se actualizan al guardar de nuevo, falta la edición de consumos).
