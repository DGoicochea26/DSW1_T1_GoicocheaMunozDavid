IF DB_ID(N'BDBIBLIOTECA2026') IS NULL
    CREATE DATABASE BDBIBLIOTECA2026;
GO

USE BDBIBLIOTECA2026;
GO

IF OBJECT_ID(N'dbo.Libros', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Libros
    (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        NombreLibro VARCHAR(120) NOT NULL,
        Autor VARCHAR(100) NOT NULL,
        NombreEditorial VARCHAR(100) NOT NULL,
        ISDN VARCHAR(20) NOT NULL
    );
END
GO

IF OBJECT_ID(N'dbo.Categorias', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categorias
    (
        Id INT IDENTITY(1,1) CONSTRAINT PK_Categorias PRIMARY KEY,
        NombreCategoria VARCHAR(80) NOT NULL
            CONSTRAINT UQ_Categorias_Nombre UNIQUE
    );
END
GO

IF OBJECT_ID(N'dbo.LibroCategoria', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LibroCategoria
    (
        IdLibro INT NOT NULL CONSTRAINT PK_LibroCategoria PRIMARY KEY,
        IdCategoria INT NOT NULL,
        CONSTRAINT FK_LibroCategoria_Libros
            FOREIGN KEY (IdLibro)
            REFERENCES dbo.Libros(Id)
            ON DELETE CASCADE,
        CONSTRAINT FK_LibroCategoria_Categorias
            FOREIGN KEY (IdCategoria)
            REFERENCES dbo.Categorias(Id)
    );
END
GO

INSERT INTO dbo.Categorias (NombreCategoria)
SELECT v.NombreCategoria
FROM
(
    VALUES
        ('Novela'),
        ('Cuento'),
        ('Poesía'),
        ('Ciencia'),
        ('Tecnología'),
        ('Historia'),
        ('Biografía'),
        ('Infantil')
) AS v(NombreCategoria)
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Categorias c
    WHERE c.NombreCategoria = v.NombreCategoria
);
GO

CREATE OR ALTER PROCEDURE dbo.usp_ListarLibros
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, NombreLibro, Autor, NombreEditorial, ISDN
    FROM dbo.Libros
    ORDER BY Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ListarLibrosPaginado
    @Pagina INT,
    @TamanoPagina INT,
    @TotalRegistros INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Pagina < 1 SET @Pagina = 1;
    IF @TamanoPagina < 1 SET @TamanoPagina = 5;

    SELECT @TotalRegistros = COUNT(*)
    FROM dbo.Libros;

    SELECT
        l.Id,
        l.NombreLibro,
        l.Autor,
        l.NombreEditorial,
        l.ISDN,
        c.Id AS IdCategoria,
        c.NombreCategoria
    FROM dbo.Libros l
    LEFT JOIN dbo.LibroCategoria lc ON lc.IdLibro = l.Id
    LEFT JOIN dbo.Categorias c ON c.Id = lc.IdCategoria
    ORDER BY l.Id
    OFFSET (@Pagina - 1) * @TamanoPagina ROWS
    FETCH NEXT @TamanoPagina ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ObtenerLibroPorId
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        l.Id,
        l.NombreLibro,
        l.Autor,
        l.NombreEditorial,
        l.ISDN,
        c.Id AS IdCategoria,
        c.NombreCategoria
    FROM dbo.Libros l
    LEFT JOIN dbo.LibroCategoria lc ON lc.IdLibro = l.Id
    LEFT JOIN dbo.Categorias c ON c.Id = lc.IdCategoria
    WHERE l.Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_RegistrarLibro
    @NombreLibro VARCHAR(120),
    @Autor VARCHAR(100),
    @NombreEditorial VARCHAR(100),
    @ISDN VARCHAR(20),
    @IdCategoria INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    INSERT INTO dbo.Libros
    (
        NombreLibro,
        Autor,
        NombreEditorial,
        ISDN
    )
    VALUES
    (
        @NombreLibro,
        @Autor,
        @NombreEditorial,
        @ISDN
    );

    DECLARE @NuevoId INT;
    SET @NuevoId = CONVERT(INT, SCOPE_IDENTITY());

    INSERT INTO dbo.LibroCategoria (IdLibro, IdCategoria)
    VALUES (@NuevoId, @IdCategoria);

    COMMIT TRANSACTION;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ActualizarLibro
    @Id INT,
    @NombreLibro VARCHAR(120),
    @Autor VARCHAR(100),
    @NombreEditorial VARCHAR(100),
    @ISDN VARCHAR(20),
    @IdCategoria INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    UPDATE dbo.Libros
    SET
        NombreLibro = @NombreLibro,
        Autor = @Autor,
        NombreEditorial = @NombreEditorial,
        ISDN = @ISDN
    WHERE Id = @Id;

    IF @@ROWCOUNT = 0
    BEGIN
        ROLLBACK TRANSACTION;
        RETURN;
    END

    UPDATE dbo.LibroCategoria
    SET IdCategoria = @IdCategoria
    WHERE IdLibro = @Id;

    IF @@ROWCOUNT = 0
    BEGIN
        INSERT INTO dbo.LibroCategoria (IdLibro, IdCategoria)
        VALUES (@Id, @IdCategoria);
    END

    COMMIT TRANSACTION;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_EliminarLibro
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.Libros
    WHERE Id = @Id;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ListarLibrosPorAutor
    @Autor VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, NombreLibro, Autor, NombreEditorial, ISDN
    FROM dbo.Libros
    WHERE Autor LIKE '%' + @Autor + '%'
    ORDER BY Autor, NombreLibro;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ListarLibrosPorAutorPaginado
    @Autor VARCHAR(100),
    @Pagina INT,
    @TamanoPagina INT,
    @TotalRegistros INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF @Pagina < 1 SET @Pagina = 1;
    IF @TamanoPagina < 1 SET @TamanoPagina = 5;

    SELECT @TotalRegistros = COUNT(*)
    FROM dbo.Libros
    WHERE Autor LIKE '%' + @Autor + '%';

    SELECT
        l.Id,
        l.NombreLibro,
        l.Autor,
        l.NombreEditorial,
        l.ISDN,
        c.Id AS IdCategoria,
        c.NombreCategoria
    FROM dbo.Libros l
    LEFT JOIN dbo.LibroCategoria lc ON lc.IdLibro = l.Id
    LEFT JOIN dbo.Categorias c ON c.Id = lc.IdCategoria
    WHERE l.Autor LIKE '%' + @Autor + '%'
    ORDER BY l.Autor, l.NombreLibro, l.Id
    OFFSET (@Pagina - 1) * @TamanoPagina ROWS
    FETCH NEXT @TamanoPagina ROWS ONLY;
END
GO

CREATE OR ALTER PROCEDURE dbo.usp_ListarCategorias
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, NombreCategoria
    FROM dbo.Categorias
    ORDER BY NombreCategoria;
END
GO

DECLARE @Seed TABLE
(
    NombreLibro VARCHAR(120) NOT NULL,
    Autor VARCHAR(100) NOT NULL,
    NombreEditorial VARCHAR(100) NOT NULL,
    ISDN VARCHAR(20) NOT NULL,
    NombreCategoria VARCHAR(80) NOT NULL
);

INSERT INTO @Seed
(
    NombreLibro,
    Autor,
    NombreEditorial,
    ISDN,
    NombreCategoria
)
VALUES
('It', 'Stephen King', 'Viking Press', '9780670813025', 'Novela'),
('El resplandor', 'Stephen King', 'Doubleday', '9780385121675', 'Novela'),
('Frankenstein', 'Mary Shelley', 'Penguin Classics', '9780141439471', 'Novela'),
('Drácula', 'Bram Stoker', 'Penguin Classics', '9780141439846', 'Novela'),
('El exorcista', 'William Peter Blatty', 'HarperCollins', '9780062094360', 'Novela'),
('Jurassic Park', 'Michael Crichton', 'Alfred A. Knopf', '9780394588162', 'Ciencia'),
('Tiburón', 'Peter Benchley', 'Doubleday', '9780385047715', 'Novela'),
('El padrino', 'Mario Puzo', 'G. P. Putnam''s Sons', '9780399103421', 'Novela'),
('Dune', 'Frank Herbert', 'Chilton Books', '9780801950773', 'Ciencia'),
('Harry Potter y la piedra filosofal', 'J. K. Rowling', 'Salamandra', '9788478884452', 'Infantil'),
('Sapiens: De animales a dioses', 'Yuval Noah Harari', 'Debate', '9788499924212', 'Historia'),
('Steve Jobs', 'Walter Isaacson', 'Simon & Schuster', '9781451648539', 'Biografía'),
('El silencio de los corderos', 'Thomas Harris', 'St. Martin''s Press', '9780312022822', 'Novela'),
('La guerra de los mundos', 'H. G. Wells', 'William Heinemann', 'S/N-HEINEMANN-1898', 'Ciencia'),
('El diario de Ana Frank', 'Ana Frank', 'Contact Publishing', 'S/N-CONTACT-1947', 'Historia'),
('El código Da Vinci', 'Dan Brown', 'Doubleday', '9780385504201', 'Novela');

INSERT INTO dbo.Libros
(
    NombreLibro,
    Autor,
    NombreEditorial,
    ISDN
)
SELECT
    s.NombreLibro,
    s.Autor,
    s.NombreEditorial,
    s.ISDN
FROM @Seed s
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.Libros l
    WHERE l.NombreLibro = s.NombreLibro
      AND l.Autor = s.Autor
);

UPDATE lc
SET lc.IdCategoria = c.Id
FROM dbo.LibroCategoria lc
INNER JOIN dbo.Libros l ON l.Id = lc.IdLibro
INNER JOIN @Seed s
    ON s.NombreLibro = l.NombreLibro
    AND s.Autor = l.Autor
INNER JOIN dbo.Categorias c
    ON c.NombreCategoria = s.NombreCategoria;

INSERT INTO dbo.LibroCategoria (IdLibro, IdCategoria)
SELECT l.Id, c.Id
FROM dbo.Libros l
INNER JOIN @Seed s
    ON s.NombreLibro = l.NombreLibro
    AND s.Autor = l.Autor
INNER JOIN dbo.Categorias c
    ON c.NombreCategoria = s.NombreCategoria
WHERE NOT EXISTS
(
    SELECT 1
    FROM dbo.LibroCategoria lc
    WHERE lc.IdLibro = l.Id
);
GO

SELECT
    l.Id,
    l.NombreLibro,
    l.Autor,
    l.NombreEditorial,
    l.ISDN,
    c.NombreCategoria
FROM dbo.Libros l
LEFT JOIN dbo.LibroCategoria lc ON lc.IdLibro = l.Id
LEFT JOIN dbo.Categorias c ON c.Id = lc.IdCategoria
ORDER BY l.Id;
GO

SELECT COUNT(*) AS TotalLibros
FROM dbo.Libros;
GO

SELECT COUNT(*) AS TotalRelacionesLibroCategoria
FROM dbo.LibroCategoria;
GO

SELECT
    c.NombreCategoria,
    COUNT(lc.IdLibro) AS Cantidad
FROM dbo.Categorias c
LEFT JOIN dbo.LibroCategoria lc ON lc.IdCategoria = c.Id
GROUP BY c.NombreCategoria
ORDER BY c.NombreCategoria;
GO

SELECT
    p.name AS Procedimiento,
    'EXISTE' AS Estado
FROM sys.procedures p
WHERE p.name IN
(
    'usp_ListarLibros',
    'usp_ListarLibrosPaginado',
    'usp_ObtenerLibroPorId',
    'usp_RegistrarLibro',
    'usp_ActualizarLibro',
    'usp_EliminarLibro',
    'usp_ListarLibrosPorAutor',
    'usp_ListarLibrosPorAutorPaginado',
    'usp_ListarCategorias'
)
ORDER BY p.name;
GO

DECLARE @Total INT;

EXEC dbo.usp_ListarLibrosPaginado
    @Pagina = 1,
    @TamanoPagina = 5,
    @TotalRegistros = @Total OUTPUT;

SELECT @Total AS TotalRegistrosPaginados;
GO

EXEC dbo.usp_ListarLibrosPorAutor
    @Autor = 'Stephen';
GO

DECLARE @TotalAutor INT;

EXEC dbo.usp_ListarLibrosPorAutorPaginado
    @Autor = 'Stephen',
    @Pagina = 1,
    @TamanoPagina = 5,
    @TotalRegistros = @TotalAutor OUTPUT;

SELECT @TotalAutor AS TotalRegistrosPorAutor;
GO

EXEC dbo.usp_ListarCategorias;
GO