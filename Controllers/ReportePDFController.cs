using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_Final_Web.Models;
using Rotativa.AspNetCore;

namespace Proyecto_Final_Web.Controllers
{
    [Authorize(Roles = "Empleado,Administrador,Gerente")]
    public class ReportePDFController : Controller
    {
        private readonly BDCanchasContext contexto;
        public ReportePDFController(BDCanchasContext contexto)
        {
            this.contexto = contexto;
        }
        // GET: ReportePDFController
        public async Task<IActionResult> Usuarios()
        {
            var usuarios = await this.contexto.Usuarios.Include(u => u.IdRolNavigation).ToListAsync();
            return new ViewAsPdf("Usuarios", usuarios)
            {
                PageSize = Rotativa.AspNetCore.Options.Size.Letter,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                PageMargins = { Top = 30 }, // Margen superior para el encabezado
                CustomSwitches = "--header-html \"" + Url.Action("UsuariosHeader", "ReportePDF", new { }, Request.Scheme) + "\" --header-spacing 5"
            };
        }

        public IActionResult UsuariosHeader()
        {
            return View("Header", "Listado de Usuarios");
        }

        public async Task<IActionResult> Canchas()
        {
            var canchas = await this.contexto.Canchas.ToListAsync();
            return new ViewAsPdf("Canchas", canchas)
            {
                PageSize = Rotativa.AspNetCore.Options.Size.Letter,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                PageMargins = { Top = 30 }, // Margen superior para el encabezado
                CustomSwitches = "--header-html \"" + Url.Action("CanchasHeader", "ReportePDF", new { }, Request.Scheme) + "\" --header-spacing 5"
            };
        }

        public IActionResult CanchasHeader()
        {
            return View("Header", "Listado de Canchas");
        }

        public async Task<IActionResult> Facturas()
        {
            var facturas = await this.contexto.Facturas
                .Include(f => f.IdClienteNavigation)
                .Include(f => f.IdEstadoFacturaNavigation)
                .Include(f => f.IdMetodoPagoNavigation)
                .ToListAsync();
            return new ViewAsPdf("Facturas", facturas)
            {
                PageSize = Rotativa.AspNetCore.Options.Size.Letter,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                PageMargins = { Top = 30 }, // Margen superior para el encabezado
                CustomSwitches = "--header-html \"" + Url.Action("FacturasHeader", "ReportePDF", new { }, Request.Scheme) + "\" --header-spacing 5"
            };
        }

        public IActionResult FacturasHeader()
        {
            return View("Header", "Listado de Facturas");
        }

        public async Task<IActionResult> Reservas()
        {
            var reservas = await this.contexto.Reservas
                .Include(r => r.IdClienteNavigation)
                .Include(r => r.IdCanchaNavigation)
                .Include(r => r.IdEstadoReservaNavigation)
                .ToListAsync();
            return new ViewAsPdf("Reservas", reservas);
        }

        public IActionResult ReservasHeader()
        {
            return View("Header", "Listado de Reservas");
        }

        public async Task<IActionResult> Tarifas()
        {
            var tarifas = await this.contexto.Tarifas.Include(t => t.IdCanchaNavigation).ToListAsync();
            return new ViewAsPdf("Tarifas", tarifas);
        }

        public IActionResult TarifasHeader()
        {
            return View("Header", "Listado de Tarifas");
        }
    }
}