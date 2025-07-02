using System;
using System.Collections.Generic;

namespace FormsApp.Data
{
    public class Template
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty; 
        public string AuthorId { get; set; } = string.Empty;
        public ApplicationUser? Author { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<FormField> Fields { get; set; } = new List<FormField>();
        public ICollection<Form> Forms { get; set; } = new List<Form>();
        public bool IsPublic { get; set; } = false;
    }
} 