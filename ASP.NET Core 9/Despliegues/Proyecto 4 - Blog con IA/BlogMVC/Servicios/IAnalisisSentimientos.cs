
namespace BlogMVC.Servicios
{
    public interface IAnalisisSentimientos
    {
        Task AnalizarComentariosPendientes();
        Task ProcesarLotesPendientes();
    }
}