using Microsoft.EntityFrameworkCore;

public class AppointmentContext : DbContext
{
    public DbSet<Appointment> nonbooked_appointments { get; set; }

    public string DbPath { get; }

    public AppointmentContext()
    {
        var folder = Environment.SpecialFolder.LocalApplicationData;
        var path = Environment.GetFolderPath(folder);
        DbPath = System.IO.Path.Join(path, "db1.db");
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source=db1.db");
}
