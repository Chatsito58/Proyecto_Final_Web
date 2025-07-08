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
    public class ReservasController : Controller
    {
        private readonly BDCanchasContext _context;

        public ReservasController(BDCanchasContext context)
        {
            _context = context;
        }

        // GET: Reservas
        public async Task<IActionResult> Index()
        {
            var userRole = int.Parse(User.FindFirst("IdRol")?.Value ?? "0");
            var userId = int.Parse(User.FindFirst("IdUsuario")?.Value ?? "0");

            IQueryable<Reserva> query = _context.Reservas.Include(r => r.IdCanchaNavigation).Include(r => r.IdClienteNavigation).Include(r => r.IdEstadoReservaNavigation);

            // Clientes solo ven sus propias reservas
            if (userRole == 4)
            {
                query = query.Where(r => r.IdCliente == userId);
            }

            return View(await query.ToListAsync());
        }

        // GET: Reservas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.IdCanchaNavigation)
                .Include(r => r.IdClienteNavigation)
                .Include(r => r.IdEstadoReservaNavigation)
                .FirstOrDefaultAsync(m => m.IdReserva == id);
            if (reserva == null)
            {
                return NotFound();
            }

            return View(reserva);
        }

        // GET: Reservas/Create
        [Authorize(Roles = "Cliente,Empleado,Administrador,Gerente")]
        public IActionResult Create()
        {
            ViewData["IdCancha"] = new SelectList(_context.Canchas, "IdCancha", "Nombre");
            ViewData["IdCliente"] = new SelectList(_context.Usuarios.Where(u => u.IdRol == 4), "IdUsuario", "NombreCompleto");
            ViewData["IdEstadoReserva"] = new SelectList(_context.EstadosReservas, "IdEstadoReserva", "Nombre");

            return View();
        }

        // POST: Reservas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdReserva,IdCliente,IdCancha,FechaHoraInicio,FechaHoraFin,IdEstadoReserva,Valor")] Reserva reserva)
        {
            ModelState.Remove("IdCanchaNavigation");
            ModelState.Remove("IdClienteNavigation");
            ModelState.Remove("IdEstadoReservaNavigation");

            // Validar que el usuario seleccionado sea un cliente
            var cliente = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == reserva.IdCliente && u.IdRol == 4);
            if (cliente == null)
            {
                ModelState.AddModelError("IdCliente", "Solo se pueden crear reservas para usuarios de tipo Cliente.");
            }

            if (ModelState.IsValid)
            {
                reserva.FechaCreacion = DateTime.Now;
                _context.Add(reserva);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Recargar SelectLists en caso de error de validación
            ViewData["IdCancha"] = new SelectList(_context.Canchas, "IdCancha", "Nombre", reserva.IdCancha);
            ViewData["IdCliente"] = new SelectList(_context.Usuarios.Where(u => u.IdRol == 4), "IdUsuario", "NombreCompleto", reserva.IdCliente);
            ViewData["IdEstadoReserva"] = new SelectList(_context.EstadosReservas, "IdEstadoReserva", "Nombre", reserva.IdEstadoReserva);
            return View(reserva);
        }

        // GET: Reservas/Edit/5
        [Authorize(Roles = "Empleado,Administrador,Gerente")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null)
            {
                return NotFound();
            }

            var userRole = int.Parse(User.FindFirst("IdRol")?.Value ?? "0");
            var userId = int.Parse(User.FindFirst("IdUsuario")?.Value ?? "0");

            // Clientes no pueden editar reservas
            if (userRole == 4)
            {
                return Forbid();
            }

            ViewData["IdCancha"] = new SelectList(_context.Canchas, "IdCancha", "Nombre", reserva.IdCancha);
            ViewData["IdCliente"] = new SelectList(_context.Usuarios.Where(u => u.IdRol == 4), "IdUsuario", "NombreCompleto", reserva.IdCliente); // Solo clientes
            ViewData["IdEstadoReserva"] = new SelectList(_context.EstadosReservas, "IdEstadoReserva", "Nombre", reserva.IdEstadoReserva);
            return View(reserva);
        }

        // POST: Reservas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdReserva,IdCliente,IdCancha,FechaHoraInicio,FechaHoraFin,IdEstadoReserva,Valor")] Reserva reserva)
        {
            ModelState.Remove("IdCanchaNavigation");
            ModelState.Remove("IdClienteNavigation");
            ModelState.Remove("IdEstadoReservaNavigation");

            var userRole = int.Parse(User.FindFirst("IdRol")?.Value ?? "0");
            var userId = int.Parse(User.FindFirst("IdUsuario")?.Value ?? "0");

            // Clientes no pueden editar reservas
            if (userRole == 4)
            {
                return Forbid();
            }

            if (id != reserva.IdReserva)
            {
                return NotFound();
            }

            // Asegurar que solo se puedan editar reservas para clientes
            var cliente = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == reserva.IdCliente && u.IdRol == 4);
            if (cliente == null)
            {
                ModelState.AddModelError("IdCliente", "Solo se pueden asignar reservas a usuarios de tipo Cliente.");
            }

            // Obtener la reserva existente para preservar la FechaCreacion original
            var reservaExistente = await _context.Reservas.AsNoTracking().FirstOrDefaultAsync(r => r.IdReserva == id);
            if (reservaExistente != null)
            {
                reserva.FechaCreacion = reservaExistente.FechaCreacion;
            }
            else
            {
                reserva.FechaCreacion = DateTime.Now;
            }

            // Validar las fechas
            if (reserva.FechaHoraInicio < new DateTime(1753, 1, 1) ||
                reserva.FechaHoraFin < new DateTime(1753, 1, 1))
            {
                ModelState.AddModelError("", "Las fechas deben ser posteriores al 1/1/1753");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reserva);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservaExists(reserva.IdReserva))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewData["IdCancha"] = new SelectList(_context.Canchas, "IdCancha", "Nombre", reserva.IdCancha);
            ViewData["IdCliente"] = new SelectList(_context.Usuarios.Where(u => u.IdRol == 4), "IdUsuario", "NombreCompleto", reserva.IdCliente);
            ViewData["IdEstadoReserva"] = new SelectList(_context.EstadosReservas, "IdEstadoReserva", "Nombre", reserva.IdEstadoReserva);
            return View(reserva);
        }

        // GET: Reservas/Delete/5
        [Authorize(Roles = "Empleado,Administrador,Gerente")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reserva = await _context.Reservas
                .Include(r => r.IdCanchaNavigation)
                .Include(r => r.IdClienteNavigation)
                .Include(r => r.IdEstadoReservaNavigation)
                .FirstOrDefaultAsync(m => m.IdReserva == id);
            if (reserva == null)
            {
                return NotFound();
            }

            return View(reserva);
        }

        // POST: Reservas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva != null)
            {
                _context.Reservas.Remove(reserva);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReservaExists(int id)
        {
            return _context.Reservas.Any(e => e.IdReserva == id);
        }
    }
}