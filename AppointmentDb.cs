using kimmy_esthi_backend;
using Microsoft.EntityFrameworkCore;

public class AppointmentDb : DbContext
{
    public AppointmentDb(DbContextOptions<AppointmentDb> options)
        : base(options) { }

    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<ScheduledAppointment> ScheduledAppointments => Set<ScheduledAppointment>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ScheduledAppointment>()
            .HasKey(sa => sa.AppointmentId);

        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.ScheduledAppointment)
            .WithOne()
            .HasForeignKey<ScheduledAppointment>(sa => sa.AppointmentId);

        base.OnModelCreating(modelBuilder);
    }
}
