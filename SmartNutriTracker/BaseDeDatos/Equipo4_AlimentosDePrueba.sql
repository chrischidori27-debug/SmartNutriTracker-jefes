USE SmartNutriTracker;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Alimentos)
BEGIN
    INSERT INTO dbo.Alimentos (Nombre, Descripcion, Categoria, Porcion, Calorias, Proteinas, Carbohidratos, Grasas, Fibra)
    VALUES
    (N'Arroz cocido', N'Arroz blanco cocido', N'Cereales', N'100 g', 130, 2.7, 28, 0.3, 0.4),
    (N'Pan integral', N'Rebanada de pan integral', N'Cereales', N'1 rebanada', 80, 4, 14, 1, 2),
    (N'Huevo cocido', N'Huevo entero cocido', N'Proteínas', N'1 unidad', 78, 6.3, 0.6, 5.3, 0),
    (N'Pechuga de pollo', N'Pechuga a la plancha', N'Proteínas', N'100 g', 165, 31, 0, 3.6, 0),
    (N'Leche entera', N'Leche de vaca entera', N'Lácteos', N'1 vaso (250 ml)', 150, 8, 12, 8, 0),
    (N'Manzana', N'Manzana mediana con cáscara', N'Frutas', N'1 unidad', 95, 0.5, 25, 0.3, 4.4),
    (N'Plátano', N'Plátano mediano', N'Frutas', N'1 unidad', 105, 1.3, 27, 0.4, 3.1),
    (N'Papa cocida', N'Papa cocida sin cáscara', N'Tubérculos', N'100 g', 87, 1.9, 20, 0.1, 1.8),
    (N'Ensalada mixta', N'Lechuga, tomate y zanahoria', N'Verduras', N'1 plato', 45, 2, 9, 0.5, 3),
    (N'Fideos cocidos', N'Fideos de trigo cocidos', N'Cereales', N'100 g', 158, 5.8, 31, 0.9, 1.8);
END
GO

SELECT * FROM dbo.Alimentos;
GO
