var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Named HttpClient trỏ tới API
builder.Services.AddHttpClient("BookApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!);
});

// Session để lưu JWT token sau khi đăng nhập
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();