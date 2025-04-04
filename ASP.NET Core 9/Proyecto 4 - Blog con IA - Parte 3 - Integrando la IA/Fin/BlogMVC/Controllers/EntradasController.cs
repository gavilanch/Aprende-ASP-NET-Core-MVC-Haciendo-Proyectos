using BlogMVC.Datos;
using BlogMVC.Entidades;
using BlogMVC.Models;
using BlogMVC.Servicios;
using BlogMVC.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogMVC.Controllers
{
    public class EntradasController: Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IAlmacenadorArchivos almacenadorArchivos;
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly IServicioChat servicioChat;
        private readonly IWebHostEnvironment env;
        private readonly IServicioImagenes servicioImagenes;
        private readonly string contenedor = "entradas";

        public EntradasController(ApplicationDbContext context, 
            IAlmacenadorArchivos almacenadorArchivos,
    IServicioUsuarios servicioUsuarios, IServicioChat servicioChat, IWebHostEnvironment env,
    IServicioImagenes servicioImagenes)
        {
            this.context = context;
            this.almacenadorArchivos = almacenadorArchivos;
            this.servicioUsuarios = servicioUsuarios;
            this.servicioChat = servicioChat;
            this.env = env;
            this.servicioImagenes = servicioImagenes;
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var entrada = await context.Entradas
                .IgnoreQueryFilters()
                .Include(x => x.UsuarioCreacion)
                .Include(x => x.Comentarios)
                    .ThenInclude(x => x.Usuario)
                .FirstOrDefaultAsync(x => x.Id == id);
            
            if (entrada is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var puedeEditarEntradas = await servicioUsuarios.PuedeUsuarioHacerCRUDEntradas();

            if (entrada.Borrado && !puedeEditarEntradas)
            {
                var urlRetorno = HttpContext.ObtenerUrlRetorno();
                return RedirectToAction("Login", "Usuarios", new { urlRetorno });
            }

            var puedeBorrarComentarios = await servicioUsuarios.PuedeUsuarioBorrarComentarios();

            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            var modelo = new EntradaDetalleViewModel
            {
                Id = entrada.Id,
                Titulo = entrada.Titulo,
                Cuerpo = entrada.Cuerpo,
                PortadaUrl = entrada.PortadaUrl,
                FechaPublicacion = entrada.FechaPublicacion,
                EscritoPor = entrada.UsuarioCreacion!.Nombre,
                MostrarBotonEdicion = puedeEditarEntradas,
                EntradaBorrada = entrada.Borrado,
                Comentarios = entrada.Comentarios.Select(x => new ComentarioViewModel
                {
                    Id = x.Id,
                    Cuerpo = x.Cuerpo,
                    EscritoPor = x.Usuario!.Nombre,
                    FechaPublicacion = x.FechaPublicacion,
                    MostrarBotonBorrar = puedeBorrarComentarios || usuarioId == x.UsuarioId
                })
            };

            return View(modelo);

        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = $"{Constantes.RolAdmin},{Constantes.CRUDEntradas}")]
        public async Task<IActionResult> Crear(EntradaCrearViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            string? portadaUrl = null;

            if (modelo.ImagenPortada is not null)
            {
                portadaUrl = await almacenadorArchivos.Almacenar(contenedor, modelo.ImagenPortada);
            } else if(modelo.ImagenPortadaIA is not null)
            {
                var archivo = Base64AIFormFile(modelo.ImagenPortadaIA);
                portadaUrl = await almacenadorArchivos.Almacenar(contenedor, archivo);
            }

                string usuarioId = servicioUsuarios.ObtenerUsuarioId()!;

            var entrada = new Entrada
            {
                Titulo = modelo.Titulo,
                Cuerpo = modelo.Cuerpo,
                FechaPublicacion = DateTime.UtcNow,
                PortadaUrl = portadaUrl,
                UsuarioCreacionId = usuarioId
            };

            context.Add(entrada);
            await context.SaveChangesAsync();

            return RedirectToAction("Detalle", new { id = entrada.Id });
        }

        [HttpGet]
        [Authorize(Roles = $"{Constantes.RolAdmin},{Constantes.CRUDEntradas}")]
        public async Task<IActionResult> Editar(int id)
        {
            var entrada = await context.Entradas
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entrada is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var modelo = new EntradaEditarViewModel
            {
                Id = entrada.Id,
                Titulo = entrada.Titulo,
                Cuerpo = entrada.Cuerpo,
                ImagenPortadaActual = entrada.PortadaUrl
            };

            return View(modelo);
        }

        [HttpPost]
        [Authorize(Roles = $"{Constantes.RolAdmin},{Constantes.CRUDEntradas}")]
        public async Task<IActionResult> Editar(EntradaEditarViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var entradaDB = await context.Entradas
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == modelo.Id);

            if (entradaDB is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            string? portadaUrl = null;

            if (modelo.ImagenPortada is not null)
            {
                portadaUrl = await almacenadorArchivos.Editar(modelo.ImagenPortadaActual,
                    contenedor, modelo.ImagenPortada);
            }
            else if(modelo.ImagenPortadaIA is not null)
            {
                var archivo = Base64AIFormFile(modelo.ImagenPortadaIA);
                portadaUrl = await almacenadorArchivos.Editar(modelo.ImagenPortadaActual,
                    contenedor, archivo);
            }
            else if (modelo.ImagenRemovida)
            {
                await almacenadorArchivos.Borrar(modelo.ImagenPortadaActual, contenedor);
            }
            else
            {
                portadaUrl = entradaDB.PortadaUrl;
            }

            string usuarioId = servicioUsuarios.ObtenerUsuarioId()!;

            entradaDB.Titulo = modelo.Titulo;
            entradaDB.Cuerpo = modelo.Cuerpo;
            entradaDB.PortadaUrl = portadaUrl;
            entradaDB.UsuarioActualizacionId = usuarioId;

            await context.SaveChangesAsync();

            return RedirectToAction("Detalle", new { id = entradaDB.Id });
        }

        [HttpPost]
        [Authorize(Roles = $"{Constantes.RolAdmin},{Constantes.CRUDEntradas}")]
        public async Task<IActionResult> Borrar(int id, bool borrado)
        {
            var entradaDB = await context.Entradas
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entradaDB is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            entradaDB.Borrado = borrado;
            await context.SaveChangesAsync();
            return RedirectToAction("Detalle", new { id = entradaDB.Id });
        }

        [HttpGet]
        public async Task GenerarCuerpo([FromQuery] string titulo)
        {
            if (string.IsNullOrWhiteSpace(titulo))
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                await Response.WriteAsync("El título no puede estar vacío.");
                return;
            }

            await foreach (var segmento in servicioChat.GenerarCuerpoStream(titulo))
            {
                await Response.WriteAsync(segmento);
                await Response.Body.FlushAsync();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GenerarImagen([FromQuery] string titulo)
        {
            if (string.IsNullOrWhiteSpace(titulo))
            {
                return BadRequest("El título no puede estar vacío.");
            }

            //if (env.IsDevelopment())
            //{
            //    var rutaImagen = Path.Combine(env.WebRootPath, "img", "ia.png");
            //    var imagenBytes = await System.IO.File.ReadAllBytesAsync(rutaImagen);
            //    await Task.Delay(1000);
            //    return File(imagenBytes, "image/png");
            //}

            var bytes = await servicioImagenes.GenerarPortadaEntrada(titulo);

            return File(bytes, "image/png");

        }

        [HttpGet]
        public async Task<ActionResult> ConComentariosNegativos()
        {
            var entradasConComentariosNegativos = await context.Entradas
                .Where(x => x.Comentarios.Where(x => x.Puntuacion == 2 || x.Puntuacion == 1).Any())
                .OrderByDescending(x => x.Comentarios.Where(x => x.Puntuacion == 2 || x.Puntuacion == 1).Count())
                .Select(x => new EntradaConComentariosNegativosViewModel
                {
                    Id = x.Id,
                    Titulo = x.Titulo,
                    CantidadComentariosNegativos =
                            x.Comentarios.Where(x => x.Puntuacion == 2 || x.Puntuacion == 1).Count()
                }).ToListAsync();

            var modelo = new EntradasConComentariosNegativosViewModel();
            modelo.Entradas = entradasConComentariosNegativos;
            return View(modelo);
        }

        private IFormFile Base64AIFormFile(string base64)
        {
            byte[] bytes = Convert.FromBase64String(base64);
            var stream = new MemoryStream(bytes);
            IFormFile archivo = new FormFile(stream, 0, bytes.Length, "imagen", "imagen.png");
            return archivo;
        }
    }
}
