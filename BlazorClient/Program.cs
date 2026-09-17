using BlazorClient;
using BlazorClient.Components;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddTransient<ApiKeyDelegatingHandler>();

// Add API key to each http Client via httpHandler (Delegating Handler)
// It is better way if your requirements grow.
// 
// the key is dynamically added to each and every individual HTTP call right before it leaves your application.
builder.Services.AddHttpClient("ProtectedApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7123/");
})
.AddHttpMessageHandler<ApiKeyDelegatingHandler>();


builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
