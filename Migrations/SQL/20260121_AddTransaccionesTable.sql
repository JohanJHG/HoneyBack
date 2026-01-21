-- ============================================
-- MIGRACION: Crear tabla Transacciones
-- Fecha: 2026-01-21
-- Descripcion: Agrega la tabla Transacciones que faltaba en el backend
-- ============================================

-- Verificar si la tabla ya existe antes de crearla
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Transacciones]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Transacciones] (
        [TransaccionID] INT IDENTITY(1,1) NOT NULL,
        [UsuarioID] INT NOT NULL,
        [Nombre] NVARCHAR(200) NOT NULL,
        [Monto] DECIMAL(15, 2) NOT NULL,
        [Tipo] NVARCHAR(20) NOT NULL, -- 'ingreso' o 'gasto'
        [Fecha] DATE NOT NULL,
        [Categoria] NVARCHAR(50) NULL,
        [FechaCreacion] DATETIME NOT NULL DEFAULT GETDATE(),
        
        CONSTRAINT [PK_Transacciones] PRIMARY KEY CLUSTERED ([TransaccionID] ASC),
        CONSTRAINT [FK_Transacciones_Usuarios] FOREIGN KEY ([UsuarioID]) 
            REFERENCES [dbo].[Usuarios] ([UsuarioID]) ON DELETE CASCADE,
        CONSTRAINT [CK_Transacciones_Tipo] CHECK ([Tipo] IN ('ingreso', 'gasto')),
        CONSTRAINT [CK_Transacciones_Monto] CHECK ([Monto] > 0)
    );

    -- Indices para mejorar rendimiento
    CREATE NONCLUSTERED INDEX [IDX_Transacciones_Usuario] ON [dbo].[Transacciones] ([UsuarioID]);
    CREATE NONCLUSTERED INDEX [IDX_Transacciones_Fecha] ON [dbo].[Transacciones] ([Fecha] DESC);
    CREATE NONCLUSTERED INDEX [IDX_Transacciones_Tipo] ON [dbo].[Transacciones] ([Tipo]);
    CREATE NONCLUSTERED INDEX [IDX_Transacciones_Usuario_Fecha] ON [dbo].[Transacciones] ([UsuarioID], [Fecha] DESC);

    PRINT 'Tabla Transacciones creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla Transacciones ya existe.';
END
GO

-- ============================================
-- MIGRACION: Agregar campos faltantes a ConfiguracionesUsuario
-- ============================================

-- Agregar MonedaPreferida si no existe
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ConfiguracionesUsuario]') AND name = 'MonedaPreferida')
BEGIN
    ALTER TABLE [dbo].[ConfiguracionesUsuario] 
    ADD [MonedaPreferida] NVARCHAR(3) NULL DEFAULT 'COP';
    PRINT 'Columna MonedaPreferida agregada.';
END
GO

-- Agregar NombreUsuario si no existe
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ConfiguracionesUsuario]') AND name = 'NombreUsuario')
BEGIN
    ALTER TABLE [dbo].[ConfiguracionesUsuario] 
    ADD [NombreUsuario] NVARCHAR(100) NULL;
    PRINT 'Columna NombreUsuario agregada.';
END
GO

-- Agregar AvatarURL si no existe
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ConfiguracionesUsuario]') AND name = 'AvatarURL')
BEGIN
    ALTER TABLE [dbo].[ConfiguracionesUsuario] 
    ADD [AvatarURL] NVARCHAR(500) NULL;
    PRINT 'Columna AvatarURL agregada.';
END
GO

-- Agregar EsVeterano si no existe
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ConfiguracionesUsuario]') AND name = 'EsVeterano')
BEGIN
    ALTER TABLE [dbo].[ConfiguracionesUsuario] 
    ADD [EsVeterano] BIT NULL DEFAULT 0;
    PRINT 'Columna EsVeterano agregada.';
END
GO

-- Agregar FechaCreacion si no existe
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[ConfiguracionesUsuario]') AND name = 'FechaCreacion')
BEGIN
    ALTER TABLE [dbo].[ConfiguracionesUsuario] 
    ADD [FechaCreacion] DATETIME NULL DEFAULT GETDATE();
    PRINT 'Columna FechaCreacion agregada.';
END
GO

PRINT 'Migracion completada exitosamente.';
