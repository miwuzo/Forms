namespace FormsApp.Data
{
    public class AccessRule
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public Template? Template { get; set; }
        public string? Email { get; set; } 
        public string? Role { get; set; } 
    }
} 