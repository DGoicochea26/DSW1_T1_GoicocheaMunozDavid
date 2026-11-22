using DSW1_T1_GoicocheaMunozDavidZlatan.Models;

namespace DSW1_T1_GoicocheaMunozDavidZlatan.Interfaces
{
    public interface ILibroService
    {
        IEnumerable<Libro> Listar();
        ResultadoPaginado<Libro> ListarPaginado(int pagina, int tamanoPagina);
        Libro? ObtenerPorId(int id);
        bool Registrar(Libro libro);
        bool Actualizar(Libro libro);
        bool Eliminar(int id);
        IEnumerable<Libro> ListarLibrosPorAutor(string autor);
        ResultadoPaginado<Libro> ListarLibrosPorAutorPaginado(string autor, int pagina, int tamanoPagina);
        IEnumerable<Categoria> ListarCategorias();
    }
}
