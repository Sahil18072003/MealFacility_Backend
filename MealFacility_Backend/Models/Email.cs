namespace MealFacility_Backend.Models
{
    public class Email
    {
        public string To { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }

        public Email(string to, string subject, string content)
        {
            To = to;
            Subject = subject;
            Content = content;
        }
    }
}
