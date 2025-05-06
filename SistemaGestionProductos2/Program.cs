using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using SistemaGestionProductos2.Models;


var builder = WebApplication.CreateBuilder(args);




// Add services to the container.
builder.Services.AddControllersWithViews();


builder.Services.AddDbContext<SistemaGestionProductosContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("cadenaconexion"))

);
var app = builder.Build();


app.UseStaticFiles();

// Carpeta donde se guardarán las imágenes:
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.WebRootPath, "images")),
    RequestPath = "/images"
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Producto}/{action=Index}/{id?}");

app.Run();
