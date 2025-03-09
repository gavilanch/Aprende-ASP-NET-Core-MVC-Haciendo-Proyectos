using TareasMVC.Models;

namespace TareasMVC.Servicios
{
    public interface IAlmacenadorArchivos
    {
        Task Borrar(string ruta, string contenedor);
        Task<AlmacenarArchivoResultado[]> Almacenar(string contenedor, 
            IEnumerable<IFormFile> archivos);
    }
}
