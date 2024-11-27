public class Appointment {

    private int _id;
    private string _date;
    private string _time;
    private string _status;

    /*
    public Appointment(int id, string date, string time, string status) {
        _id = id;
        _date = date;
        _time = time;
        _status = status;
    }
    */

    public int Id { get { return _id; } }
    public string Date { get { return _date; } }
    public string Time { get { return _time; } }
    public string Status { get { return _status; } }
}
    
