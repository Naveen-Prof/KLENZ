using KLENZ.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// ✅ Configure Database Context (Fixes duplicate registration)
builder.Services.AddDbContext<KLENZDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("KLENZDbContext"),
    sqlServerOptionsAction: sqlOptions => {
        sqlOptions.EnableRetryOnFailure();
    })
);

// ✅ Add Services
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // 🔥 Enables UI Hot-Reload

// ✅ Session & Company Context
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<KLENZ.Services.ICompanyContext, KLENZ.Services.CompanyContext>();

// ✅ Identity Configuration
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
})
.AddEntityFrameworkStores<KLENZDbContext>()
.AddDefaultUI()
.AddDefaultTokenProviders();

// ✅ Configure Authentication & Authorization
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.ReturnUrlParameter = "returnUrl";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
});

// ✅ Configure Data Protection (Fixes hardcoded path)
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KLENZKeys")))
    .SetApplicationName("KLENZ");

// ✅ CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ✅ Configure Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// ✅ Map Razor Pages (Identity UI)
app.MapRazorPages();

// ✅ Map Controllers & Routes
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ✅ Ensure Database Schema Migration for Multi-Company FinancialYear
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<KLENZDbContext>();
        db.Database.ExecuteSqlRaw(@"
            IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'Services' AND TABLE_NAME = 'FinancialYear' AND COLUMN_NAME = 'CompanyNameId')
            BEGIN
                ALTER TABLE Services.FinancialYear ADD CompanyNameId INT NULL;
                ALTER TABLE Services.FinancialYear ADD CONSTRAINT FK_FinancialYear_Company FOREIGN KEY (CompanyNameId) REFERENCES Services.Companies(Id);
                EXEC('UPDATE Services.FinancialYear SET CompanyNameId = (SELECT TOP 1 Id FROM Services.Companies WHERE IsActive = 1 ORDER BY Id) WHERE CompanyNameId IS NULL');
            END

            IF NOT EXISTS (SELECT TOP 1 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'Services' AND TABLE_NAME = 'Companies' AND COLUMN_NAME = 'SequenceNo')
            BEGIN
                ALTER TABLE Services.Companies ADD SequenceNo INT NULL;
                EXEC('UPDATE Services.Companies SET SequenceNo = Id WHERE SequenceNo IS NULL');
            END

            -- Create ARCUS company if not already created
            IF NOT EXISTS (SELECT 1 FROM Services.Companies WHERE ShortName = 'ARCUS')
            BEGIN
                INSERT INTO Services.Companies (SequenceNo, FullName, ShortName, IsActive, CreatedDateTime)
                VALUES (2, 'ARCUS', 'ARCUS', 1, GETDATE());
            END

            -- Update sequence numbers as requested:
            -- 1: KLENZ, 2: ARCUS, 3: ZION, 4: GTN
            UPDATE Services.Companies SET SequenceNo = 1 WHERE ShortName = 'KLENZ';
            UPDATE Services.Companies SET SequenceNo = 2 WHERE ShortName = 'ARCUS';
            UPDATE Services.Companies SET SequenceNo = 3 WHERE ShortName = 'ZION' OR FullName LIKE '%ZION%';
            UPDATE Services.Companies SET SequenceNo = 4 WHERE ShortName = 'GTN' OR FullName LIKE '%GTN%';
        ");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error checking/migrating FinancialYear schema.");
    }
}

// ✅ Run Application
app.Run();
