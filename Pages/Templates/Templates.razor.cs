using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using FormsApp.Data;

namespace FormsApp.Pages.Templates
{
    public partial class Templates : ComponentBase
    {
        [Parameter] public int? Id { get; set; }
        private Template Template = new() { Fields = new List<FormField>(), Forms = new List<Form>() };
        private string accessType = "all";
        private string emails = string.Empty;
        private string roles = string.Empty;
        private bool canEdit = false;
        private int? dragFieldId = null;
        private string? validationError;
        private string activeTab = "general";
        private FieldType newFieldType = FieldType.String;
        private bool hasAccessToFill = false;
        private bool hasAccessToView = false;
        private bool isInitialized = false;
        
        private string sortColumn = "FilledAt";
        private bool sortAscending = false;
        private List<Form> sortedForms = new();
        private string? userId;

        [Inject] private LocalizationService Loc { get; set; }
        [Inject] private NavigationManager Navigation { get; set; }
        [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; }
        [Inject] private Microsoft.Extensions.Localization.IStringLocalizer<SharedResource> L { get; set; }
        [Inject] private ITemplateService TemplateService { get; set; }
        [Inject] private IFormService FormService { get; set; }
        [Inject] private IUserService UserService { get; set; }
        [Inject] private ICommentService CommentService { get; set; }
        [Inject] private ILikeService LikeService { get; set; }
        [Inject] private ITagService TagService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            if (!authState.User.Identity?.IsAuthenticated ?? true)
            {
                Navigation.NavigateTo("/Identity/Account/Login", true);
                return;
            }
            
            var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
            if (QueryHelpers.ParseQuery(uri.Query).TryGetValue("tab", out var tabValues))
            {
                var tab = tabValues.FirstOrDefault();
                if (!string.IsNullOrEmpty(tab) && new[] { "general", "access", "results", "questions", "analytics" }.Contains(tab))
                {
                    activeTab = tab;
                }
            }
            
            if (Id.HasValue)
            {
                Template = TemplateService.GetTemplateById(Id.Value) ?? Template;
                
                if (Template.Id > 0)
                {
                    hasAccessToView = await CheckAccessToTemplate(Template.Id, authState.User);
                    if (!hasAccessToView)
                    {
                        return;
                    }
                    var accessRules = TemplateService.GetAccessRules(Template.Id);
                    if (accessRules.Any())
                    {
                        if (accessRules.Any(r => r.Email == null && r.Role == null))
                        {
                            accessType = "all";
                            emails = string.Empty;
                            roles = string.Empty;
                        }
                        else if (accessRules.All(r => r.Email != null && r.Role == null))
                        {
                            accessType = "email";
                            emails = string.Join(", ", accessRules.Where(r => !string.IsNullOrWhiteSpace(r.Email)).Select(r => r.Email));
                            roles = string.Empty;
                        }
                        else if (accessRules.All(r => r.Role != null && r.Email == null))
                        {
                            accessType = "role";
                            roles = string.Join(", ", accessRules.Where(r => !string.IsNullOrWhiteSpace(r.Role)).Select(r => r.Role));
                            emails = string.Empty;
                        }
                        else
                        {
                            accessType = "all";
                            emails = string.Empty;
                            roles = string.Empty;
                        }
                    }
                }
                
                var user = authState.User;
                userId = user.Identity?.IsAuthenticated == true ? user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value : null;
                canEdit = user.Identity?.IsAuthenticated == true && (userId == Template.AuthorId || user.Claims.Any(c => c.Type == "role" && c.Value == "Admin"));
                
                hasAccessToFill = await CheckAccessToTemplate(Template.Id, user);
                
                if (Template.Id > 0 && !canEdit && hasAccessToFill)
                {
                    Navigation.NavigateTo($"/fill/{Template.Id}", true);
                    return;
                }
                
                ApplySorting();
            }
            
            isInitialized = true;
        }

        private void SetTab(string tab) 
        {
            activeTab = tab;
        }

        private void SaveTemplate()
        {
            TemplateService.SaveTemplate(Template);
            TagService.SyncTagsFromTemplates();
            validationError = "Изменения сохранены";
            Navigation.NavigateTo("/user?tab=templates");
        }

        private void CreateTemplate()
        {
            TemplateService.SaveTemplate(Template);
            TagService.SyncTagsFromTemplates();
            validationError = "Шаблон создан";
            Navigation.NavigateTo("/user?tab=templates");
        }

        private void SaveAccess()
        {
            TemplateService.SaveAccessRules(Template.Id, accessType, emails, roles);
            validationError = "OK";
        }

        private void AddField()
        {
            if (CanAddField(newFieldType))
            {
                var newField = new FormField { Type = newFieldType, Label = "Новый вопрос", Order = Template.Fields.Count + 1 };
                Template.Fields.Add(newField);
                if (Template.Id > 0)
                {
                    TemplateService.SaveTemplate(Template);
                }
            }
        }

        private bool CanAddField(FieldType type)
        {
            return Template.Fields.Count(f => f.Type == type) < 4;
        }

        private void RemoveField(int id)
        {
            var field = Template.Fields.FirstOrDefault(f => f.Id == id);
            if (field != null)
            {
                Template.Fields.Remove(field);
                
                var orderedFields = Template.Fields.OrderBy(f => f.Order).ToList();
                for (int i = 0; i < orderedFields.Count; i++)
                {
                    orderedFields[i].Order = i + 1;
                }
                Template.Fields = orderedFields;
                
                if (Template.Id > 0)
                {
                    TemplateService.SaveTemplate(Template);
                }
            }
        }

        private void OnDragStart(int id)
        {
            dragFieldId = id;
        }

        private void OnDragOver()
        {
        }

        private void OnDrop(int id)
        {
            if (!canEdit || !dragFieldId.HasValue || dragFieldId.Value == id) 
            {
                dragFieldId = null;
                return;
            }

            try
            {
                var fields = Template.Fields.ToList();
                var fromIndex = fields.FindIndex(f => f.Id == dragFieldId.Value);
                var toIndex = fields.FindIndex(f => f.Id == id);
                
                if (fromIndex >= 0 && toIndex >= 0)
                {
                    var item = fields[fromIndex];
                    fields.RemoveAt(fromIndex);
                    fields.Insert(toIndex, item);
                    
                    for (int i = 0; i < fields.Count; i++)
                    {
                        fields[i].Order = i + 1;
                    }
                    
                    Template.Fields = fields;
                    
                    if (Template.Id > 0)
                    {
                        TemplateService.SaveTemplate(Template);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in drag & drop: {ex.Message}");
            }
            
            dragFieldId = null;
        }

        private async Task<bool> CheckAccessToTemplate(int templateId, System.Security.Claims.ClaimsPrincipal user)
        {
            var accessRules = TemplateService.GetAccessRules(templateId);
            
            if (!accessRules.Any())
            {
                return true;
            }
            
            if (accessRules.Any(r => r.Email == null && r.Role == null))
            {
                return true;
            }
            
            var userEmail = user.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            if (!string.IsNullOrEmpty(userEmail) && accessRules.Any(r => r.Email == userEmail))
            {
                return true;
            }
            
            var userRoles = user.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value);
            if (userRoles.Any() && accessRules.Any(r => r.Role != null && userRoles.Contains(r.Role)))
            {
                return true;
            }
            
            var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                var template = TemplateService.GetTemplateById(templateId);
                if (template?.AuthorId == userId)
                {
                    return true;
                }
            }
            
            if (user.Claims.Any(c => c.Type == "role" && c.Value == "Admin"))
            {
                return true;
            }
            
            return false;
        }
        
        private void SortResults(string column)
        {
            if (sortColumn == column)
            {
                sortAscending = !sortAscending;
            }
            else
            {
                sortColumn = column;
                sortAscending = true;
            }
            
            ApplySorting();
        }
        
        private void ApplySorting()
        {
            if (Template.Forms == null || !Template.Forms.Any())
            {
                sortedForms = new List<Form>();
                return;
            }
            
            var query = Template.Forms.AsQueryable();
            
            switch (sortColumn)
            {
                case "UserName":
                    query = sortAscending 
                        ? query.OrderBy(f => f.User != null ? f.User.UserName : f.UserId)
                        : query.OrderByDescending(f => f.User != null ? f.User.UserName : f.UserId);
                    break;
                case "FilledAt":
                    query = sortAscending 
                        ? query.OrderBy(f => f.FilledAt)
                        : query.OrderByDescending(f => f.FilledAt);
                    break;
                default:
                    query = sortAscending 
                        ? query.OrderBy(f => f.FilledAt)
                        : query.OrderByDescending(f => f.FilledAt);
                    break;
            }
            
            sortedForms = query.ToList();
        }
        
        private void NavigateToForm(int formId)
        {
            Navigation.NavigateTo($"/filled/{formId}");
        }

        private Task OnAccessTypeChanged(string newType)
        {
            accessType = newType;
            return Task.CompletedTask;
        }

        private Task OnEmailsChanged(string newEmails)
        {
            emails = newEmails;
            return Task.CompletedTask;
        }

        private Task OnRolesChanged(string newRoles)
        {
            roles = newRoles;
            return Task.CompletedTask;
        }

        private void DeleteFormFromResults(int formId)
        {
            FormService.DeleteForm(formId);
            sortedForms = sortedForms.Where(f => f.Id != formId).ToList();
            StateHasChanged();
        }
    }
} 