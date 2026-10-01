USE BibliotecaDB;
GO

IF EXISTS (SELECT 1 FROM Autores WHERE Nombre = N'Gabriela Mistral')
BEGIN
    PRINT 'Los datos extra ya estaban cargados. No se hizo nada.';
    SET NOEXEC ON;
END
GO

INSERT INTO Autores (Nombre, Nacionalidad) VALUES
(N'Gabriela Mistral',          N'Chilena'),
(N'Octavio Paz',               N'Mexicana'),
(N'Pablo Neruda',              N'Chilena'),
(N'Juan Rulfo',                N'Mexicana'),
(N'César Vallejo',             N'Peruana'),
(N'José María Arguedas',       N'Peruana');

INSERT INTO Libros (Titulo, ISBN, AutorId, Ejemplares)
SELECT v.Titulo, v.ISBN, a.AutorId, v.Ejemplares
FROM (VALUES
    (N'Desolación',                                        N'9780000100011', N'Gabriela Mistral',    3),
    (N'Ternura',                                           N'9780000100012', N'Gabriela Mistral',    2),
    (N'Tala',                                              N'9780000100013', N'Gabriela Mistral',    2),
    (N'El laberinto de la soledad',                        N'9780000100021', N'Octavio Paz',         4),
    (N'Piedra de sol',                                     N'9780000100022', N'Octavio Paz',         2),
    (N'Libertad bajo palabra',                             N'9780000100023', N'Octavio Paz',         1),
    (N'Veinte poemas de amor y una canción desesperada',   N'9780000100031', N'Pablo Neruda',        5),
    (N'Canto general',                                     N'9780000100032', N'Pablo Neruda',        3),
    (N'Residencia en la tierra',                           N'9780000100033', N'Pablo Neruda',        2),
    (N'Confieso que he vivido',                            N'9780000100034', N'Pablo Neruda',        2),
    (N'Pedro Páramo',                                      N'9780000100041', N'Juan Rulfo',          4),
    (N'El llano en llamas',                                N'9780000100042', N'Juan Rulfo',          3),
    (N'Trilce',                                            N'9780000100051', N'César Vallejo',       2),
    (N'Los heraldos negros',                               N'9780000100052', N'César Vallejo',       3),
    (N'Poemas humanos',                                    N'9780000100053', N'César Vallejo',       2),
    (N'Los ríos profundos',                                N'9780000100061', N'José María Arguedas', 3),
    (N'Todas las sangres',                                 N'9780000100062', N'José María Arguedas', 2),
    (N'El zorro de arriba y el zorro de abajo',            N'9780000100063', N'José María Arguedas', 1),
    (N'El libro de arena',                                 N'9780000100071', N'Jorge Luis Borges',   2),
    (N'Historias de cronopios y de famas',                 N'9780000100072', N'Julio Cortázar',      3)
) AS v(Titulo, ISBN, AutorNombre, Ejemplares)
INNER JOIN Autores a ON a.Nombre = v.AutorNombre;

INSERT INTO Socios (DNI, Nombre, Email, Activo) VALUES
(N'42000011', N'Valeria Cárdenas Lara',   N'valeria.cardenas@mail.com', 1),
(N'42000012', N'Diego Herrera Campos',    N'diego.herrera@mail.com',    1),
(N'42000013', N'Camila Zevallos Rivas',   N'camila.zevallos@mail.com',  1),
(N'42000014', N'Andrés Palomino Gil',     N'andres.palomino@mail.com',  1),
(N'42000015', N'Sofía Benavides Luna',    N'sofia.benavides@mail.com',  1),
(N'42000016', N'Mateo Uribe Saldaña',     NULL,                         1),
(N'42000017', N'Daniela Ponce Arce',      N'daniela.ponce@mail.com',    1),
(N'42000018', N'Gustavo Lozano Bravo',    N'gustavo.lozano@mail.com',   1),
(N'42000019', N'Paola Cornejo Yupanqui',  N'paola.cornejo@mail.com',    1),
(N'42000020', N'Iván Bustamante Ríos',    N'ivan.bustamante@mail.com',  0);

DECLARE @hoy DATETIME = CAST(CAST(GETDATE() AS DATE) AS DATETIME);

DECLARE @d TABLE (n INT, dni NVARCHAR(8), dp INT, dl INT, isbn NVARCHAR(20), dev INT NULL);
INSERT INTO @d (n, dni, dp, dl, isbn, dev) VALUES
(1, N'42000011', -12,  -5, N'9780000100011', NULL),
(1, N'42000011', -12,  -5, N'9780000100031', NULL),
(2, N'42000012', -25, -18, N'9780000100041', -14),
(3, N'42000013',  -3,   4, N'9780000100021', NULL),
(3, N'42000013',  -3,   4, N'9780000100051', NULL),
(3, N'42000013',  -3,   4, N'9780000100061', NULL),
(4, N'42000014',  -9,  -2, N'9780000100032', -5),
(4, N'42000014',  -9,  -2, N'9780000100033', NULL),
(5, N'42000015', -20, -13, N'9780000100042', -15),
(6, N'42000016',  -1,   6, N'9780000100052', NULL),
(6, N'42000016',  -1,   6, N'9780000100062', NULL),
(7, N'42000017', -40, -33, N'9780000100022', NULL),
(8, N'42000018', -35, -28, N'9780000100012', -26),
(8, N'42000018', -35, -28, N'9780000100013', -26),
(8, N'42000018', -35, -28, N'9780000100023', -26),
(9, N'40234567',  -2,   5, N'9780000100034', NULL);

DECLARE @n INT = 1, @p INT;
WHILE @n <= 9
BEGIN
    INSERT INTO Prestamos (SocioId, FechaPrestamo, FechaLimite, Estado)
    SELECT TOP 1 s.SocioId, DATEADD(DAY, d.dp, @hoy), DATEADD(DAY, d.dl, @hoy), 'Pendiente'
    FROM @d d INNER JOIN Socios s ON s.DNI = d.dni
    WHERE d.n = @n;

    SET @p = SCOPE_IDENTITY();

    INSERT INTO DetallePrestamo (PrestamoId, LibroId, FechaDevolucion)
    SELECT @p, l.LibroId, CASE WHEN d.dev IS NULL THEN NULL ELSE DATEADD(DAY, d.dev, @hoy) END
    FROM @d d INNER JOIN Libros l ON l.ISBN = d.isbn
    WHERE d.n = @n;

    UPDATE Prestamos SET Estado = 'Devuelto'
    WHERE PrestamoId = @p
      AND NOT EXISTS (SELECT 1 FROM DetallePrestamo WHERE PrestamoId = @p AND FechaDevolucion IS NULL);

    SET @n += 1;
END

UPDATE l SET Ejemplares = l.Ejemplares - x.Pendientes
FROM Libros l
INNER JOIN (
    SELECT d.LibroId, COUNT(*) AS Pendientes
    FROM DetallePrestamo d
    INNER JOIN Prestamos p ON p.PrestamoId = d.PrestamoId
    WHERE d.FechaDevolucion IS NULL AND p.PrestamoId > 5
    GROUP BY d.LibroId
) x ON x.LibroId = l.LibroId;
GO

SET NOEXEC OFF;
GO
