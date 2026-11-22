namespace DSW1_T1_GoicocheaMunozDavidZlatan.Models
{
    public class ResultadoPaginado<T>
    {
        public IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();
        public int PaginaActual { get; init; } = 1;
        public int TamanoPagina { get; init; } = 5;
        public int TotalRegistros { get; init; }
        public int TotalPaginas => TotalRegistros == 0 ? 0 : (int)Math.Ceiling(TotalRegistros / (double)TamanoPagina);
    }
}
