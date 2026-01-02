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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<AppointmentRequest>();

        modelBuilder.Entity<ScheduledAppointment>().HasKey(sa => sa.AppointmentId);

        modelBuilder
            .Entity<Appointment.Appointment>()
            .HasOne(a => a.ScheduledAppointment)
            .WithOne()
            .HasForeignKey<ScheduledAppointment>(sa => sa.AppointmentId);

        base.OnModelCreating(modelBuilder);
    }
}
