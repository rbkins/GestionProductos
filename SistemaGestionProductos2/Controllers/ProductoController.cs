using Microsoft.AspNetCore.Mvc;
using SistemaGestionProductos2.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace SistemaGestionProductos2.Controllers

{
    public class ProductoController : Controller
    {

        private readonly SistemaGestionProductosContext _SistemaGestionProductosContext;

        public ProductoController(SistemaGestionProductosContext _context) {

            _SistemaGestionProductosContext = _context;


        }


        public async Task<IActionResult> Index()
        {
            var productos = await _SistemaGestionProductosContext.Productos
                .FromSqlRaw("EXEC sp_ObtenerProductos")
                .ToListAsync();

            return View(productos);
        }

        [HttpGet]

        public IActionResult Create()
        {
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
      [Bind("Nombre,Descripcion,PrecioBase,PrecioDescuento")] Producto producto,
      IFormFile? Imagen) 
        {
          
            if (producto.PrecioBase <= 0)
            {
                ModelState.AddModelError("PrecioBase", "El precio base debe ser mayor que 0");
            }

            // Validación: Precio descuento debe ser menor que el base (si existe)
            if (producto.PrecioDescuento.HasValue && producto.PrecioDescuento.Value >= producto.PrecioBase)
            {
                ModelState.AddModelError("PrecioDescuento", "El precio con descuento debe ser menor que el precio base.");
            }

            if (ModelState.IsValid)
            {
                // Subir imagen si existe
                if (Imagen != null && Imagen.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Imagen.FileName);
                    var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

                    // Asegura que el directorio exista (esto previene excepciones en tests)
                    if (!Directory.Exists(imagesFolder))
                    {
                        Directory.CreateDirectory(imagesFolder);
                    }

                    var filePath = Path.Combine(imagesFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await Imagen.CopyToAsync(stream);
                    }

                    producto.Imagen = "/images/" + fileName; 
                }

                producto.FechaCreacion = DateTime.Now;
                producto.FechaModificacion = DateTime.Now;

                _SistemaGestionProductosContext.Add(producto);
                await _SistemaGestionProductosContext.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(producto); 
        }


        [HttpPost] 
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Delete(int id) 
        {
            var producto = await _SistemaGestionProductosContext.Productos.FindAsync(id);
            if (producto != null)
            {
                _SistemaGestionProductosContext.Productos.Remove(producto);
                await _SistemaGestionProductosContext.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var producto = await _SistemaGestionProductosContext.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }
            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Descripcion,PrecioBase,PrecioDescuento")] Producto producto, IFormFile Imagen)
        {
            if (id != producto.Id)
            {
                return NotFound();
            }

            if (producto.PrecioDescuento.HasValue && producto.PrecioDescuento.Value >= producto.PrecioBase)
            {
                ModelState.AddModelError("PrecioDescuento", "El precio con descuento debe ser menor que el precio base.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var productoExistente = await _SistemaGestionProductosContext.Productos.FindAsync(id);
                    if (productoExistente == null)
                        return NotFound();

                    // Actualizar propiedades manualmente
                    productoExistente.Nombre = producto.Nombre;
                    productoExistente.Descripcion = producto.Descripcion;
                    productoExistente.PrecioBase = producto.PrecioBase;
                    productoExistente.PrecioDescuento = producto.PrecioDescuento;
                    productoExistente.FechaModificacion = DateTime.Now;

                    if (Imagen != null && Imagen.Length > 0)
                    {
                        // Eliminar imagen anterior si existe
                        if (!string.IsNullOrEmpty(productoExistente.Imagen))
                        {
                            var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", productoExistente.Imagen.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Guardar nueva imagen
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Imagen.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await Imagen.CopyToAsync(stream);
                        }

                        productoExistente.Imagen = "/images/" + fileName;
                    }
                   

                    _SistemaGestionProductosContext.Update(productoExistente);
                    await _SistemaGestionProductosContext.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(producto);
        }



        private bool ProductoExists(int id)
        {
            return _SistemaGestionProductosContext.Productos.Any(e => e.Id == id);
        }




    }
}
