// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

window.addEventListener('pageshow', function (evt) {
    if (evt.persisted) {   // la página viene del back/forward cache
        window.location.reload();
    }
});

document.addEventListener('DOMContentLoaded', () => {
    const canchaSelect = document.getElementById('IdCancha');
    const inicioInput = document.getElementById('FechaHoraInicio');
    const finInput = document.getElementById('FechaHoraFin');
    const valorInput = document.getElementById('Valor');
    const valorDisplay = document.getElementById('ValorDisplay');

    async function obtenerPrecioHora(idCancha) {
        if (!idCancha) return 0;
        try {
            const respuesta = await fetch(`/Canchas/PrecioHora?id=${idCancha}`);
            if (!respuesta.ok) return 0;
            return parseFloat(await respuesta.json());
        } catch (e) {
            console.error('Error obteniendo PrecioHora:', e);
            return 0;
        }
    }

    async function calcularValor() {
        const idCancha = canchaSelect ? canchaSelect.value : null;
        const inicio = inicioInput ? new Date(inicioInput.value) : null;
        const fin = finInput ? new Date(finInput.value) : null;

        if (!idCancha || isNaN(inicio) || isNaN(fin)) {
            if (valorInput) valorInput.value = '';
            if (valorDisplay) valorDisplay.value = '';
            return;
        }

        const precioHora = await obtenerPrecioHora(idCancha);
        const horas = (fin - inicio) / 3600000; // ms a horas
        if (horas > 0 && precioHora >= 0) {
            const total = (horas * precioHora).toFixed(2);
            if (valorInput) valorInput.value = total;
            if (valorDisplay) valorDisplay.value = total;
        }
    }

    if (canchaSelect) canchaSelect.addEventListener('change', calcularValor);
    if (inicioInput) inicioInput.addEventListener('change', calcularValor);
    if (finInput) finInput.addEventListener('change', calcularValor);
});

