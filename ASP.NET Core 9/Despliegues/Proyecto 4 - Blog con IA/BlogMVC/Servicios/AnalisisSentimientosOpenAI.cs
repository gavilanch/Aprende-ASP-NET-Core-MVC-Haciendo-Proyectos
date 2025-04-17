using BlogMVC.Configuraciones;
using BlogMVC.Datos;
using BlogMVC.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BlogMVC.Servicios
{
    public class AnalisisSentimientosOpenAI : IAnalisisSentimientos
    {
        private readonly ApplicationDbContext context;
        private readonly IOptions<ConfiguracionesIA> options;
        private readonly HttpClient httpClient;

        public AnalisisSentimientosOpenAI(ApplicationDbContext context,
            IOptions<ConfiguracionesIA> options, HttpClient httpClient)
        {
            this.context = context;
            this.options = options;
            httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", options.Value.LlaveOpenAI);
            this.httpClient = httpClient;
        }

        public async Task AnalizarComentariosPendientes()
        {
            var comentariosPendientesDeAnalisis = await context.Comentarios
                .Where(x => x.Puntuacion == null)
                .Take(1_000)
                .Select(x => new Comentario { Id = x.Id, Cuerpo = x.Cuerpo })
                .ToListAsync();

            if (!comentariosPendientesDeAnalisis.Any())
            {
                return;
            }

            string jsonContent = ArmarPeticiones(comentariosPendientesDeAnalisis);
            var inputFileId = await SubirArchivoPeticiones(jsonContent);
            var batchId = await EnviarBatch(inputFileId);
            await GuardarLoteYActualizarComentarios(comentariosPendientesDeAnalisis, batchId);
        }

        private string ArmarPeticiones(List<Comentario> comentariosPendientesDeAnalisis)
        {
            var configuracionesIA = options.Value;

            var requests = new StringBuilder();
            foreach (var comentario in comentariosPendientesDeAnalisis)
            {
                var request = new
                {
                    custom_id = comentario.Id.ToString(),
                    method = "POST",
                    url = "/v1/chat/completions",
                    body = new
                    {
                        model = configuracionesIA.ModeloSentimientos,
                        messages = new[]
                        {
                             new { role = "system", content = "Eres un analista de sentimientos. Devuelve una puntuación de 1 a 5 según la emoción expresada en el mensaje. Solo debes retornar la puntuación y nada más." },
                             new { role = "user", content = comentario.Cuerpo }
                        }
                    }
                };

                var requestJSON = JsonSerializer.Serialize(request);
                requests.AppendLine(requestJSON);
            }

            var jsonContent = requests.ToString();
            return jsonContent;
        }

        private async Task<string> SubirArchivoPeticiones(string jsonContent)
        {
            var configuracionesIA = options.Value;

            using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));
            using var fileContent = new StreamContent(memoryStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using var form = new MultipartFormDataContent();
            form.Add(fileContent, "file", "batch_comentarios.jsonl");
            form.Add(new StringContent("batch"), "purpose");

            HttpResponseMessage respuestaSubidaArchivo =
                await httpClient.PostAsync("https://api.openai.com/v1/files", form);

            string responseContent = await respuestaSubidaArchivo.Content.ReadAsStringAsync();

            var inputFileId = JsonDocument.Parse(responseContent)
                .RootElement.GetProperty("id").GetString();

            return inputFileId!;
        }

        private async Task<string> EnviarBatch(string inputFileId)
        {
            var configuracionesIA = options.Value;

            var batchRequest = new
            {
                input_file_id = inputFileId,
                endpoint = "/v1/chat/completions",
                completion_window = "24h"
            };

            var jsonRequest = JsonSerializer.Serialize(batchRequest);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://api.openai.com/v1/batches", content);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            var batchId = JsonDocument.Parse(jsonResponse).RootElement.GetProperty("id").GetString();
            return batchId!;

        }

        private async Task GuardarLoteYActualizarComentarios
            (List<Comentario> comentariosPendientesDeAnalisis, string batchId)
        {
            var lote = new Lote
            {
                Id = batchId,
                Estatus = "in_progress",
                FechaCreacion = DateTime.UtcNow
            };

            context.Add(lote);
            await context.SaveChangesAsync();

            var idsComentarios = comentariosPendientesDeAnalisis.Select(x => x.Id);

            await context.Comentarios
                        .Where(x => idsComentarios.Contains(x.Id))
                        .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.Puntuacion, -1));

        }

        public async Task ProcesarLotesPendientes()
        {
            var lotesSinProcesar = await context.Lotes.Where(x => x.Estatus != "failed" &&
            x.Estatus != "completed" && x.Estatus != "expired").ToListAsync();

            if (!lotesSinProcesar.Any())
            {
                return;
            }

            var configuracionesIA = options.Value;

            foreach (var lote in lotesSinProcesar)
            {
                var response = await httpClient.GetAsync($"https://api.openai.com/v1/batches/{lote.Id}");

                var respuestaString = await response.Content.ReadAsStringAsync();

                var respuestaJSON = JsonDocument.Parse(respuestaString).RootElement;

                var status = respuestaJSON.GetProperty("status").GetString();

                lote.Estatus = status!;

                if (status == "completed")
                {
                    var outputFileId = respuestaJSON.GetProperty("output_file_id").GetString();

                    var respuestaContenidoArchivo = await 
                        httpClient.GetAsync($"https://api.openai.com/v1/files/{outputFileId}/content");

                    var respuestaContenidoArchivoString = await 
                        respuestaContenidoArchivo.Content.ReadAsStringAsync();

                    var lineas = respuestaContenidoArchivoString
                                .Split("\n", StringSplitOptions.RemoveEmptyEntries);

                    foreach (var linea in lineas)
                    {
                        var json = JsonDocument.Parse(linea).RootElement;
                        var comentarioId = int.Parse(json.GetProperty("custom_id").GetString()!);
                        var puntuacion = 
                            int.Parse(json.GetProperty("response")
                            .GetProperty("body").GetProperty("choices")
                            .EnumerateArray().First().GetProperty("message")
                            .GetProperty("content").GetString()!);

                        var comentario = new Comentario { Id = comentarioId };
                        context.Attach(comentario);
                        comentario.Puntuacion = puntuacion;
                    }
                }
            }

            await context.SaveChangesAsync();
        }

    }
}
