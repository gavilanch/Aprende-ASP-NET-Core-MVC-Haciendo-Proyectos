namespace BlogMVC.Utilidades
{
    public static class HttpContextExtensions
    {
        public static string ObtenerUrlRetorno(this HttpContext httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext);
            return httpContext.Request.Path + httpContext.Request.QueryString;
        }
    }
}
