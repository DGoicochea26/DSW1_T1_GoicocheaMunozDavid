using DSW1_T1_GoicocheaMunozDavidZlatan.Interfaces;
using DSW1_T1_GoicocheaMunozDavidZlatan.Models;

namespace DSW1_T1_GoicocheaMunozDavidZlatan.Services
{
    public class LibroService : ILibroService
    {
        private readonly ILibroRepository _repository;

        public LibroService(ILibroRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<Libro> Listar() => _repository.Listar();

        public ResultadoPaginado<Libro> ListarPaginado(int pagina, int tamanoPagina) =>
            Consultar(p => _repository.ListarPaginado(p, tamanoPagina), pagina);

        public Libro? ObtenerPorId(int id)
        {
            if (id <= 0) return null;
            return _repository.ObtenerPorId(id);
        }

        public bool Registrar(Libro libro)
        {
            if (!EsValido(libro)) return false;
            Normalizar(libro);
            return _repository.Registrar(libro);
        }

        public bool Actualizar(Libro libro)
        {
            if (libro.Id <= 0 || !EsValido(libro)) return false;
            Normalizar(libro);
            return _repository.Actualizar(libro);
        }

        public bool Eliminar(int id)
        {
            if (id <= 0) return false;
            return _repository.Eliminar(id);
        }

        public IEnumerable<Libro> ListarLibrosPorAutor(string autor)
        {
            if (string.IsNullOrWhiteSpace(autor)) return Enumerable.Empty<Libro>();
            return _repository.ListarLibrosPorAutor(autor.Trim());
        }

        public ResultadoPaginado<Libro> ListarLibrosPorAutorPaginado(string autor, int pagina, int tamanoPagina)
        {
            if (string.IsNullOrWhiteSpace(autor)) return new ResultadoPaginado<Libro>();
            var termino = autor.Trim();
            return Consultar(p => _repository.ListarLibrosPorAutorPaginado(termino, p, tamanoPagina), pagina);
        }

        public IEnumerable<Categoria> ListarCategorias() => _repository.ListarCategorias();

        private static ResultadoPaginado<Libro> Consultar(Func<int, ResultadoPaginado<Libro>> consulta, int pagina)
        {
            var resultado = consulta(Math.Max(1, pagina));
            if (resultado.TotalPaginas > 0 && resultado.PaginaActual > resultado.TotalPaginas)
                resultado = consulta(resultado.TotalPaginas);
            return resultado;
        }

        private static bool EsValido(Libro l) =>
            !string.IsNullOrWhiteSpace(l.NombreLibro) &&
            !string.IsNullOrWhiteSpace(l.Autor) &&
            !string.IsNullOrWhiteSpace(l.NombreEditorial) &&
            !string.IsNullOrWhiteSpace(l.ISDN) &&
            l.IdCategoria is > 0;

        private static void Normalizar(Libro l)
        {
            l.NombreLibro = l.NombreLibro.Trim();
            l.Autor = l.Autor.Trim();
            l.NombreEditorial = l.NombreEditorial.Trim();
            l.ISDN = l.ISDN.Trim();
        }
    }
}
