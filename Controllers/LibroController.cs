using DSW1_T1_GoicocheaMunozDavidZlatan.Interfaces;
using DSW1_T1_GoicocheaMunozDavidZlatan.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DSW1_T1_GoicocheaMunozDavidZlatan.Controllers
{
    public class LibroController : Controller
    {
        private const int TamanoPagina = 5;

        private readonly ILibroService _service;

        public LibroController(ILibroService service)
        {
            _service = service;
        }

        [NonAction]
        public IEnumerable<Libro> ListarLibros() => _service.Listar();

        [NonAction]
        public IEnumerable<Libro> ListarLibrosPorAutor(string autor) => _service.ListarLibrosPorAutor(autor);

        [HttpGet]
        public IActionResult Index(int pagina = 1)
        {
            ViewBag.Mensaje = TempData["Mensaje"];
            ViewBag.Error = TempData["Error"];
            try
            {
                return View(_service.ListarPaginado(pagina, TamanoPagina));
            }
            catch (Exception)
            {
                ViewBag.Error = "No se pudo conectar con la base de datos. Verifique la cadena de conexión.";
                return View(new ResultadoPaginado<Libro>());
            }
        }

        [HttpGet]
        public IActionResult Crear()
        {
            var libro = new Libro();
            return CargarCategorias(libro.IdCategoria) ? View(libro) : RedirigirPorErrorDeCategorias();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Libro libro)
        {
            if (!ModelState.IsValid)
                return CargarCategorias(libro.IdCategoria) ? View(libro) : RedirigirPorErrorDeCategorias();

            try
            {
                if (_service.Registrar(libro))
                    TempData["Mensaje"] = "El libro se registró correctamente.";
                else
                    TempData["Error"] = "No se pudo registrar el libro.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Ocurrió un error al registrar el libro.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            try
            {
                var libro = _service.ObtenerPorId(id);
                if (libro == null)
                {
                    TempData["Error"] = "El libro solicitado no existe.";
                    return RedirectToAction(nameof(Index));
                }
                return CargarCategorias(libro.IdCategoria) ? View(libro) : RedirigirPorErrorDeCategorias();
            }
            catch (Exception)
            {
                TempData["Error"] = "Ocurrió un error al recuperar el libro.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(Libro libro)
        {
            if (!ModelState.IsValid)
                return CargarCategorias(libro.IdCategoria) ? View(libro) : RedirigirPorErrorDeCategorias();

            try
            {
                if (_service.Actualizar(libro))
                    TempData["Mensaje"] = "El libro se actualizó correctamente.";
                else
                    TempData["Error"] = "No se pudo actualizar el libro.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Ocurrió un error al actualizar el libro.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id, int pagina = 1)
        {
            try
            {
                if (_service.Eliminar(id))
                    TempData["Mensaje"] = "El libro se eliminó correctamente.";
                else
                    TempData["Error"] = "No se pudo eliminar el libro.";
            }
            catch (Exception)
            {
                TempData["Error"] = "Ocurrió un error al eliminar el libro.";
            }
            return RedirectToAction(nameof(Index), new { pagina });
        }

        [HttpGet]
        public IActionResult BuscarPorAutor(string? autor, int pagina = 1)
        {
            ViewBag.Autor = autor;
            bool buscado = !string.IsNullOrWhiteSpace(autor);
            ViewBag.Buscado = buscado;

            if (!buscado) return View(new ResultadoPaginado<Libro>());

            try
            {
                var resultados = _service.ListarLibrosPorAutorPaginado(autor!, pagina, TamanoPagina);
                ViewBag.Mensaje = resultados.TotalRegistros == 0
                    ? $"No se encontraron libros del autor \"{autor!.Trim()}\"."
                    : $"Se encontraron {resultados.TotalRegistros} libro(s).";
                return View(resultados);
            }
            catch (Exception)
            {
                ViewBag.Error = "Ocurrió un error al realizar la búsqueda.";
                return View(new ResultadoPaginado<Libro>());
            }
        }

        private bool CargarCategorias(int? seleccionada)
        {
            try
            {
                ViewBag.Categorias = new SelectList(_service.ListarCategorias(), nameof(Categoria.Id), nameof(Categoria.NombreCategoria), seleccionada);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private IActionResult RedirigirPorErrorDeCategorias()
        {
            TempData["Error"] = "No se pudieron cargar las categorías.";
            return RedirectToAction(nameof(Index));
        }
    }
}
