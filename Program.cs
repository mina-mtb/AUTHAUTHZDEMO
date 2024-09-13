using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? throw new ArgumentNullException("Authentication:Google:ClientId");
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? throw new ArgumentNullException("Authentication:Google:ClientSecret");
    })
    .AddOpenIdConnect(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Cognito:ClientId"] ?? throw new InvalidOperationException("Cognito ClientId is not set.");
        options.ResponseType = builder.Configuration["Authentication:Cognito:ResponseType"] ?? throw new InvalidOperationException("Cognito ResponseType is not set.");
        options.MetadataAddress = builder.Configuration["Authentication:Cognito:MetadataAddress"] ?? throw new InvalidOperationException("Cognito MetadataAddress is not set.");
        options.Events = new OpenIdConnectEvents
        {
            OnRedirectToIdentityProviderForSignOut = context =>
            {
                context.ProtocolMessage.Scope = builder.Configuration["Authentication:Cognito:Scope"] ?? throw new InvalidOperationException("Cognito Scope is not set.");
                context.ProtocolMessage.ResponseType = builder.Configuration["Authentication:Cognito:ResponseType"] ?? throw new InvalidOperationException("Cognito ResponseType is not set.");;
                // context.ProtocolMessage.IssuerAddress = CognitoHelpers.GetCognitoLogoutUrl(builder.Configuration, context.HttpContext);

                // Create Cognito logout URL
                var cognitoDomain = builder.Configuration["Authentication:Cognito:CognitoDomain"] ?? throw new InvalidOperationException("Cognito CognitoDomain is not set.");
                var clientId = builder.Configuration["Authentication:Cognito:ClientId"] ?? throw new InvalidOperationException("Cognito ClientId is not set.");
                var appSignOutUrl = builder.Configuration["Authentication:Cognito:AppSignOutUrl"] ?? throw new InvalidOperationException("Cognito AppSignOutUrl is not set.");
                var logoutUrl = $"{context.Request.Scheme}://{context.Request.Host}{appSignOutUrl}";
                var cognitoLogoutUrl = $"{cognitoDomain}/logout?client_id={clientId}&logout_uri={logoutUrl}";

                context.ProtocolMessage.IssuerAddress = cognitoLogoutUrl;

                // Close authentication sessions
                context.Properties.Items.Remove(CookieAuthenticationDefaults.AuthenticationScheme);
                context.Properties.Items.Remove(OpenIdConnectDefaults.AuthenticationScheme);

                return Task.CompletedTask;
            }
        };
    });




var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

var port = Environment.GetEnvironmentVariable("PORT") ?? "";
if (!string.IsNullOrEmpty(port))
{
    app.Urls.Add($"http://*:{port}");
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
