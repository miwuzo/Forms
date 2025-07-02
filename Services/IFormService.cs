using FormsApp.Data;
using System.Collections.Generic;

public interface IFormService
{
    List<Form> GetUserForms(string userId);
    Form? GetFormById(int id);
    List<Form> GetFormsByTemplateId(int templateId);
    void SaveForm(Form form);
    void DeleteForm(int id);
    List<Form> GetAllForms();
} 