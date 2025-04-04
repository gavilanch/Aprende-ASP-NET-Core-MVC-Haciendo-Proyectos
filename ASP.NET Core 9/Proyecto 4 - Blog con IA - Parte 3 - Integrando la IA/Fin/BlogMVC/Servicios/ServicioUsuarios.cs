using BlogMVC.Entidades;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BlogMVC.Servicios
{
    public interface IServicioUsuarios
    {
        string? ObtenerUsuarioId();
        Task<bool> PuedeUsuarioBorrarComentarios();
        Task<bool> PuedeUsuarioHacerCRUDEntradas();
    }

    public class ServicioUsuarios : IServicioUsuarios
    {
        private readonly UserManager<Usuario> userManager;
        private readonly HttpContext httpContext;
        private readonly Usuario usuarioActual;
        private static readonly string[] RolesCRUDEntradas =
            { Constantes.RolAdmin, Constantes.CRUDEntradas };

        private static readonly string[] RolesBorrarComentarios =
            { Constantes.RolAdmin, Constantes.BorraComentarios };

        public ServicioUsuarios(IHttpContextAccessor httpContextAccessor,
            UserManager<Usuario> userManager)
        {
            this.userManager = userManager;
            httpContext = httpContextAccessor.HttpContext!;
            usuarioActual = new Usuario { Id = ObtenerUsuarioId()! };
        }

        public async Task<bool> PuedeUsuarioHacerCRUDEntradas()
        {
            return await UsuarioEstaEnRol(RolesCRUDEntradas);
        }

        public async Task<bool> PuedeUsuarioBorrarComentarios()
        {
            return await UsuarioEstaEnRol(RolesBorrarComentarios);
        }


        private async Task<bool> UsuarioEstaEnRol(IEnumerable<string> roles)
        {
            var rolesUsuario = await userManager.GetRolesAsync(usuarioActual);
            return roles.Any(rolesUsuario.Contains);
        }

        public string? ObtenerUsuarioId()
        {
            var idClaim = httpContext.User.Claims
                .Where(x => x.Type == ClaimTypes.NameIdentifier).FirstOrDefault();

            if (idClaim is null)
            {
                return null;
            }

            return idClaim.Value;
        }
    }
}
