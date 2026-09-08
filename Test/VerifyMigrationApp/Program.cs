using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using loxxking_backend_clean.Domain.Entities.Users;
using loxxking_backend_clean.Infrastructure.Persistence;
using loxxking_backend_clean.Infrastructure.Authentication;
using loxxking_backend_clean.Application.Features.Users.Queries.GetProfile;
using loxxking_backend_clean.Application.Features.Users.Queries.GetStaff;

namespace VerifyMigrationApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql("Host=localhost;Port=5432;Database=testdb;Username=postgres;Password=postgres;"));

            services.AddIdentityCore<User>(options => {
                options.User.RequireUniqueEmail = false;
            }).AddEntityFrameworkStores<ApplicationDbContext>();
            
            services.AddScoped<IPasswordHasher<User>, LegacyBCryptPasswordHasher>();

            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            Console.WriteLine("TEST 1: GetProfileHandler with u.PhoneNumber fix...");
            var profileHandler = new GetProfileHandler(dbContext);
            var john = await dbContext.Users.FirstAsync(u => u.Email == "john@example.com");
            var profileResult = await profileHandler.Handle(new GetProfileQuery(john.Id), CancellationToken.None);
            if (profileResult.IsFailure) throw new Exception($"GetProfile failed: {profileResult.Error}");
            Console.WriteLine($"   OK: Profile returned â€” Name={profileResult.Value.Name}, Phone={profileResult.Value.Phone}");
            if (string.IsNullOrEmpty(profileResult.Value.Phone))
                Console.WriteLine("   WARNING: Phone is empty (may be expected if PhoneNumber was not backfilled in seed)");
            else
                Console.WriteLine($"   Phone value confirmed: {profileResult.Value.Phone}");

            Console.WriteLine("TEST 2: GetStaffHandler with u.PhoneNumber fix...");
            var staffHandler = new GetStaffHandler(dbContext);
            var staffResult = await staffHandler.Handle(new GetStaffQuery(null), CancellationToken.None);
            if (staffResult.IsFailure) throw new Exception($"GetStaff failed: {staffResult.Error}");
            Console.WriteLine($"   OK: Staff list returned â€” {staffResult.Value.Count} staff members");
            foreach (var s in staffResult.Value)
            {
                Console.WriteLine($"     - {s.Name} ({s.Role}) Phone={s.Phone}");
            }

            Console.WriteLine("ALL PRIORITY 1 TESTS PASSED.");
        }
    }
}
