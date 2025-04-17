using BlogMVC.Configuraciones;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace BlogMVC.Servicios
{
    public class ServicioChatOpenAI : IServicioChat
    {
        private readonly IOptions<ConfiguracionesIA> options;
        private readonly OpenAIClient openAIClient;

        private string systemPromptGenerarCuerpo = """
            Eres un ingeniero de software experto en ASP.NET Core. 
            Escribes artículos con un tono jovial y amigable. 
            Te esfuerzas para que los principiantes entiendan las cosas dando ejemplos prácticos.
            """;

        private string ObtenerPromptGeneraCuerpo(string titulo) => $"""

            Crear un artículo para un blog. El título del artículo será {titulo}.

            Si lo entiendes conveniente, debes insertar tips.

            El formato de respuesta es HTML. Por tanto, debes colocar negritas donde consideres, 
            títulos, subtítulos, entre otras cosas que ayuden a resaltar el formato.

            La respuesta no debe ser un documento HTML, sino solamente el artículo en formato HTML, 
            con sus párrafos bien separados. Por tanto, nada de DOCTYPE, ni head, ni body. Solo el artículo.

            No incluyas el título del artículo en el artículo.

            """;


        public ServicioChatOpenAI(IOptions<ConfiguracionesIA> options, OpenAIClient openAIClient)
        {
            this.options = options;
            this.openAIClient = openAIClient;
        }

        public async Task<string> GenerarCuerpo(string titulo)
        {
            var modeloTexto = options.Value.ModeloTexto;
            var clienteChat = openAIClient.GetChatClient(modeloTexto);

            var mensajeDeSistema = new SystemChatMessage(systemPromptGenerarCuerpo);

            var promptUsuario = ObtenerPromptGeneraCuerpo(titulo);

            var mensajeUsuario = new UserChatMessage(promptUsuario);

            ChatMessage[] mensajes = { mensajeDeSistema, mensajeUsuario };
            var respuesta = await clienteChat.CompleteChatAsync(mensajes);
            var cuerpo = respuesta.Value.Content[0].Text;
            return cuerpo;
        }

        public async IAsyncEnumerable<string> GenerarCuerpoStream(string titulo)
        {
            var modeloTexto = options.Value.ModeloTexto;
            var clienteChat = openAIClient.GetChatClient(modeloTexto);

            var mensajeDeSistema = new SystemChatMessage(systemPromptGenerarCuerpo);

            var promptUsuario = ObtenerPromptGeneraCuerpo(titulo);

            var mensajeUsuario = new UserChatMessage(promptUsuario);

            ChatMessage[] mensajes = { mensajeDeSistema, mensajeUsuario };

            await foreach (var completionUpdate in clienteChat.CompleteChatStreamingAsync(mensajes))
            {
                foreach (var contenido in completionUpdate.ContentUpdate)
                {
                    yield return contenido.Text;
                }
            }


        }

    }
}
