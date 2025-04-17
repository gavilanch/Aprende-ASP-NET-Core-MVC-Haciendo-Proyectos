
namespace BlogMVC.Servicios
{
    public interface IServicioImagenes
    {
        Task<byte[]> GenerarPortadaEntrada(string titulo);
    }
}