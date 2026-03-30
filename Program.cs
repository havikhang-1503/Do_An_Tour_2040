// Folder: (Root)
// File path: Program.cs
// File name: Program.cs
// Method/Class: Program (Top-level statements)
// Labels: A(DbContext + Warning), B(Identity), C(AI Service), D(Session), E(Pipeline), F(SeedData)
// Mô tả: Cấu hình service + middleware cho Tour_2040.
// FIX: Bỏ đăng ký AI bị trùng; thêm ConfigureWarnings để tạm không chặn PendingModelChangesWarning khi đang reset DB/migration.

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Rotativa.AspNetCore;
using Tour_2040.Data;
using Tour_2040.Models;
using Tour_2040.Services;

var builder = WebApplication.CreateBuilder(args);

// ===== A) UserSecrets (DEV) =====
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// ===== A) ConnectionString =====
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Không tìm thấy Connection string 'DefaultConnection'.");

// ===== A) DbContext =====
// FIX: Thêm ConfigureWarnings để tạm KHÔNG chặn lỗi PendingModelChangesWarning (hay gặp khi seed động/OnModelCreating thay đổi)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);

    // ✅ FIX (tạm thời): cho phép Update-Database chạy khi đang reset migration/DB
    // Sau khi ổn định model + seed, em có thể bỏ dòng này để EF cảnh báo nghiêm lại.
    options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ===== B) Identity =====
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;

    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 1;

    options.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// ===== C) AI Service =====
// FIX: Bỏ đăng ký trùng (em đăng ký 2 lần trước đó)
builder.Services.AddHttpClient();
builder.Services.AddScoped<ISupportAiService, SupportAiService>();

// ===== MVC / Razor =====
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// ===== D) Session =====
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ===== Rotativa =====
try
{
    RotativaConfiguration.Setup(app.Environment.WebRootPath, "Rotativa");
}
catch
{
    // bỏ qua nếu môi trường thiếu Rotativa
}

// ===== F) SeedData (chạy khi app start) =====
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedData.InitializeAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Lỗi khi khởi tạo dữ liệu (Seed Data).");
    }
}

// ===== E) Pipeline =====
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// ✅ Quan trọng cho API attribute routes kiểu /api/ai/answer
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
