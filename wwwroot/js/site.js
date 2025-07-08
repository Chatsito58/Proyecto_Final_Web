// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

window.addEventListener('pageshow', function (evt) {
    if (evt.persisted) {   // la página viene del back/forward cache
        window.location.reload();
    }
});

