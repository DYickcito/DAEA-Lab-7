USE master;
GO

IF DB_ID('BibliotecaDB') IS NOT NULL
BEGIN
    ALTER DATABASE BibliotecaDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE BibliotecaDB;
END
GO

CREATE DATABASE BibliotecaDB;
GO

USE BibliotecaDB;
GO

CREATE TABLE Autores (
    AutorId       INT IDENTITY(1,1) CONSTRAINT PK_Autores PRIMARY KEY,
    Nombre        NVARCHAR(100) NOT NULL,
    Nacionalidad  NVARCHAR(50)  NULL,
    Activo        BIT NOT NULL CONSTRAINT DF_Autores_Activo DEFAULT 1
);

CREATE TABLE Libros (
    LibroId     INT IDENTITY(1,1) CONSTRAINT PK_Libros PRIMARY KEY,
    Titulo      NVARCHAR(200) NOT NULL,
    ISBN        NVARCHAR(20)  NOT NULL CONSTRAINT UQ_Libros_ISBN UNIQUE,
    AutorId     INT NOT NULL CONSTRAINT FK_Libros_Autores REFERENCES Autores(AutorId),
    Ejemplares  INT NOT NULL CONSTRAINT CK_Libros_Ejemplares CHECK (Ejemplares >= 0),
    Activo      BIT NOT NULL CONSTRAINT DF_Libros_Activo DEFAULT 1
);

CREATE TABLE Socios (
    SocioId  INT IDENTITY(1,1) CONSTRAINT PK_Socios PRIMARY KEY,
    DNI      NVARCHAR(8)   NOT NULL CONSTRAINT UQ_Socios_DNI UNIQUE,
    Nombre   NVARCHAR(100) NOT NULL,
    Email    NVARCHAR(100) NULL,
    Activo   BIT NOT NULL CONSTRAINT DF_Socios_Activo DEFAULT 1
);

CREATE TABLE Prestamos (
    PrestamoId     INT IDENTITY(1,1) CONSTRAINT PK_Prestamos PRIMARY KEY,
    SocioId        INT NOT NULL CONSTRAINT FK_Prestamos_Socios REFERENCES Socios(SocioId),
    FechaPrestamo  DATETIME NOT NULL,
    FechaLimite    DATETIME NOT NULL,
    Estado         NVARCHAR(20) NOT NULL CONSTRAINT DF_Prestamos_Estado DEFAULT 'Pendiente',
    CONSTRAINT CK_Prestamos_Estado CHECK (Estado IN ('Pendiente', 'Devuelto')),
    CONSTRAINT CK_Prestamos_Fechas CHECK (FechaLimite >= FechaPrestamo)
);

CREATE TABLE DetallePrestamo (
    PrestamoId       INT NOT NULL CONSTRAINT FK_Detalle_Prestamos REFERENCES Prestamos(PrestamoId),
    LibroId          INT NOT NULL CONSTRAINT FK_Detalle_Libros REFERENCES Libros(LibroId),
    FechaDevolucion  DATETIME NULL,
    CONSTRAINT PK_DetallePrestamo PRIMARY KEY (PrestamoId, LibroId)
);
GO

INSERT INTO Autores (Nombre, Nacionalidad) VALUES
(N'Gabriel García Márquez', N'Colombiana'),
(N'Mario Vargas Llosa',     N'Peruana'),
(N'Isabel Allende',         N'Chilena'),
(N'Jorge Luis Borges',      N'Argentina'),
(N'Julio Cortázar',         N'Argentina'),
(N'Ricardo Palma',          N'Peruana'),
(N'Miguel de Cervantes',    N'Española'),
(N'George Orwell',          N'Británica');

INSERT INTO Libros (Titulo, ISBN, AutorId, Ejemplares, Activo) VALUES
(N'Cien años de soledad',              N'9780307474728', 1, 2, 1),
(N'El amor en los tiempos del cólera', N'9780307389732', 1, 1, 1),
(N'Crónica de una muerte anunciada',   N'9781400034956', 1, 3, 1),
(N'La ciudad y los perros',            N'9788432217432', 2, 2, 1),
(N'Conversación en La Catedral',       N'9788420471914', 2, 2, 1),
(N'La casa verde',                     N'9788466318518', 2, 1, 1),
(N'La casa de los espíritus',          N'9780553383805', 3, 2, 1),
(N'Eva Luna',                          N'9780060927363', 3, 3, 1),
(N'Ficciones',                         N'9780802130303', 4, 2, 1),
(N'El Aleph',                          N'9780142437889', 4, 0, 1),
(N'Rayuela',                           N'9780394752846', 5, 2, 1),
(N'Bestiario',                         N'9788420471587', 5, 3, 1),
(N'Tradiciones peruanas',              N'9786123030001', 6, 4, 1),
(N'Don Quijote de la Mancha',          N'9788420412146', 7, 5, 1),
(N'Novelas ejemplares',                N'9788437604091', 7, 2, 1),
(N'1984',                              N'9780451524935', 8, 4, 1),
(N'Rebelión en la granja',             N'9780451526342', 8, 3, 1),
(N'Homenaje a Cataluña',               N'9780156421171', 8, 1, 1),
(N'Pantaleón y las visitadoras',       N'9788432217449', 2, 2, 1),
(N'Los funerales de la Mamá Grande',   N'9780060882860', 1, 1, 0);

INSERT INTO Socios (DNI, Nombre, Email, Activo) VALUES
(N'40123456', N'Ana Torres Ríos',      N'ana.torres@mail.com',   1),
(N'40234567', N'Luis Mendoza Paredes', N'luis.mendoza@mail.com', 1),
(N'40345678', N'María Quispe Huamán',  N'maria.quispe@mail.com', 1),
(N'40456789', N'Carlos Ramos Vega',    N'carlos.ramos@mail.com', 1),
(N'40567890', N'Lucía Flores Rojas',   N'lucia.flores@mail.com', 1),
(N'40678901', N'Jorge Castillo León',  NULL,                     1),
(N'40789012', N'Rosa Delgado Soto',    N'rosa.delgado@mail.com', 1),
(N'40890123', N'Pedro Salazar Cruz',   N'pedro.salazar@mail.com',1),
(N'40901234', N'Elena Vargas Díaz',    N'elena.vargas@mail.com', 1),
(N'41012345', N'Raúl Medina Ochoa',    N'raul.medina@mail.com',  0);

DECLARE @hoy DATETIME = CAST(CAST(GETDATE() AS DATE) AS DATETIME);

INSERT INTO Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
VALUES (1, DATEADD(DAY, -20, @hoy), DATEADD(DAY, -13, @hoy), 'Pendiente');
INSERT INTO DetallePrestamo (PrestamoId, LibroId, FechaDevolucion) VALUES
(1, 1, NULL), (1, 2, NULL), (1, 3, NULL);

INSERT INTO Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
VALUES (2, DATEADD(DAY, -30, @hoy), DATEADD(DAY, -23, @hoy), 'Devuelto');
INSERT INTO DetallePrestamo (PrestamoId, LibroId, FechaDevolucion) VALUES
(2, 4, DATEADD(DAY, -20, @hoy));

INSERT INTO Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
VALUES (3, DATEADD(DAY, -5, @hoy), DATEADD(DAY, 2, @hoy), 'Pendiente');
INSERT INTO DetallePrestamo (PrestamoId, LibroId, FechaDevolucion) VALUES
(3, 5, DATEADD(DAY, -1, @hoy)), (3, 6, NULL);

INSERT INTO Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
VALUES (4, DATEADD(DAY, -10, @hoy), DATEADD(DAY, -3, @hoy), 'Pendiente');
INSERT INTO DetallePrestamo (PrestamoId, LibroId, FechaDevolucion) VALUES
(4, 7, NULL);

INSERT INTO Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
VALUES (5, DATEADD(DAY, -8, @hoy), DATEADD(DAY, -1, @hoy), 'Devuelto');
INSERT INTO DetallePrestamo (PrestamoId, LibroId, FechaDevolucion) VALUES
(5, 8, DATEADD(DAY, -2, @hoy)), (5, 9, DATEADD(DAY, -2, @hoy));
GO
