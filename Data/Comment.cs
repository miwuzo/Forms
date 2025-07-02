using System;

namespace FormsApp.Data
{
    public class Comment
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public Template? Template { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 