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
        private readonly string contenedor = "entradas";

        public EntradasController(ApplicationDbContext context, 
            IAlmacenadorArchivos almacenadorArchivos,
    IServicioUsuarios servicioUsuarios)
        {
            this.context = context;
            this.almacenadorArchivos = almacenadorArchivos;
            this.servicioUsuarios = servicioUsuarios;
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
            } else if (modelo.ImagenRemovida)
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
    }
}
