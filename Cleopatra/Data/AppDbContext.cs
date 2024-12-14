using Microsoft.EntityFrameworkCore;

namespace Cleopatra.Data
{
    public class AppDbContext : DbContext
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
            
            Seed(modelBuilder);
        }
        
          private void Seed(ModelBuilder modelBuilder)
        {
            // Customers
            modelBuilder.Entity<Customer>().HasData(
                new Customer { CustomerId = 1, Name = "John Doe", Email = "johndoe@example.com", PhoneNumber = "1234567890", DateOfBirth = new DateTime(1985, 5, 20), CreatedDate = DateTime.Now },
                new Customer { CustomerId = 2, Name = "Jane Smith", Email = "janesmith@example.com", PhoneNumber = "0987654321", DateOfBirth = new DateTime(1990, 8, 15), CreatedDate = DateTime.Now }
            );

            // Employees
            modelBuilder.Entity<Employee>().HasData(
                new Employee { EmployeeId = 1, Name = "Alice Johnson", Role = "Manager", Email = "alice@example.com", PhoneNumber = "1111111111", HireDate = DateTime.Now },
                new Employee { EmployeeId = 2, Name = "Bob Williams", Role = "Technician", Email = "bob@example.com", PhoneNumber = "2222222222", HireDate = DateTime.Now }
            );

            // Services
            modelBuilder.Entity<Service>().HasData(
                new Service { ServiceId = 1, Name = "Haircut", Description = "Basic haircut service", Price = 20.0, Duration = 30 },
                new Service { ServiceId = 2, Name = "Hair Coloring", Description = "Full hair coloring service", Price = 100.0, Duration = 120 }
            );

            // Appointments
            modelBuilder.Entity<Appointment>().HasData(
                new Appointment { AppointmentId = 1, CustomerId = 1, EmployeeId = 2, ServiceId = 1, AppointmentDateTime = DateTime.Now.AddDays(1), Duration = 30, Status = "Scheduled", Notes = "First-time customer" }
            );

            // Products
            modelBuilder.Entity<Product>().HasData(
                new Product { ProductId = 1, Name = "Shampoo", Brand = "BrandX", QuantityInStock = 50, PricePerUnit = 10.0, LastRestockedDate = DateTime.Now.AddDays(-10) },
                new Product { ProductId = 2, Name = "Hair Gel", Brand = "BrandY", QuantityInStock = 30, PricePerUnit = 15.0, LastRestockedDate = DateTime.Now.AddDays(-5) }
            );

            // Inventory
            modelBuilder.Entity<Inventory>().HasData(
                new Inventory { InventoryId = 1, ProductId = 1, QuantityUsed = 5, UsedDate = DateTime.Now.AddDays(-2) }
            );

            // Notifications
            modelBuilder.Entity<Notification>().HasData(
                new Notification { NotificationId = 1, CustomerId = 1, Type = "Email", Message = "Your appointment is confirmed!", SentDate = DateTime.Now, Status = "Sent" }
            );

            // Reports
            modelBuilder.Entity<Report>().HasData(
                new Report { ReportId = 1, GeneratedDate = DateTime.Now, Type = "Sales", FilePath = "/reports/sales_report.pdf" }
            );

            // Schedules
            modelBuilder.Entity<Schedule>().HasData(
                new Schedule { ScheduleId = 1, EmployeeId = 1, StartDateTime = DateTime.Now.AddDays(1).AddHours(9), EndDateTime = DateTime.Now.AddDays(1).AddHours(17), BreakTimes = "12:00-12:30" }
            );
        }
    }
}
