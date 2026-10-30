using GdPicture14;
using MultiDocPageSelection.Services;

var builder = WebApplication.CreateBuilder(args);

// GdPicture license key from configuration or the GDPICTURE_LICENSE_KEY environment variable.
// An empty key starts GdPicture in evaluation mode. RegisterKEY must still be called:
// without it, GdPicture refuses PDF processing.
var gdPictureLicenseKey = builder.Configuration["GDPICTURE_LICENSE_KEY"] ?? string.Empty;
new LicenseManager().RegisterKEY(gdPictureLicenseKey.Trim());

// Configure documents folder
var documentsPath = Path.Combine(builder.Environment.ContentRootPath, "Documents");
if (!Directory.Exists(documentsPath))
    Directory.CreateDirectory(documentsPath);

// Register services
builder.Services.AddSingleton(new GdPictureService(documentsPath));
builder.Services.AddControllers();

// CORS for Angular dev server
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();

// Serve documents folder so Nutrient viewer can load PDFs
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(documentsPath),
    RequestPath = "/documents"
});

// Web SDK license key for the client. Empty = evaluation mode.
app.MapGet("/api/config", (IConfiguration config) =>
    Results.Ok(new { licenseKey = config["NUTRIENT_WEB_LICENSE_KEY"] ?? string.Empty }));

app.MapControllers();

app.Run();
