using System;

public class Message {
    public bool IsBooking { get; set; }
    public bool IsCancellation { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public Tour SelectedTour { get; set; }

    public class Tour {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}