const quill = new Quill('#editor', {
    modules: {
        toolbar: [
            [{ header: [1, 2, false] }],
            ['bold', 'italic', 'underline'],
            ['code-block'],
        ],
    },
    placeholder: 'Coloque aquí la entrada...',
    theme: 'snow', // or 'bubble'
});

function cargarContenido(contenido) {
    quill.setContents(contenido, 'silent');
}

function btnEnviarClick() {
    let esValido = validarFormularioCompleto();

    if (!esValido) {
        return;
    }

    const delta = quill.getContents();
    const deltaJSON = JSON.stringify(delta.ops);

    $("#Cuerpo").val(deltaJSON);
    $("#formEntrada").trigger("submit");
}

function validarFormularioCompleto() {
    let formEntradaEsValido = $("#formEntrada").valid();
    let cuerpoEsValido = validarCuerpo();

    return formEntradaEsValido && cuerpoEsValido;
}

function validarCuerpo() {
    let mensajeDeError = null;
    let esValido = true;

    const htmlCuerpo = quill.getSemanticHTML();

    if (htmlCuerpo === '<p></p>') {
        mensajeDeError = 'El Cuerpo es requerido';
        esValido = false;
    }

    $("#cuerpo-error").html(mensajeDeError);
    return esValido;
}

quill.on('text-change', function (delta, oldDelta, source) {
    validarCuerpo();
})

function mostrarPrevisualizacion(event) {
    const input = event.target;
    const imagenPreview = document.getElementById('PreviewImagen');

    if (input.files && input.files[0]) {
        const urlImagen = URL.createObjectURL(input.files[0]);
        imagenPreview.src = urlImagen;
        imagenPreview.style.display = "block";
    }
}
