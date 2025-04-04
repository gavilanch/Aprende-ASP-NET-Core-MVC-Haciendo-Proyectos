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

async function generarImagen() {
    const titulo = document.getElementById('Titulo').value;

    if (!titulo) {
        alert('El título no puede estar vacío');
        return;
    }

    const ImagenPortadaInput = document.getElementById('ImagenPortada');
    ImagenPortadaInput.value = '';

    const imagenPreview = document.getElementById('PreviewImagen');
    imagenPreview.style.display = "none";

    const loading = document.getElementById('loading-imagen-ia');
    loading.style.display = "block";

    const respuesta = await fetch('/entradas/GenerarImagen?titulo=' + encodeURIComponent(titulo));

    if (!respuesta.ok) {
        const contenido = await respuesta.text();
        alert(contenido);
        return;
    }

    const blob = await respuesta.blob();
    imagenPreview.src = URL.createObjectURL(blob);
    imagenPreview.style.display = "block";
    loading.style.display = "none";

    const base64String = await convertirBlobABase64(blob);
    document.getElementById('ImagenPortadaIA').value = base64String;
}

async function convertirBlobABase64(blob) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.readAsDataURL(blob);
        reader.onloadend = () => {
            const base64String = reader.result.split(",")[1]; 
            resolve(base64String);
        }

        reader.onerror = reject;
    });
}
