using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Proyecto_Final_Web.Models;

namespace Proyecto_Final_Web.Controllers
{
    [Authorize(Roles = "Cliente,Empleado,Administrador,Gerente")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class FacturasController : Controller
    {
        private readonly BDCanchasContext _context;

        public FacturasController(BDCanchasContext context)
        {
            _context = context;
        }

        // GET: Facturas
        public async Task<IActionResult> Index()
        {
            var userRole = int.Parse(User.FindFirst("IdRol")?.Value ?? "0");
            var userId = int.Parse(User.FindFirst("IdUsuario")?.Value ?? "0");

            IQueryable<Factura> query = _context.Facturas.Include(f => f.IdClienteNavigation).Include(f => f.IdEstadoFacturaNavigation).Include(f => f.IdMetodoPagoNavigation).Include(f => f.IdReservaNavigation);

            // Clientes solo ven sus propias facturas
            if (User.IsInRole("cliente"))
            {
                query = query.Where(f => f.IdCliente == userId);
            }

            return View(await query.ToListAsync());
        }

        // GET: Facturas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var factura = await _context.Facturas
                .Include(f => f.IdClienteNavigation)
                .Include(f => f.IdEstadoFacturaNavigation)
                .Include(f => f.IdMetodoPagoNavigation)
                .Include(f => f.IdReservaNavigation)
                .FirstOrDefaultAsync(m => m.IdFactura == id);
            if (factura == null)
            {
                return NotFound();
            }

            return View(factura);
        }

        // GET: Facturas/Create
        [Authorize(Roles = "Empleado,Administrador,Gerente")]
        public IActionResult Create()
        {
            var userRole = int.Parse(User.FindFirst("IdRol")?.Value ?? "0");
            var userId = int.Parse(User.FindFirst("IdUsuario")?.Value ?? "0");

            ViewData["IdReserva"] = new SelectList(_context.Reservas, "IdReserva", "IdReserva");
            ViewData["IdMetodoPago"] = new SelectList(_context.MetodosPagos, "IdMetodoPago", "Nombre");

            if (User.IsInRole("cliente")) // Cliente
            {
                // Si es cliente, solo puede seleccionarse a sí mismo y el estado es pendiente
                ViewData["IdCliente"] = new SelectList(_context.Usuarios.Where(u => u.IdUsuario == userId), "IdUsuario", "NombreCompleto", userId);
                ViewData["IdEstadoFactura"] = new SelectList(_context.EstadosFacturas.Where(e => e.IdEstadoFactura == 1), "IdEstadoFactura", "Nombre", 1); // Pendiente
            }
            else // Empleado, Administrador, Gerente
            {
                ViewData["IdCliente"] = new SelectList(_context.Usuarios.Where(u => u.IdRol == 4), "IdUsuario", "NombreCompleto"); // Solo clientes
                ViewData["IdEstadoFactura"] = new SelectList(_context.EstadosFacturas, "IdEstadoFactura", "Nombre");
            }
            return View();
        }

        // POST: Facturas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdFactura,IdReserva,IdCliente,IdEstadoFactura,IdMetodoPago,Total")] Factura factura)
        {
            ModelState.Remove("IdClienteNavigation");
            ModelState.Remove("IdEstadoFacturaNavigation");
            ModelState.Remove("IdMetodoPagoNavigation");
            ModelState.Remove("IdReservaNavigation");

            var userRole = int.Parse(User.FindFirst("IdRol")?.Value ?? "0");
            var userId = int.Parse(User.FindFirst("IdUsuario")?.Value ?? "0");

            if (User.IsInRole("cliente")) // Cliente
            {
                factura.IdCliente = userId; // El cliente solo puede crear facturas para sí mismo
                factura.IdEstadoFactura = 1; // Estado inicial: pendiente
            }
            else // Empleado, Administrador, Gerente
            {
                // Asegurar que solo se puedan crear facturas para clientes
                var cliente = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == factura.IdCliente && u.IdRol == 4);
                if (cliente == null)
                {
                    ModelState.AddModelError("IdCliente", "Solo se pueden crear facturas para usuarios de tipo Cliente.");
                }
                factura.IdEstadoFactura = 1; // Estado inicial: pendiente
            }

            if (ModelState.IsValid)
            {
                factura.FechaEmision = DateTime.Now;
                _context.Add(factura);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Recargar SelectLists en caso de error de validación
            ViewData["IdReserva"] = new SelectList(_context.Reservas, "IdReserva", "IdReserva", factura.IdReserva);
            ViewData["IdMetodoPago"] = new SelectList(_context.MetodosPagos, "IdMetodoPago", "Nombre", factura.IdMetodoPago);
            if (User.IsInRole("cliente"))
            {
                ViewData["IdCliente"] = new SelectList(_context.Usuarios.Where(u => u.IdUsuario == userId), "IdUsuario", "NombreCompleto", userId);
                ViewData["IdEstadoFactura"] = new SelectList(_context.EstadosFacturas.Where(e => e.IdEstadoFactura == 1), "IdEstadoFactura", "Nombre", 1);
            }
            else
            {
                ViewData["IdCliente"] = new SelectList(_context.Usuarios.Where(u => u.IdRol == 4), "IdUsuario", "NombreCompleto", factura.IdCliente);
                ViewData["IdEstadoFactura"] = new SelectList(_context.EstadosFacturas, "IdEstadoFactura", "Nombre", factura.IdEstadoFactura);
            }
            return View(factura);
        }

        // GET: Facturas/Edit/5
        [Authorize(Roles = "Empleado,Administrador,Gerente")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var factura = await _context.Facturas.FindAsync(id);
            if (factura == null)
            {
                return NotFound();
            }

            var userRole = int.Parse(User.FindFirst("IdRol")?.Value ?? "0");

            // Clientes no pueden editar facturas
            if (User.IsInRole("cliente"))
            {
                return Forbid();
            }

            ViewData["IdReserva"] = new SelectList(_context.Reservas, "IdReserva", "IdReserva", factura.IdReserva);
            ViewData["IdCliente"] = new SelectList(_context.Usuarios.Where(u => u.IdRol == 4), "IdUsuario", "NombreCompleto", factura.IdCliente); // Solo clientes
            ViewData["IdEstadoFactura"] = new SelectList(_context.EstadosFacturas, "IdEstadoFactura", "Nombre", factura.IdEstadoFactura);
            ViewData["IdMetodoPago"] = new SelectList(_context.MetodosPagos, "IdMetodoPago", "Nombre", factura.IdMetodoPago);
            return View(factura);
        }

        // POST: Facturas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdFactura,IdReserva,IdCliente,IdEstadoFactura,IdMetodoPago,Total")] Factura factura)
        {
            ModelState.Remove("IdClienteNavigation");
            ModelState.Remove("IdEstadoFacturaNavigation");
            ModelState.Remove("IdMetodoPagoNavigation");
            ModelState.Remove("IdReservaNavigation");

            var userRole = int.Parse(User.FindFirst("IdRol")?.Value ?? "0");

            // Clientes no pueden editar facturas
            if (User.IsInRole("cliente"))
            {
                return Forbid();
            }

            if (id != factura.IdFactura)
            {
                return NotFound();
            }

            // Asegurar que solo se puedan asignar facturas a clientes
            var cliente = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == factura.IdCliente && u.IdRol == 4);
            if (cliente == null)
            {
                ModelState.AddModelError("IdCliente", "Solo se pueden asignar facturas a usuarios de tipo Cliente.");
            }

            // Obtener la factura existente para preservar la FechaEmision original
            var facturaExistente = await _context.Facturas.AsNoTracking().FirstOrDefaultAsync(f => f.IdFactura == id);
            if (facturaExistente != null)
            {
                factura.FechaEmision = facturaExistente.FechaEmision;
            }
            else
            {
                factura.FechaEmision = DateTime.Now;
            }

            // Validar la fecha
            if (factura.FechaEmision < new DateTime(1753, 1, 1))
            {
                ModelState.AddModelError("", "La fecha de emisión debe ser posterior al 1/1/1753");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(factura);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FacturaExists(factura.IdFactura))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewData["IdReserva"] = new SelectList(_context.Reservas, "IdReserva", "IdReserva", factura.IdReserva);
            ViewData["IdCliente"] = new SelectList(_context.Usuarios.Where(u => u.IdRol == 4), "IdUsuario", "NombreCompleto", factura.IdCliente);
            ViewData["IdEstadoFactura"] = new SelectList(_context.EstadosFacturas, "IdEstadoFactura", "Nombre", factura.IdEstadoFactura);
            ViewData["IdMetodoPago"] = new SelectList(_context.MetodosPagos, "IdMetodoPago", "Nombre", factura.IdMetodoPago);
            return View(factura);
        }

        // GET: Facturas/Delete/5
        [Authorize(Roles = "Empleado,Administrador,Gerente")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var factura = await _context.Facturas
                .Include(f => f.IdClienteNavigation)
                .Include(f => f.IdEstadoFacturaNavigation)
                .Include(f => f.IdMetodoPagoNavigation)
                .Include(f => f.IdReservaNavigation)
                .FirstOrDefaultAsync(m => m.IdFactura == id);
            if (factura == null)
            {
                return NotFound();
            }

            return View(factura);
        }

        // POST: Facturas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var factura = await _context.Facturas.FindAsync(id);
            if (factura != null)
            {
                _context.Facturas.Remove(factura);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FacturaExists(int id)
        {
            return _context.Facturas.Any(e => e.IdFactura == id);
        }
    }
}