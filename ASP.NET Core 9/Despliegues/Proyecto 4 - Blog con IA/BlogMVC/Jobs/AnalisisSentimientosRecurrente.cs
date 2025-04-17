
using BlogMVC.Servicios;

namespace BlogMVC.Jobs
{
    public class AnalisisSentimientosRecurrente : BackgroundService
    {
        private readonly IServiceProvider serviceProvider;

        public AnalisisSentimientosRecurrente(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    Console.WriteLine("Iniciando análisis de sentimientos de comentarios");
                    var analisisSentimientos = scope.ServiceProvider
                                            .GetRequiredService<IAnalisisSentimientos>();
                    await analisisSentimientos.AnalizarComentariosPendientes();
                    await analisisSentimientos.ProcesarLotesPendientes();
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
