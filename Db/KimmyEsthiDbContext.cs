using KimmyEsthi.Admin;
using KimmyEsthi.Appointment;
using Microsoft.EntityFrameworkCore;

namespace KimmyEsthi.Db;

public class KimmyEsthiDbContext : DbContext
{
    public KimmyEsthiDbContext(DbContextOptions<KimmyEsthiDbContext> options)
        : base(options) { }

    public DbSet<Appointment.Appointment> Appointments => Set<Appointment.Appointment>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<Client.Client> Clients => Set<Client.Client>();
    public DbSet<Promotion> Promotions => Set<Promotion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment.Appointment>().HasIndex(a => a.DateTime).IsUnique();
        modelBuilder.Ignore<AppointmentRequest>();

        modelBuilder.Entity<ScheduledAppointment>().HasKey(sa => sa.AppointmentId);

        modelBuilder.Entity<Client.Client>().HasKey(c => c.ClientId);
        modelBuilder.Entity<Client.Client>().HasIndex(c => c.Email).IsUnique();

        modelBuilder
            .Entity<Appointment.Appointment>()
            .HasOne(a => a.ScheduledAppointment)
            .WithOne()
            .HasForeignKey<ScheduledAppointment>(sa => sa.AppointmentId);

        modelBuilder
            .Entity<Appointment.Appointment>()
            .HasOne(a => a.Promotion)
            .WithOne()
            .HasForeignKey<Promotion>(p => p.AppointmentId);

        modelBuilder
            .Entity<ScheduledAppointment>()
            .HasOne(a => a.Client)
            .WithMany()
            .HasForeignKey(x => x.ClientId);

        modelBuilder
            .Entity<Client.Client>()
            .HasOne(x => x.ConsentForm)
            .WithOne()
            .HasForeignKey<ConsentForm.ConsentForm>(x => x.ClientId);

        base.OnModelCreating(modelBuilder);
    }
}
