using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using MyShop.BusinessLogic.Models;
using MyShop.BusinessLogic.Repositories;
using MyShop.DataAccess.Data;
using MyShop.DataAccess.Implementation;
using MyShop.Presentation.Utilities;
using Stripe;


using Microsoft.AspNetCore.Identity.EntityFrameworkCore;





namespace MyShop.Presentation
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddControllersWithViews();
			builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
			builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(

				builder.Configuration.GetConnectionString("DefaultConnection")
				));
			builder.Services.Configure<StripeData>(builder.Configuration.GetSection("stripe"));

            builder.Services.AddIdentity<IdentityUser, IdentityRole>(
				 options => options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(2))
                   .AddEntityFrameworkStores<ApplicationDbContext>()
                   .AddDefaultTokenProviders().AddDefaultUI();



            builder.Services.AddSingleton<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IUniteOfWork, UnitOfWork>();


			builder.Services.AddDistributedMemoryCache();
			builder.Services.AddSession();

            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();

            var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (!app.Environment.IsDevelopment())
			{
				app.UseExceptionHandler("/Home/Error");
			}

            app.UseHttpsRedirection();
            app.UseStaticFiles();

			app.UseRouting();

			StripeConfiguration.ApiKey = builder.Configuration.GetSection("stripe:SecretKey").Get<string>();


            app.UseAuthentication();

			app.UseAuthorization();

			app.UseSession();	

            app.MapRazorPages();
            app.MapControllers();

            app.MapControllerRoute(
				name: "default",
				pattern: "{area=Admin}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "Customer",
                pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

            

            app.Run();
		}
	}
}
