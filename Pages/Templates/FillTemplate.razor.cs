namespace FormsApp.Pages.Templates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FormsApp.Data;
    using Microsoft.AspNetCore.Components;

    public partial class FillTemplate : ComponentBase
    {
        [Parameter] public int Id { get; set; }
        protected Template Template = new() { Fields = new List<FormField>() };
        protected Dictionary<int, string> StringAnswers = new();
        protected Dictionary<int, int> IntAnswers = new();
        protected Dictionary<int, bool> BoolAnswers = new();
        protected string? userId;
        protected bool hasAccess = false;
        protected string? accessError = null;
        protected bool hasExistingForm = false;
        protected bool canEdit = false;
        protected bool isAdmin = false;

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            if (!authState.User.Identity?.IsAuthenticated ?? true)
            {
                Navigation.NavigateTo("/Identity/Account/Login", true);
                return;
            }
            Template = TemplateService.GetTemplateById(Id) ?? Template;
            if (Template.Id == 0)
            {
                accessError = Loc["TemplateNotFound"];
                return;
            }
            var user = authState.User;
            userId = user.Identity?.IsAuthenticated == true ? user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value : null;
            canEdit = user.Identity?.IsAuthenticated == true && (userId == Template.AuthorId || user.Claims.Any(c => c.Type == "role" && c.Value == "Admin"));
            isAdmin = user.Claims.Any(c => c.Type == "role" && c.Value == "Admin");
            hasAccess = await CheckAccessToTemplate(Template.Id, user);
            if (!hasAccess)
            {
                accessError = Loc["NoAccessToFill"];
                return;
            }
            if (userId != null)
            {
                var existingForm = FormService.GetFormsByTemplateId(Id).FirstOrDefault(f => f.UserId == userId);
                if (existingForm != null)
                {
                    hasExistingForm = true;
                    foreach (var answer in existingForm.Answers)
                    {
                        var field = Template.Fields.FirstOrDefault(f => f.Id == answer.FieldId);
                        if (field != null)
                        {
                            switch (field.Type)
                            {
                                case FieldType.Checkbox:
                                    BoolAnswers[field.Id] = answer.Value == "true";
                                    break;
                                case FieldType.Integer:
                                    IntAnswers[field.Id] = int.TryParse(answer.Value, out var i) ? i : 0;
                                    break;
                                default:
                                    StringAnswers[field.Id] = answer.Value;
                                    break;
                            }
                        }
                    }
                }
            }
            foreach (var field in Template.Fields)
            {
                switch (field.Type)
                {
                    case FieldType.Checkbox:
                        if (!BoolAnswers.ContainsKey(field.Id)) BoolAnswers[field.Id] = false;
                        break;
                    case FieldType.Integer:
                        if (!IntAnswers.ContainsKey(field.Id)) IntAnswers[field.Id] = 0;
                        break;
                    default:
                        if (!StringAnswers.ContainsKey(field.Id)) StringAnswers[field.Id] = string.Empty;
                        break;
                }
            }
        }

        protected async Task<bool> CheckAccessToTemplate(int templateId, System.Security.Claims.ClaimsPrincipal user)
        {
            var accessRules = TemplateService.GetAccessRules(templateId);
            if (!accessRules.Any())
                return true;
            if (accessRules.Any(r => r.Email == null && r.Role == null))
                return true;
            var userEmail = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(userEmail) && accessRules.Any(r => r.Email == userEmail))
                return true;
            var userRoles = user.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value);
            if (userRoles.Any() && accessRules.Any(r => r.Role != null && userRoles.Contains(r.Role)))
                return true;
            var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var template = TemplateService.GetTemplateById(templateId);
                if (template?.AuthorId == userId)
                    return true;
            }
            if (user.Claims.Any(c => c.Type == "role" && c.Value == "Admin"))
                return true;
            return false;
        }

        protected void OnSubmit()
        {
            if (userId == null || !hasAccess)
                return;
            
            var existingForm = FormService.GetFormsByTemplateId(Id).FirstOrDefault(f => f.UserId == userId);
            
            Form form;
            if (existingForm != null)
            {
                form = existingForm;
                form.FilledAt = DateTime.UtcNow;
                
                foreach (var field in Template.Fields)
                {
                    var existingAnswer = form.Answers.FirstOrDefault(a => a.FieldId == field.Id);
                    string answerValue = field.Type switch
                    {
                        FieldType.Checkbox => (BoolAnswers.TryGetValue(field.Id, out var b) && b) ? "true" : "false",
                        FieldType.Integer => IntAnswers.TryGetValue(field.Id, out var i) ? i.ToString() : "0",
                        _ => StringAnswers.TryGetValue(field.Id, out var s) ? s : string.Empty
                    };
                    
                    if (existingAnswer != null)
                    {
                        existingAnswer.Value = answerValue;
                    }
                    else
                    {
                        form.Answers.Add(new FormAnswer
                        {
                            FieldId = field.Id,
                            Value = answerValue
                        });
                    }
                }
            }
            else
            {
                form = new Form
                {
                    TemplateId = Id,
                    UserId = userId,
                    FilledAt = DateTime.UtcNow,
                    Answers = new List<FormAnswer>()
                };
                
                foreach (var field in Template.Fields)
                {
                    string answerValue = field.Type switch
                    {
                        FieldType.Checkbox => (BoolAnswers.TryGetValue(field.Id, out var b) && b) ? "true" : "false",
                        FieldType.Integer => IntAnswers.TryGetValue(field.Id, out var i) ? i.ToString() : "0",
                        _ => StringAnswers.TryGetValue(field.Id, out var s) ? s : string.Empty
                    };
                    form.Answers.Add(new FormAnswer
                    {
                        FieldId = field.Id,
                        Value = answerValue
                    });
                }
                FormService.SaveForm(form);
            }
            
            var formId = form.Id;
            Navigation.NavigateTo($"/filled/{formId}");
        }

        public Task HandleSubmit()
        {
            OnSubmit();
            return Task.CompletedTask;
        }

        [Inject] protected Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
        [Inject] protected NavigationManager Navigation { get; set; } = default!;
        [Inject] protected LocalizationService Loc { get; set; } = default!;
        [Inject] protected ITemplateService TemplateService { get; set; } = default!;
        [Inject] protected IFormService FormService { get; set; } = default!;
    }
} 