using System;
using KimmyEsthi.Admin;
using KimmyEsthi.Appointments;
using KimmyEsthi.ConsentForm;
using KimmyEsthi.Services;
using Microsoft.EntityFrameworkCore;

namespace KimmyEsthi.Db;

public class KimmyEsthiDbContext : DbContext
{
    public KimmyEsthiDbContext(DbContextOptions<KimmyEsthiDbContext> options)
        : base(options) { }

    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<Client.Client> Clients => Set<Client.Client>();
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<ConsentFormStatement> ConsentFormStatements => Set<ConsentFormStatement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>().HasIndex(a => a.DateTime).IsUnique();
        modelBuilder.Ignore<AppointmentRequest>();

        modelBuilder.Entity<ScheduledAppointment>().HasKey(sa => sa.AppointmentId);

        modelBuilder.Entity<Client.Client>().HasKey(c => c.ClientId);
        modelBuilder.Entity<Client.Client>().HasIndex(c => c.Email).IsUnique();

        modelBuilder
            .Entity<Appointment>()
            .HasOne(a => a.ScheduledAppointment)
            .WithOne()
            .HasForeignKey<ScheduledAppointment>(sa => sa.AppointmentId);

        modelBuilder
            .Entity<Appointment>()
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

        modelBuilder
            .Entity<Service>()
            .Property(e => e.ServiceType)
            .HasConversion(
                v => v.ToString(),
                v => (ServiceType)Enum.Parse(typeof(ServiceType), v));

        base.OnModelCreating(modelBuilder);
    }
}
