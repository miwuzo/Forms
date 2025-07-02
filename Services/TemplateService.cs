using FormsApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

public class TemplateService : ITemplateService
{
    private readonly ApplicationDbContext _db;

    public TemplateService(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<Template> GetUserTemplates(string userId)
    {
        return _db.Templates.Where(t => t.AuthorId == userId).ToList();
    }

    public Template? GetTemplateById(int id)
    {
        return _db.Templates
            .Include(t => t.Fields)
            .Include(t => t.Forms)
                .ThenInclude(f => f.User)
            .FirstOrDefault(t => t.Id == id);
    }

    public List<Template> GetAllTemplates()
    {
        return _db.Templates
            .Include(t => t.Fields)
            .Include(t => t.Forms)
                .ThenInclude(f => f.User)
            .Include(t => t.Author)
            .ToList();
    }

    public void SaveTemplate(Template template)
    {
        if (template.Id == 0)
            _db.Templates.Add(template);
        _db.SaveChanges();
    }

    public void DeleteTemplate(int id)
    {
        var template = _db.Templates.Find(id);
        if (template != null)
        {
            _db.Templates.Remove(template);
            _db.SaveChanges();
        }
    }

    public List<Template> GetTemplatesByTag(string tag)
    {
        return _db.Templates.Where(t => t.Tags.Contains(tag)).ToList();
    }

    public List<Template> GetTemplatesByAuthor(string authorId)
    {
        return _db.Templates.Where(t => t.AuthorId == authorId).ToList();
    }

    public List<AccessRule> GetAccessRules(int templateId)
    {
        return _db.AccessRules.Where(r => r.TemplateId == templateId).ToList();
    }

    public void SaveAccessRules(int templateId, string accessType, string emails, string roles)
    {
        var existingRules = _db.AccessRules.Where(r => r.TemplateId == templateId).ToList();
        _db.AccessRules.RemoveRange(existingRules);
        if (accessType == "all")
        {
            _db.AccessRules.Add(new AccessRule { TemplateId = templateId, Email = null, Role = null });
        }
        else if (accessType == "email" && !string.IsNullOrWhiteSpace(emails))
        {
            var emailList = emails.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var email in emailList)
            {
                if (!string.IsNullOrWhiteSpace(email))
                    _db.AccessRules.Add(new AccessRule { TemplateId = templateId, Email = email.Trim(), Role = null });
            }
        }
        else if (accessType == "role" && !string.IsNullOrWhiteSpace(roles))
        {
            var roleList = roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var role in roleList)
            {
                if (!string.IsNullOrWhiteSpace(role))
                    _db.AccessRules.Add(new AccessRule { TemplateId = templateId, Email = null, Role = role.Trim() });
            }
        }
        _db.SaveChanges();
    }
} 