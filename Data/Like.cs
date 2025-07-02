namespace FormsApp.Data
{
    public class Like
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public Template? Template { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
    }
} 