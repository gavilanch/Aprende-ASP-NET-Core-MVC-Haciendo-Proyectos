using BlogMVC.Configuraciones;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Images;

namespace BlogMVC.Servicios
{
    public class ServicioImagenesOpenAI : IServicioImagenes
    {
        private readonly IOptions<ConfiguracionesIA> options;
        private readonly OpenAIClient openAIClient;

        public ServicioImagenesOpenAI(IOptions<ConfiguracionesIA> options, OpenAIClient openAIClient)
        {
            this.options = options;
            this.openAIClient = openAIClient;
        }

        public async Task<byte[]> GenerarPortadaEntrada(string titulo)
        {
            string prompt = $"""
        Una imagen foto-realista inspirada en el tema '{titulo}'. 
                
        La escena debe reflejar el concepto central del artículo con una composición atractiva. 
                
        Si el tema trata sobre programación en general, mostrar a un programador 
        concentrado frente a una computadora con código en pantalla. 
                
        Si el tema está relacionado con bases de datos, representar un servidor o una 
        visualización de datos en una pantalla holográfica. 
                
        Si trata sobre inteligencia artificial, mostrar un entorno futurista con redes 
        neuronales digitales. 
                
        La iluminación debe ser natural y realista, con una profundidad de campo bien definida. 
                
        La imagen debe ser moderna y tecnológica, evitando clichés exagerados como fondos 
        de neón o código flotante en el aire
        """;

            var imageGenerationOptions = new ImageGenerationOptions
            {
                Quality = GeneratedImageQuality.Standard,
                Size = GeneratedImageSize.W1792xH1024,
                Style = GeneratedImageStyle.Natural,
                ResponseFormat = GeneratedImageFormat.Bytes
            };

            var modeloImagenes = options.Value.ModeloImagenes;
            var clienteImagen = openAIClient.GetImageClient(modeloImagenes);
            var imagenGenerada = await clienteImagen.GenerateImageAsync(prompt, imageGenerationOptions);
            var bytes = imagenGenerada.Value.ImageBytes.ToArray();
            return bytes;
        }
    }
}
