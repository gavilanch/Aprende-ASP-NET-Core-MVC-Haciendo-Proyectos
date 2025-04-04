using BlogMVC.Configuraciones;
using BlogMVC.Datos;
using BlogMVC.Entidades;
using BlogMVC.Jobs;
using BlogMVC.Servicios;
using BlogMVC.Utilidades;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<ConfiguracionesIA>()
    .Bind(builder.Configuration.GetSection(ConfiguracionesIA.Seccion))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddScoped(sp =>
{
    var configuracionesIA = sp.GetRequiredService<IOptions<ConfiguracionesIA>>();
    return new OpenAIClient(configuracionesIA.Value.LlaveOpenAI);
});

builder.Services.AddServerSideBlazor();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddTransient<IAlmacenadorArchivos, AlmacenadorArchivosLocal>();
builder.Services.AddTransient<IServicioUsuarios, ServicioUsuarios>();
builder.Services.AddTransient<IServicioChat, ServicioChatOpenAI>();
builder.Services.AddTransient<IServicioImagenes, ServicioImagenesOpenAI>();
builder.Services.AddScoped<IAnalisisSentimientos, AnalisisSentimientosOpenAI>();

builder.Services.AddHttpClient();

builder.Services.AddHostedService<AnalisisSentimientosRecurrente>();

builder.Services.AddDbContextFactory<ApplicationDbContext>(opciones =>
opciones.UseSqlServer("name=DefaultConnection")
.UseSeeding(Seeding.Aplicar)
.UseAsyncSeeding(Seeding.AplicarAsync));

builder.Services.AddIdentity<Usuario, IdentityRole>(opciones =>
{
    opciones.SignIn.RequireConfirmedAccount = false;
}).AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.PostConfigure<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme,
    opciones =>
    {
        opciones.LoginPath = "/usuarios/login";
        opciones.AccessDeniedPath = "/usuarios/login";
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapBlazorHub();

app.Run();
