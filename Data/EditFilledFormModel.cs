using System.Collections.Generic;

namespace FormsApp.Data
{
    public class EditFilledFormModel
    {
        public Dictionary<int, string> StringAnswers { get; set; } = new();
        public Dictionary<int, int> IntAnswers { get; set; } = new();
        public Dictionary<int, bool> BoolAnswers { get; set; } = new();
    }
} 