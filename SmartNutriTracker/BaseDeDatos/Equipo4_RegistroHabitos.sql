USE SmartNutriTracker;
GO

IF OBJECT_ID(N'dbo.ConsumosAlimento', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ConsumosAlimento (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UsuarioId INT NOT NULL,
        AlimentoId INT NOT NULL,
        TipoComida NVARCHAR(20) NOT NULL,
        Porciones DECIMAL(10,2) NOT NULL,
        Fecha DATE NOT NULL,
        Calorias DECIMAL(10,2) NOT NULL,
        Proteinas DECIMAL(10,2) NOT NULL,
        Carbohidratos DECIMAL(10,2) NOT NULL,
        Grasas DECIMAL(10,2) NOT NULL,
        FechaRegistro DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

        CONSTRAINT FK_ConsumosAlimento_Usuarios
            FOREIGN KEY (UsuarioId) REFERENCES dbo.Usuarios(Id),

        CONSTRAINT FK_ConsumosAlimento_Alimentos
            FOREIGN KEY (AlimentoId) REFERENCES dbo.Alimentos(Id),

        CONSTRAINT CK_ConsumosAlimento_Porciones CHECK (Porciones > 0),

        CONSTRAINT CK_ConsumosAlimento_TipoComida
            CHECK (TipoComida IN (N'Desayuno', N'Media mañana', N'Almuerzo', N'Merienda', N'Cena'))
    );

    CREATE INDEX IX_ConsumosAlimento_Usuario_Fecha
        ON dbo.ConsumosAlimento (UsuarioId, Fecha);
END
GO

IF OBJECT_ID(N'dbo.HabitosDiarios', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.HabitosDiarios (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UsuarioId INT NOT NULL,
        Fecha DATE NOT NULL,
        HorasSueno DECIMAL(4,2) NOT NULL,
        AguaLitros DECIMAL(4,2) NOT NULL,
        MinutosActividad INT NOT NULL,
        TipoActividad NVARCHAR(80) NULL,
        FechaRegistro DATETIME2 NOT NULL DEFAULT SYSDATETIME(),

        CONSTRAINT FK_HabitosDiarios_Usuarios
            FOREIGN KEY (UsuarioId) REFERENCES dbo.Usuarios(Id),

        CONSTRAINT UQ_HabitosDiarios_Usuario_Fecha UNIQUE (UsuarioId, Fecha),

        CONSTRAINT CK_HabitosDiarios_Sueno CHECK (HorasSueno BETWEEN 0 AND 24),
        CONSTRAINT CK_HabitosDiarios_Agua CHECK (AguaLitros BETWEEN 0 AND 10),
        CONSTRAINT CK_HabitosDiarios_Actividad CHECK (MinutosActividad BETWEEN 0 AND 600)
    );
END
GO

SELECT * FROM dbo.ConsumosAlimento;
SELECT * FROM dbo.HabitosDiarios;
GO
