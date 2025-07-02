using System.ComponentModel.DataAnnotations.Schema;

namespace FormsApp.Data
{
    public enum FieldType
    {
        Checkbox,
        Integer,
        String,
        MultilineText
    }

    public class FormField
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public Template? Template { get; set; }
        public FieldType Type { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool ShowInResults { get; set; } = true;
        public int Order { get; set; }
    }
} 