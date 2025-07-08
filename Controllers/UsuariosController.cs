using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Proyecto_Final_Web.Models;
using Proyecto_Final_Web.Logica;

namespace Proyecto_Final_Web.Controllers
{
    [Authorize(Roles = "Empleado,Administrador,Gerente")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class UsuariosController : Controller
    {
        private readonly BDCanchasContext _context;

        public UsuariosController(BDCanchasContext context)
        {
            _context = context;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            var userRole = int.Parse(User.FindFirst("IdRol")?.Value ?? "0");

            IQueryable<Usuario> query = _context.Usuarios.Include(u => u.IdRolNavigation);
            if (userRole == 3) {                // Empleado solo ve clientes
                query = query.Where(u => u.IdRol == 4);
            } else if (userRole == 2) {         // Administrador ve empleados y clientes
                query = query.Where(u => u.IdRol >= 3);
            }
            // Gerente ve todo
            return View(await query.ToListAsync());
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(m => m.IdUsuario == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // GET: Usuarios/Create
        public IActionResult Create()
        {
            var userRole = int.Parse(User.FindFirst("IdRol")?.Value ?? "0");
            IQueryable<Role> rolesQuery = _context.Roles;

            if (userRole == 3) // Empleado solo puede crear clientes
            {
                rolesQuery = rolesQuery.Where(r => r.IdRol == 4);
            }
            else if (userRole == 2) // Administrador puede crear empleados y clientes
            {
                rolesQuery = rolesQuery.Where(r => r.IdRol >= 3);
            }
            // Gerente puede crear cualquier rol

            ViewData["IdRol"] = new SelectList(rolesQuery, "IdRol", "Nombre");
            return View();
        }

        // POST: Usuarios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdRol,NombreCompleto,Correo,Contrasena,Telefono")] Usuario usuario)
        {
            ModelState.Remove("IdRolNavigation");
            var userRole = int.Parse(User.FindFirst("IdRol")?.Value ?? "0");

            if (userRole == 3 && usuario.IdRol != 4) { return Forbid(); }
            else if (userRole == 2 && usuario.IdRol < 3) { return Forbid(); }

            if (ModelState.IsValid)
            {
                usuario.FechaRegistro = DateTime.Now; // Asigna la fecha actual
                usuario.Contrasena = PasswordSecurity.HashPassword(usuario.Contrasena);
                _context.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdRol"] = new SelectList(_context.Roles, "IdRol", "Nombre", usuario.IdRol);
            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            // No mostrar el hash de la contraseña en el formulario
            usuario.Contrasena = string.Empty;

            var userRole = int.Parse(User.FindFirst("IdRol")?.Value ?? "0");
            IQueryable<Role> rolesQuery = _context.Roles;

            if (userRole == 3) // Empleado solo puede editar clientes
            {
                rolesQuery = rolesQuery.Where(r => r.IdRol == 4);
            }
            else if (userRole == 2) // Administrador puede editar empleados y clientes
            {
                rolesQuery = rolesQuery.Where(r => r.IdRol >= 3);
            }
            // Gerente puede editar cualquier rol

            ViewData["IdRol"] = new SelectList(rolesQuery, "IdRol", "Nombre", usuario.IdRol);
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdUsuario,IdRol,NombreCompleto,Correo,Contrasena,Telefono,FechaRegistro")] Usuario usuario)
        {
            ModelState.Remove("IdRolNavigation");
            if (id != usuario.IdUsuario)
            {
                return NotFound();
            }

            var userRole = int.Parse(User.FindFirst("IdRol")?.Value ?? "0");

            if (userRole == 3 && usuario.IdRol != 4) { return Forbid(); }
            else if (userRole == 2 && usuario.IdRol < 3) { return Forbid(); }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.IdUsuario == id);
                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    if (string.IsNullOrWhiteSpace(usuario.Contrasena))
                    {
                        usuario.Contrasena = existingUser.Contrasena;
                    }
                    else
                    {
                        usuario.Contrasena = PasswordSecurity.HashPassword(usuario.Contrasena);
                    }

                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.IdUsuario))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdRol"] = new SelectList(_context.Roles, "IdRol", "Nombre", usuario.IdRol);
            return View(usuario);
        }

        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(m => m.IdUsuario == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Usuarios/Register
        [AllowAnonymous]
        public IActionResult Register()
        {
            ViewData["HideLayout"] = true;
            ViewData["IsPublicRegister"] = true;
            ViewData["IdRol"] = new SelectList(_context.Roles.Where(r => r.IdRol == 4), "IdRol", "Nombre");
            var usuario = new Usuario { IdRol = 4 };
            return View("Create", usuario);
        }

        // POST: Usuarios/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("NombreCompleto,Correo,Contrasena,Telefono")] Usuario usuario)
        {
            ModelState.Remove("IdRolNavigation");
            usuario.IdRol = 4;

            if (ModelState.IsValid)
            {
                usuario.FechaRegistro = DateTime.Now;
                usuario.Contrasena = PasswordSecurity.HashPassword(usuario.Contrasena);
                _context.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Acceso");
            }
            ViewData["HideLayout"] = true;
            ViewData["IsPublicRegister"] = true;
            ViewData["IdRol"] = new SelectList(_context.Roles.Where(r => r.IdRol == 4), "IdRol", "Nombre", usuario.IdRol);
            return View("Create", usuario);
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.IdUsuario == id);
        }
    }
}
