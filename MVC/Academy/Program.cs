using Academy.Data;
using Microsoft.EntityFrameworkCore;
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
string connectionString = builder.Configuration.GetConnectionString("AcademyContext")
	?? throw new InvalidOperationException("Не задана строка подключения AcademyContext.");
builder.Services.AddDbContext<AcademyContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddControllersWithViews();
WebApplication app = builder.Build();
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
public partial class Program { }
