using BlogMVC.Servicios;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlogMVC.Utilidades
{
    public static class Seeding
    {
        private static List<string> roles = new List<string>
        {
            Constantes.BorraComentarios,
            Constantes.CRUDEntradas,
            Constantes.RolAdmin
        };

        public static void Aplicar(DbContext context, bool _)
        {
            foreach (var rol in roles)
            {
                var rolDB = context.Set<IdentityRole>().FirstOrDefault(x => x.Name == rol);

                if (rolDB is null)
                {
                    context.Set<IdentityRole>().Add(new IdentityRole
                    {
                        Name = rol,
                        NormalizedName = rol.ToUpper()
                    });
                    context.SaveChanges();
                }
            }
        }

        public static async Task AplicarAsync(DbContext context, bool _,
            CancellationToken cancellationToken)
        {
            foreach (var rol in roles)
            {
                var rolDB = await context.Set<IdentityRole>().FirstOrDefaultAsync(x => x.Name == rol);

                if (rolDB is null)
                {
                    context.Set<IdentityRole>().Add(new IdentityRole
                    {
                        Name = rol,
                        NormalizedName = rol.ToUpper()
                    });
                    await context.SaveChangesAsync(cancellationToken);
                }
            }
        }
    }
}
