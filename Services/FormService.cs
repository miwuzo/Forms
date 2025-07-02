using FormsApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

public class FormService : IFormService
{
    private readonly ApplicationDbContext _db;

    public FormService(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<Form> GetUserForms(string userId)
    {
        return _db.Forms.Include(f => f.Template).Include(f => f.User).Where(f => f.UserId == userId).ToList();
    }

    public Form? GetFormById(int id)
    {
        return _db.Forms
            .Include(f => f.Template)
                .ThenInclude(t => t.Fields)
            .Include(f => f.Answers)
            .Include(f => f.User)
            .FirstOrDefault(f => f.Id == id);
    }

    public List<Form> GetFormsByTemplateId(int templateId)
    {
        return _db.Forms
            .Include(f => f.Template)
            .Include(f => f.Answers)
            .Include(f => f.User)
            .Where(f => f.TemplateId == templateId)
            .ToList();
    }

    public List<Form> GetAllForms()
    {
        return _db.Forms
            .Include(f => f.Template)
            .Include(f => f.Answers)
            .Include(f => f.User)
            .ToList();
    }

    public void SaveForm(Form form)
    {
        if (form.Id == 0)
            _db.Forms.Add(form);
        _db.SaveChanges();
    }

    public void DeleteForm(int id)
    {
        var form = _db.Forms.Find(id);
        if (form != null)
        {
            _db.Forms.Remove(form);
            _db.SaveChanges();
        }
    }
} 