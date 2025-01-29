using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Cleopatra.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        // DbSets for all tables
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Service> Services { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Define relationships
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Customer)
                .WithMany()
                .HasForeignKey(a => a.CustomerId);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Employee)
                .WithMany()
                .HasForeignKey(a => a.EmployeeId);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Service)
                .WithMany()
                .HasForeignKey(a => a.ServiceId);

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Customer)
                .WithMany()
                .HasForeignKey(n => n.CustomerId);

            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.Employee)
                .WithMany()
                .HasForeignKey(s => s.EmployeeId);
            
            
        }
        
        public async Task SeedManually(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        { 
            // Seed Roles
            var roles = new[] { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            if (!Employees.Any())
            {
                var employees = new List<Employee>
                {
                    new Employee { Name = "Alice Johnson", Role = "Manager", Email = "alice@example.com", PhoneNumber = "1231231234", HireDate = DateTime.Now },
                    new Employee { Name = "Bob Brown", Role = "Technician", Email = "bob@example.com", PhoneNumber = "4324324321", HireDate = DateTime.Now }
                };

                foreach (var employee in employees)
                {
                    var identityUser = new ApplicationUser
                    {
                        UserName = employee.Email,
                        Email = employee.Email,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(identityUser, "Admin123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(identityUser, "Admin");
                        employee.IdentityUserId = identityUser.Id; // Set IdentityUserId
                    }
                }

                Employees.AddRange(employees);
                SaveChanges();
            }

            if (!Customers.Any())
            {
                var customers = new List<Customer>
                {
                    new Customer { Name = "John Doe", Email = "john@example.com", PhoneNumber = "1111111111", DateOfBirth = DateTime.Now, CreatedDate = DateTime.Now },
                    new Customer { Name = "Jane Smith", Email = "jane@example.com", PhoneNumber = "2222222222", DateOfBirth = DateTime.Now, CreatedDate = DateTime.Now }
                };

                foreach (var customer in customers)
                {
                    var identityUser = new ApplicationUser
                    {
                        UserName = customer.Email,
                        Email = customer.Email,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(identityUser, "User123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(identityUser, "User");
                        customer.IdentityUserId = identityUser.Id; // Set IdentityUserId
                    }
                }

                Customers.AddRange(customers);
                SaveChanges();
            }
      
            try
            {
            // Seed Services (parent table)
            if (!Services.Any())
            {
                Services.AddRange(
                    new Service { Name = "Haircut", Description = "Basic haircut service", Price = 20.0 },
                    new Service { Name = "Massage", Description = "Relaxing full body massage", Price = 50.0}
                );
                SaveChanges();
            }

            // Seed Products (parent table for Inventory)
            if (!Products.Any())
            {
                Products.AddRange(
                    new Product
                    {
                        Name = "Shampoo", Brand = "BrandA", QuantityInStock = 100, PricePerUnit = 5.0,
                        LastRestockedDate = DateTime.Now
                    },
                    new Product
                    {
                        Name = "Conditioner", Brand = "BrandB", QuantityInStock = 50, PricePerUnit = 6.5,
                        LastRestockedDate = DateTime.Now
                    }
                );
                SaveChanges();
            }

            // Seed Schedules (references Employees)
            if (!Schedules.Any())
            {
                Schedules.AddRange(
                    new Schedule { EmployeeId = Employees.First().EmployeeId, StartDateTime = DateTime.Now.AddHours(1), EndDateTime = DateTime.Now.AddHours(5)},
                    new Schedule { EmployeeId = Employees.Skip(1).First().EmployeeId, StartDateTime = DateTime.Now.AddHours(2), EndDateTime = DateTime.Now.AddHours(6)}
                );
                SaveChanges();
            }

            // Seed Inventory (references Products)
            if (!Inventories.Any())
            {
                Inventories.AddRange(
                    new Inventory { ProductId = Products.First().ProductId, QuantityUsed = 10, UsedDate = DateTime.Now },
                    new Inventory { ProductId = Products.Skip(1).First().ProductId, QuantityUsed = 5, UsedDate = DateTime.Now }
                );
                SaveChanges();
            }

            // Seed Appointments (references Customers, Employees, and Services)
            if (!Appointments.Any())
            {
                Appointments.AddRange(
                    new Appointment { CustomerId = Customers.First().CustomerId, EmployeeId = Employees.First().EmployeeId, ServiceId = Services.First().ServiceId, AppointmentDateTime = DateTime.Now.AddDays(1), Status = "Confirmed", Notes = "First appointment" },
                    new Appointment { CustomerId = Customers.Skip(1).First().CustomerId, EmployeeId = Employees.Skip(1).First().EmployeeId, ServiceId = Services.Skip(1).First().ServiceId, AppointmentDateTime = DateTime.Now.AddDays(2),  Status = "Pending", Notes = "Second appointment" }
                );
                SaveChanges();
            }

            // Seed Notifications (references Customers)
            if (!Notifications.Any())
            {
                Notifications.AddRange(
                    new Notification { CustomerId = Customers.First().CustomerId, Type = "Email", Message = "Welcome email sent.", SentDate = DateTime.Now, Status = "Sent" },
                    new Notification { CustomerId = Customers.Skip(1).First().CustomerId, Type = "SMS", Message = "Appointment reminder sent.", SentDate = DateTime.Now, Status = "Sent" }
                );
                SaveChanges();
            }

            // Seed Reports (independent table)
            if (!Reports.Any())
            {
                Reports.AddRange(
                    new Report { GeneratedDate = DateTime.Now, Type = "Monthly Sales", FilePath = "/reports/sales_march.pdf" },
                    new Report { GeneratedDate = DateTime.Now, Type = "Inventory Check", FilePath = "/reports/inventory_check.pdf" }
                );
                SaveChanges();
            }

            Console.WriteLine("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during seeding: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
        }
    }
    }
}
