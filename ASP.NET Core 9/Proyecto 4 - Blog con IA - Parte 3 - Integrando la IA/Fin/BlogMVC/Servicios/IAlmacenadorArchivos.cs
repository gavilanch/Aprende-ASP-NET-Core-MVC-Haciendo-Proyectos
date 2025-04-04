namespace BlogMVC.Servicios
{
    public interface IAlmacenadorArchivos
    {
        Task<string> Almacenar(string contenedor, IFormFile archivo);
        Task Borrar(string? ruta, string contenedor);
        async Task<string> Editar(string? ruta, string contenedor, IFormFile archivo)
        {
            await Borrar(ruta, contenedor);
            return await Almacenar(contenedor, archivo);
        }

    }
}
