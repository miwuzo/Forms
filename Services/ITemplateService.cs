using FormsApp.Data;
using System.Collections.Generic;

public interface ITemplateService
{
    List<Template> GetUserTemplates(string userId);
    Template? GetTemplateById(int id);
    List<Template> GetAllTemplates();
    void SaveTemplate(Template template);
    void DeleteTemplate(int id);
    List<Template> GetTemplatesByTag(string tag);
    List<Template> GetTemplatesByAuthor(string authorId);
    List<AccessRule> GetAccessRules(int templateId);
    void SaveAccessRules(int templateId, string accessType, string emails, string roles);
} 