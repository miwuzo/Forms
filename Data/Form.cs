using System;
using System.Collections.Generic;

namespace FormsApp.Data
{
    public class Form
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public Template? Template { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        public DateTime FilledAt { get; set; } = DateTime.UtcNow;
        public ICollection<FormAnswer> Answers { get; set; } = new List<FormAnswer>();
    }
} 