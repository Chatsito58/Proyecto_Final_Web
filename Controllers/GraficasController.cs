using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_Final_Web.Models;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Proyecto_Final_Web.Controllers
{
    [Authorize(Roles = "Empleado,Administrador,Gerente")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class GraficasController : Controller
    {
        private readonly BDCanchasContext _context;

        public GraficasController(BDCanchasContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Usuarios()
        {
            var usuariosPorRol = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .GroupBy(u => u.IdRolNavigation.Nombre)
                .Select(g => new { Rol = g.Key, Cantidad = g.Count() })
                .ToListAsync();

            ViewBag.ChartData = JsonConvert.SerializeObject(usuariosPorRol);
            return View();
        }

        public async Task<IActionResult> Canchas()
        {
            var canchasPorTipo = await _context.Canchas
                .GroupBy(c => c.TipoCancha)
                .Select(g => new { Tipo = g.Key, Cantidad = g.Count() })
                .ToListAsync();

            ViewBag.ChartData = JsonConvert.SerializeObject(canchasPorTipo);
            return View(); 
        }

        public async Task<IActionResult> Facturas()
        {
            var facturasPorEstado = await _context.Facturas
                .Include(f => f.IdEstadoFacturaNavigation)
                .GroupBy(f => f.IdEstadoFacturaNavigation.Nombre)
                .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
                .ToListAsync();

            ViewBag.ChartData = JsonConvert.SerializeObject(facturasPorEstado);
            return View(); 
        }

        public async Task<IActionResult> Reservas()
        {
            var reservasPorEstado = await _context.Reservas
                .Include(r => r.IdEstadoReservaNavigation)
                .GroupBy(r => r.IdEstadoReservaNavigation.Nombre)
                .Select(g => new { Estado = g.Key, Cantidad = g.Count() })
                .ToListAsync();

            ViewBag.ChartData = JsonConvert.SerializeObject(reservasPorEstado);
            return View();
        }
    }
}