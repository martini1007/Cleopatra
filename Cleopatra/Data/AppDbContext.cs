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

            // Explicitly configure ProfilePicture as a BLOB for Customer
            modelBuilder.Entity<Customer>()
                .Property(c => c.ProfilePicture)
                .HasColumnType("BLOB");
            
            //Seed(modelBuilder);
        }
        
        // TEMPORARY method for manual seeding before adding migrations to the proj
        public void SeedManually()
        {
            try
            {
            // Seed Services (parent table)
            if (!Services.Any())
            {
                Services.AddRange(
                    new Service { Name = "Haircut", Description = "Basic haircut service", Price = 20.0, Duration = 30 },
                    new Service { Name = "Massage", Description = "Relaxing full body massage", Price = 50.0, Duration = 60 }
                );
                SaveChanges();
            }

            // Seed Products (parent table for Inventory)
            if (!Products.Any())
            {
                Products.AddRange(
                    new Product { Name = "Shampoo", Brand = "BrandA", QuantityInStock = 100, PricePerUnit = 5.0, LastRestockedDate = DateTime.Now },
                    new Product { Name = "Conditioner", Brand = "BrandB", QuantityInStock = 50, PricePerUnit = 6.5, LastRestockedDate = DateTime.Now }
                );
                SaveChanges();
            }

            // Seed Employees (parent table for Appointments and Schedules)
            if (!Employees.Any())
            {
                Employees.AddRange(
                    new Employee { Name = "Alice Johnson", Role = "Manager", Email = "alice@example.com", PhoneNumber = "1231231234", HireDate = DateTime.Now },
                    new Employee { Name = "Bob Brown", Role = "Technician", Email = "bob@example.com", PhoneNumber = "4324324321", HireDate = DateTime.Now }
                );
                SaveChanges();
            }

            // Seed Customers (parent table for Appointments and Notifications)
            if (!Customers.Any())
            {
                Customers.AddRange(
                    new Customer { IdentityUserId = "1", Name = "John Doe", Email = "john@example.com", PhoneNumber = "1111111111", DateOfBirth = new DateTime(1985, 5, 15), CreatedDate = DateTime.Now },
                    new Customer { IdentityUserId = "2", Name = "Jane Smith", Email = "jane@example.com", PhoneNumber = "2222222222", DateOfBirth = new DateTime(1990, 3, 22), CreatedDate = DateTime.Now }
                );
                SaveChanges();
            }

            // Seed Schedules (references Employees)
            if (!Schedules.Any())
            {
                Schedules.AddRange(
                    new Schedule { EmployeeId = Employees.First().EmployeeId, StartDateTime = DateTime.Now.AddHours(1), EndDateTime = DateTime.Now.AddHours(5), BreakTimes = "12:00-12:30" },
                    new Schedule { EmployeeId = Employees.Skip(1).First().EmployeeId, StartDateTime = DateTime.Now.AddHours(2), EndDateTime = DateTime.Now.AddHours(6), BreakTimes = "14:00-14:30" }
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
                    new Appointment { CustomerId = Customers.First().CustomerId, EmployeeId = Employees.First().EmployeeId, ServiceId = Services.First().ServiceId, AppointmentDateTime = DateTime.Now.AddDays(1), Duration = 30, Status = "Confirmed", Notes = "First appointment" },
                    new Appointment { CustomerId = Customers.Skip(1).First().CustomerId, EmployeeId = Employees.Skip(1).First().EmployeeId, ServiceId = Services.Skip(1).First().ServiceId, AppointmentDateTime = DateTime.Now.AddDays(2), Duration = 60, Status = "Pending", Notes = "Second appointment" }
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


        
        // UNCOMMENT (as well as Seed() in the OnModelCreating() method above) after fixing migrations :)
        
        //   private void Seed(ModelBuilder modelBuilder)
        // {
        //     // Customers
        //     modelBuilder.Entity<Customer>().HasData(
        //         new Customer { CustomerId = 1, Name = "John Doe", Email = "johndoe@example.com", PhoneNumber = "1234567890", DateOfBirth = new DateTime(1985, 5, 20), CreatedDate = DateTime.Now },
        //         new Customer { CustomerId = 2, Name = "Jane Smith", Email = "janesmith@example.com", PhoneNumber = "0987654321", DateOfBirth = new DateTime(1990, 8, 15), CreatedDate = DateTime.Now }
        //     );
        //
        //     // Employees
        //     modelBuilder.Entity<Employee>().HasData(
        //         new Employee { EmployeeId = 1, Name = "Alice Johnson", Role = "Manager", Email = "alice@example.com", PhoneNumber = "1111111111", HireDate = DateTime.Now },
        //         new Employee { EmployeeId = 2, Name = "Bob Williams", Role = "Technician", Email = "bob@example.com", PhoneNumber = "2222222222", HireDate = DateTime.Now }
        //     );
        //
        //     // Services
        //     modelBuilder.Entity<Service>().HasData(
        //         new Service { ServiceId = 1, Name = "Haircut", Description = "Basic haircut service", Price = 20.0, Duration = 30 },
        //         new Service { ServiceId = 2, Name = "Hair Coloring", Description = "Full hair coloring service", Price = 100.0, Duration = 120 }
        //     );
        //
        //     // Appointments
        //     modelBuilder.Entity<Appointment>().HasData(
        //         new Appointment { AppointmentId = 1, CustomerId = 1, EmployeeId = 2, ServiceId = 1, AppointmentDateTime = DateTime.Now.AddDays(1), Duration = 30, Status = "Scheduled", Notes = "First-time customer" }
        //     );
        //
        //     // Products
        //     modelBuilder.Entity<Product>().HasData(
        //         new Product { ProductId = 1, Name = "Shampoo", Brand = "BrandX", QuantityInStock = 50, PricePerUnit = 10.0, LastRestockedDate = DateTime.Now.AddDays(-10) },
        //         new Product { ProductId = 2, Name = "Hair Gel", Brand = "BrandY", QuantityInStock = 30, PricePerUnit = 15.0, LastRestockedDate = DateTime.Now.AddDays(-5) }
        //     );
        //
        //     // Inventory
        //     modelBuilder.Entity<Inventory>().HasData(
        //         new Inventory { InventoryId = 1, ProductId = 1, QuantityUsed = 5, UsedDate = DateTime.Now.AddDays(-2) }
        //     );
        //
        //     // Notifications
        //     modelBuilder.Entity<Notification>().HasData(
        //         new Notification { NotificationId = 1, CustomerId = 1, Type = "Email", Message = "Your appointment is confirmed!", SentDate = DateTime.Now, Status = "Sent" }
        //     );
        //
        //     // Reports
        //     modelBuilder.Entity<Report>().HasData(
        //         new Report { ReportId = 1, GeneratedDate = DateTime.Now, Type = "Sales", FilePath = "/reports/sales_report.pdf" }
        //     );
        //
        //     // Schedules
        //     modelBuilder.Entity<Schedule>().HasData(
        //         new Schedule { ScheduleId = 1, EmployeeId = 1, StartDateTime = DateTime.Now.AddDays(1).AddHours(9), EndDateTime = DateTime.Now.AddDays(1).AddHours(17), BreakTimes = "12:00-12:30" }
        //     );
        // }
    }
}
