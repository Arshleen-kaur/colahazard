using PackTrack.Components;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();

// RIS Services
builder.Services.AddSingleton<PackTrack.Data.LiteDbContext>();
builder.Services.AddScoped<PackTrack.Data.Repositories.InventoryRepository>();
builder.Services.AddScoped<PackTrack.Data.Repositories.RackRepository>();
builder.Services.AddScoped<PackTrack.Data.Repositories.InvoiceRepository>();
builder.Services.AddScoped<PackTrack.Data.Repositories.BatchRepository>();
builder.Services.AddScoped<PackTrack.Services.InventoryService>();
builder.Services.AddScoped<PackTrack.Services.RackService>();
builder.Services.AddScoped<PackTrack.Services.InvoiceService>();
builder.Services.AddScoped<PackTrack.Services.PredictionService>();
builder.Services.AddScoped<PackTrack.Services.OcrService>();
builder.Services.AddScoped<PackTrack.Services.FactoryService>();
builder.Services.AddScoped<PackTrack.Services.LogisticsService>();
builder.Services.AddScoped<PackTrack.Services.RetailService>();
builder.Services.AddScoped<PackTrack.Services.SustainabilityService>();
builder.Services.AddScoped<PackTrack.Services.ReportingService>();
builder.Services.AddScoped<PackTrack.Services.PdfReportService>();
builder.Services.AddScoped<PackTrack.Services.ManifestGenerator>();
builder.Services.AddScoped<PackTrack.Features.Dashboards.Logistics.ITransportDashboardViewModel, PackTrack.Features.Dashboards.Logistics.TransportDashboardViewModel>();




var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
