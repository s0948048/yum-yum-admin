using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using yum_admin.Models;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddCors(options =>
{
    // 可以新增多個來源
    options.AddPolicy("LiveServer",
        policy =>
        {
            policy.WithOrigins("http://127.0.0.1:5500")  // 不要加斜線。
                                .AllowAnyHeader()
                                .AllowAnyMethod();
        });
});


// EF  等DBCONTEXT命名出來就可以引用了。 
builder.Services.AddDbContext<YumyumdbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("YumYumDB")));


// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.Encoder =
            JavaScriptEncoder.Create(
                UnicodeRanges.BasicLatin,
                UnicodeRanges.CjkUnifiedIdeographs
            );
        options.JsonSerializerOptions.WriteIndented = true;
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors("LiveServer");

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=home}/{action=Index}/{id?}");

app.Run();
