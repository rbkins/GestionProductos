using Xunit;
using Moq;
using SistemaGestionProductos2.Controllers;
using SistemaGestionProductos2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;

namespace SistemaGestionProductos2.Tests.ProductoControllerTest
{
    public class ProductoControllerTests
    {
        private readonly Mock<SistemaGestionProductosContext> _mockContext;
        private readonly ProductoController _controller;

        public ProductoControllerTests()
        {
            _mockContext = new Mock<SistemaGestionProductosContext>();
            _controller = new ProductoController(_mockContext.Object);
        }

        [Fact]
        public async Task Create_ProductoValidoSinImagen_RedirigeAIndex()
        {
            // Arrange
            var producto = new Producto
            {
                Nombre = "Laptop",
                PrecioBase = 1000,
                Descripcion = "Test"
            };

            // Act
            var result = await _controller.Create(producto, null);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task Create_ProductoConImagen_GuardaRutaImagen()
        {
            // Arrange
            var producto = new Producto
            {
                Nombre = "Laptop",
                PrecioBase = 1000
            };

            // Mock de IFormFile
            var content = "Fake image content";
            var fileName = "test.jpg";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var mockImage = new FormFile(stream, 0, stream.Length, "Imagen", fileName);

            // Act
            var result = await _controller.Create(producto, mockImage);

            // Assert
            Assert.NotNull(producto.Imagen);
            Assert.Contains("/images/", producto.Imagen);        
            Assert.EndsWith(".jpg", producto.Imagen);             
        }


        [Fact]
        public async Task Create_PrecioDescuentoInvalido_MuestraError()
        {
            // Arrange
            var producto = new Producto
            {
                Nombre = "Laptop",
                PrecioBase = 1000,
                PrecioDescuento = 1200 // Inválido (mayor que precio base)
            };

            // Act
            var result = await _controller.Create(producto, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Contains(
                "El precio con descuento debe ser menor que el precio base",
                _controller.ModelState["PrecioDescuento"].Errors[0].ErrorMessage
            );
        }

        [Fact]
        public async Task Create_PrecioBaseCero_MuestraError()
        {
            
            var producto = new Producto
            {
                Nombre = "Laptop",
                PrecioBase = 0 // Inválido
            };

            
            var result = await _controller.Create(producto, null);

            
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }
    }
}
