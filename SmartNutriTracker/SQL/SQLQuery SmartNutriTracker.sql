CREATE TABLE Usuarios (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL,
    Correo NVARCHAR(150) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL,
    RolId INT NOT NULL,
    Estado BIT NOT NULL DEFAULT 1,
    FechaRegistro DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
 
    CONSTRAINT FK_Usuarios_Roles
    FOREIGN KEY (RolId) REFERENCES Roles(Id)
);
 
CREATE TABLE Roles (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL UNIQUE,
    Descripcion NVARCHAR(150)
);
 
CREATE TABLE LogsAuditoria (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioId INT NULL,
    Accion NVARCHAR(100) NOT NULL,
    Descripcion NVARCHAR(500),
    Fecha DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
 
    CONSTRAINT FK_LogsAuditoria_Usuarios
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id)
);
INSERT INTO Roles (Nombre, Descripcion)
VALUES
('Administrador', 'Control total del sistema'),
('Docente', 'Seguimiento de estudiantes'),
('Estudiante', 'Consulta y registro de información'),
('Nutricionista', 'Gestión nutricional y alimentos');

INSERT INTO Usuarios (Nombre, Correo, PasswordHash, RolId, Estado, FechaRegistro) VALUES
('Camilo', 'camilo@smartnutri.com', 'AQAAAAEAACcQAAAAAEI67sB25gVTZlNlVPtIYmsc8P78oPkAf+rXOb...', 3, 1, GETDATE()),
('Américo', 'americo@smartnutri.com', 'AQAAAAEAACcQAAAAAEI67sB25gVTZlNlVPtIYmsc8P78oPkAf+rXOb...', 3, 1, GETDATE()),
('Guzmán', 'guzman@smartnutri.com', 'AQAAAAEAACcQAAAAAEI67sB25gVTZlNlVPtIYmsc8P78oPkAf+rXOb...', 3, 1, GETDATE()),
('Oliver', 'oliver@smartnutri.com', 'AQAAAAEAACcQAAAAAEI67sB25gVTZlNlVPtIYmsc8P78oPkAf+rXOb...', 3, 1, GETDATE()),
('Chris', 'chris@smartnutri.com', 'AQAAAAEAACcQAAAAAEI67sB25gVTZlNlVPtIYmsc8P78oPkAf+rXOb...', 3, 1, GETDATE());