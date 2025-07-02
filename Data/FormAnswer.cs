namespace FormsApp.Data
{
    public class FormAnswer
    {
        public int Id { get; set; }
        public int FormId { get; set; }
        public Form? Form { get; set; }
        public int FieldId { get; set; }
        public FormField? Field { get; set; }
        public string Value { get; set; } = string.Empty;
    }
} 