namespace BlogMVC.Entidades
{
    public class Lote
    {
        public required string Id { get; set; }
        public required string Estatus { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
