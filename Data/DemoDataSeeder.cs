using System;
using System.Collections.Generic;
using System.Linq;

namespace FormsApp.Data
{
    public static class DemoDataSeeder
    {
        public static void Seed(ApplicationDbContext db)
        {
            if (db.Templates.Any()) return;

            var user1 = new ApplicationUser { Id = "user1", Email = "user1@mail.com", Role = "Admin" };
            var user2 = new ApplicationUser { Id = "user2", Email = "user2@mail.com", Role = "User" };
            db.Users.AddRange(user1, user2);

            var template1 = new Template
            {
                Title = "Анкета для IT",
                Description = "Опрос для IT-специалистов",
                Topic = "IT",
                Tags = "it,разработка,опрос",
                AuthorId = user1.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                Fields = new List<FormField>
                {
                    new FormField { Type = FieldType.String, Label = "Ваше имя", Order = 1 },
                    new FormField { Type = FieldType.Integer, Label = "Стаж (лет)", Order = 2 },
                    new FormField { Type = FieldType.Checkbox, Label = "Удалёнка?", Order = 3 },
                    new FormField { Type = FieldType.MultilineText, Label = "Комментарий", Order = 4 }
                }
            };
            var template2 = new Template
            {
                Title = "Опрос студентов",
                Description = "Анкета для студентов вузов",
                Topic = "Образование",
                Tags = "студенты,образование,опрос",
                AuthorId = user2.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                Fields = new List<FormField>
                {
                    new FormField { Type = FieldType.String, Label = "ФИО", Order = 1 },
                    new FormField { Type = FieldType.Integer, Label = "Курс", Order = 2 },
                    new FormField { Type = FieldType.String, Label = "Специальность", Order = 3 },
                    new FormField { Type = FieldType.MultilineText, Label = "Пожелания", Order = 4 }
                }
            };
            db.Templates.AddRange(template1, template2);
            db.SaveChanges();

            var form1 = new Form
            {
                TemplateId = template1.Id,
                UserId = user2.Id,
                FilledAt = DateTime.UtcNow.AddDays(-1),
                Answers = new List<FormAnswer>
                {
                    new FormAnswer { FieldId = template1.Fields.ElementAt(0).Id, Value = "Иван" },
                    new FormAnswer { FieldId = template1.Fields.ElementAt(1).Id, Value = "3" },
                    new FormAnswer { FieldId = template1.Fields.ElementAt(2).Id, Value = "true" },
                    new FormAnswer { FieldId = template1.Fields.ElementAt(3).Id, Value = "Работаю в IT" }
                }
            };
            db.Forms.Add(form1);
            db.SaveChanges();
        }
    }
} 