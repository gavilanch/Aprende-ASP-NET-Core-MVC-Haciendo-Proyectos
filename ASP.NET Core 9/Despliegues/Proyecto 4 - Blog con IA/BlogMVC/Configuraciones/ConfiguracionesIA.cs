using System.ComponentModel.DataAnnotations;

namespace BlogMVC.Configuraciones
{
    public class ConfiguracionesIA
    {
        public const string Seccion = "ConfiguracionesIA";
        [Required]
        public required string ModeloTexto { get; set; }
        [Required]
        public required string ModeloImagenes { get; set; }
        [Required]
        public required string ModeloSentimientos { get; set; }
        [Required]
        public required string LlaveOpenAI { get; set; }

    }
}
