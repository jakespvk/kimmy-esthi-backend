using System.Collections.Generic;

public class AppointmentCollection
{
    public static List<Appointment> appointments = new List<Appointment>();

    /*public static void Init()
    {
        using var db = new AppointmentContext();
        appointments = db.Appointments;
        /*appointments.Add(new Appointment(1, "11/13/2024", "8:30 AM", "Booked"));
        appointments.Add(new Appointment(2, "11/14/2024", "9:30 AM", "Available"));
        appointments.Add(new Appointment(3, "11/14/2024", "10:30 AM", "Booked"));
    }

    public void AddNewAppointment(Appointment appointment)
    {
        appointments.Add(appointment);
    }

    public static IEnumerable<Appointment> GetAppointmentsForGivenDay(string apptDate)
    {
        var result = from appointment in appointments
            where appointment.Date == apptDate
            select appointment;

        return result;
    }*/
}
