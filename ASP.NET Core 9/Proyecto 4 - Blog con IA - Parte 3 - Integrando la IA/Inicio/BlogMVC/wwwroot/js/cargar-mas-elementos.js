window.observarElemento = (idElemento, dotNetHelper) => {
    let observador = new IntersectionObserver((entradas) => {
        if (entradas[0].isIntersecting) {
            dotNetHelper.invokeMethodAsync("CargarMasElementos");
        }
    });

    let elemento = document.getElementById(idElemento);
    if (elemento) {
        observador.observe(elemento);
    }
}