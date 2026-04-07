namespace HealthCare.Application.Dto
{
    public class AlertDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }        
        public string Category { get; set; }
        public bool IsRead { get; set; }
        public string TimeAgo { get; set; }    
        public DateTime CreatedAt { get; set; }
    }
}
