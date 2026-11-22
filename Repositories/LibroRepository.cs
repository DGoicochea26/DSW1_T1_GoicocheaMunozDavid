using System.Data;
using DSW1_T1_GoicocheaMunozDavidZlatan.Interfaces;
using DSW1_T1_GoicocheaMunozDavidZlatan.Models;
using Microsoft.Data.SqlClient;

namespace DSW1_T1_GoicocheaMunozDavidZlatan.Repositories
{
    public class LibroRepository : ILibroRepository
    {
        private readonly string _cadenaConexion;

        public LibroRepository(IConfiguration configuration)
        {
            _cadenaConexion = configuration.GetConnectionString("BDBIBLIOTECA")
                ?? throw new InvalidOperationException("Falta la cadena de conexión 'BDBIBLIOTECA' en appsettings.json.");
        }

        public IEnumerable<Libro> Listar()
        {
            using var cn = new SqlConnection(_cadenaConexion);
            using var cmd = new SqlCommand("usp_ListarLibros", cn) { CommandType = CommandType.StoredProcedure };
            return LeerLibros(cn, cmd, MapearLibro);
        }

        public ResultadoPaginado<Libro> ListarPaginado(int pagina, int tamanoPagina)
        {
            using var cn = new SqlConnection(_cadenaConexion);
            using var cmd = new SqlCommand("usp_ListarLibrosPaginado", cn) { CommandType = CommandType.StoredProcedure };
            AgregarParametrosPagina(cmd, pagina, tamanoPagina);
            return LeerPagina(cn, cmd, pagina, tamanoPagina);
        }

        public Libro? ObtenerPorId(int id)
        {
            using var cn = new SqlConnection(_cadenaConexion);
            using var cmd = new SqlCommand("usp_ObtenerLibroPorId", cn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
            return LeerLibros(cn, cmd, MapearLibroConCategoria).FirstOrDefault();
        }

        public bool Registrar(Libro libro)
        {
            using var cn = new SqlConnection(_cadenaConexion);
            using var cmd = new SqlCommand("usp_RegistrarLibro", cn) { CommandType = CommandType.StoredProcedure };
            AgregarParametrosLibro(cmd, libro);
            return Ejecutar(cn, cmd) > 0;
        }

        public bool Actualizar(Libro libro)
        {
            using var cn = new SqlConnection(_cadenaConexion);
            using var cmd = new SqlCommand("usp_ActualizarLibro", cn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = libro.Id;
            AgregarParametrosLibro(cmd, libro);
            return Ejecutar(cn, cmd) > 0;
        }

        public bool Eliminar(int id)
        {
            using var cn = new SqlConnection(_cadenaConexion);
            using var cmd = new SqlCommand("usp_EliminarLibro", cn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
            return Ejecutar(cn, cmd) > 0;
        }

        public IEnumerable<Libro> ListarLibrosPorAutor(string autor)
        {
            using var cn = new SqlConnection(_cadenaConexion);
            using var cmd = new SqlCommand("usp_ListarLibrosPorAutor", cn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.Add("@Autor", SqlDbType.VarChar, 100).Value = autor;
            return LeerLibros(cn, cmd, MapearLibro);
        }

        public ResultadoPaginado<Libro> ListarLibrosPorAutorPaginado(string autor, int pagina, int tamanoPagina)
        {
            using var cn = new SqlConnection(_cadenaConexion);
            using var cmd = new SqlCommand("usp_ListarLibrosPorAutorPaginado", cn) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.Add("@Autor", SqlDbType.VarChar, 100).Value = autor;
            AgregarParametrosPagina(cmd, pagina, tamanoPagina);
            return LeerPagina(cn, cmd, pagina, tamanoPagina);
        }

        public IEnumerable<Categoria> ListarCategorias()
        {
            var lista = new List<Categoria>();
            using var cn = new SqlConnection(_cadenaConexion);
            using var cmd = new SqlCommand("usp_ListarCategorias", cn) { CommandType = CommandType.StoredProcedure };
            cn.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                lista.Add(new Categoria
                {
                    Id = dr.GetInt32(dr.GetOrdinal("Id")),
                    NombreCategoria = dr.GetString(dr.GetOrdinal("NombreCategoria"))
                });
            }
            return lista;
        }

        private static void AgregarParametrosLibro(SqlCommand cmd, Libro libro)
        {
            cmd.Parameters.Add("@NombreLibro", SqlDbType.VarChar, 120).Value = libro.NombreLibro;
            cmd.Parameters.Add("@Autor", SqlDbType.VarChar, 100).Value = libro.Autor;
            cmd.Parameters.Add("@NombreEditorial", SqlDbType.VarChar, 100).Value = libro.NombreEditorial;
            cmd.Parameters.Add("@ISDN", SqlDbType.VarChar, 20).Value = libro.ISDN;
            cmd.Parameters.Add("@IdCategoria", SqlDbType.Int).Value = (object?)libro.IdCategoria ?? DBNull.Value;
        }

        private static void AgregarParametrosPagina(SqlCommand cmd, int pagina, int tamanoPagina)
        {
            cmd.Parameters.Add("@Pagina", SqlDbType.Int).Value = pagina;
            cmd.Parameters.Add("@TamanoPagina", SqlDbType.Int).Value = tamanoPagina;
            cmd.Parameters.Add("@TotalRegistros", SqlDbType.Int).Direction = ParameterDirection.Output;
        }

        private static int Ejecutar(SqlConnection cn, SqlCommand cmd)
        {
            cn.Open();
            return cmd.ExecuteNonQuery();
        }

        private static ResultadoPaginado<Libro> LeerPagina(SqlConnection cn, SqlCommand cmd, int pagina, int tamanoPagina)
        {
            var libros = LeerLibros(cn, cmd, MapearLibroConCategoria);
            return new ResultadoPaginado<Libro>
            {
                Items = libros,
                PaginaActual = pagina,
                TamanoPagina = tamanoPagina,
                TotalRegistros = Convert.ToInt32(cmd.Parameters["@TotalRegistros"].Value)
            };
        }

        private static List<Libro> LeerLibros(SqlConnection cn, SqlCommand cmd, Func<SqlDataReader, Libro> mapear)
        {
            var lista = new List<Libro>();
            cn.Open();
            using var dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                lista.Add(mapear(dr));
            }
            return lista;
        }

        private static Libro MapearLibro(SqlDataReader dr) => new Libro
        {
            Id = dr.GetInt32(dr.GetOrdinal("Id")),
            NombreLibro = dr.GetString(dr.GetOrdinal("NombreLibro")),
            Autor = dr.GetString(dr.GetOrdinal("Autor")),
            NombreEditorial = dr.GetString(dr.GetOrdinal("NombreEditorial")),
            ISDN = dr.GetString(dr.GetOrdinal("ISDN"))
        };

        private static Libro MapearLibroConCategoria(SqlDataReader dr)
        {
            var libro = MapearLibro(dr);
            var idCategoria = dr.GetOrdinal("IdCategoria");
            var nombreCategoria = dr.GetOrdinal("NombreCategoria");
            libro.IdCategoria = dr.IsDBNull(idCategoria) ? null : dr.GetInt32(idCategoria);
            libro.NombreCategoria = dr.IsDBNull(nombreCategoria) ? null : dr.GetString(nombreCategoria);
            return libro;
        }
    }
}
