using System;

public class MessageValidation {
    public bool Valid {get; set;}
    public string Message {get; set;}

    public MessageValidation(bool valid, string message) {
        Valid = valid;
        Message = message;
    }
}