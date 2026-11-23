window.nutri = {
    irArriba: () => window.scrollTo({ top: 0 }),

    enfocarPrimerInvalido: (formulario) => {
        const campo = formulario && formulario.querySelector('[aria-invalid="true"]');
        if (campo) campo.focus();
    },

    enfocarId: (id) => {
        const campo = document.getElementById(id);
        if (campo) campo.focus();
    },

    observarAncho: (elemento, referencia) => {
        if (!elemento) return;
        const medir = () => referencia.invokeMethodAsync('ActualizarAncho', Math.floor(elemento.clientWidth));
        medir();
        const observador = new ResizeObserver(medir);
        observador.observe(elemento);
        elemento.__observador = observador;
    },

    dejarDeObservar: (elemento) => {
        if (elemento && elemento.__observador) elemento.__observador.disconnect();
    },
};