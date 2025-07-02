using Microsoft.AspNetCore.Components;
using FormsApp.Data;
using FormsApp.Shared;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FormsApp.Pages.Common
{
    public partial class Index : ComponentBase, IDisposable
    {
        private List<Template> LastTemplates = new();
        private List<Template> PopularTemplates = new();
        private List<Template> AllTemplates = new();
        private List<(string Name, int Count)> Tags = new();
        private string filterTag = string.Empty;
        private string filterTopic = string.Empty;
        private string filterAuthor = string.Empty;
        private string searchQuery = string.Empty;
        private string? currentUserId;

        private string latestSortField = "created";
        private string latestSortDirection = "desc";
        private string popularSortField = "count";
        private string popularSortDirection = "desc";

        private List<Template> LastTemplatesSorted = new();
        private List<Template> PopularTemplatesSorted = new();

        private Dictionary<string, string> UserNicknames = new();

        private List<Template> SearchResults = new();
        private List<Template> AllSearchResults = new();
        private string searchSortField = "popular";
        private string searchSortDirection = "desc";
        private int CurrentPage = 1;
        private const int PageSize = 5;
        private int TotalSearchResults = 0;
        private int TotalPages = 0;

        private List<SortableTable<Template>.SortableColumn> searchColumns;
        private List<SortableTable<Template>.SortableColumn> latestColumns;
        private List<SortableTable<Template>.SortableColumn> popularColumns;

        [Inject] NavigationManager Navigation { get; set; }
        [Inject] IStringLocalizer<SharedResource> L { get; set; }
        [Inject] LocalizationService Loc { get; set; }
        [Inject] AuthenticationStateProvider AuthStateProvider { get; set; }
        [Inject] ITemplateService TemplateService { get; set; }
        [Inject] IUserService UserService { get; set; }
        [Inject] ITagService TagService { get; set; }
        [Inject] IHttpContextAccessor HttpContextAccessor { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            currentUserId = authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            searchColumns = new List<SortableTable<Template>.SortableColumn>
            {
                new() { Title = Loc["Title"], Field = "title", Sortable = true },
                new() { Title = Loc["Description"], Field = "description", Sortable = false },
                new() { Title = Loc["Author"], Field = "author", Sortable = true },
                new() { Title = Loc["FilledCount"], Field = "popular", Sortable = true },
                new() { Title = Loc["CreatedDate"], Field = "recent", Sortable = true }
            };
            latestColumns = new List<SortableTable<Template>.SortableColumn>
            {
                new() { Title = Loc["Title"], Field = "title", Sortable = true },
                new() { Title = Loc["Description"], Field = "description", Sortable = false },
                new() { Title = Loc["Author"], Field = "author", Sortable = true },
                new() { Title = Loc["CreatedDate"], Field = "created", Sortable = true }
            };
            popularColumns = new List<SortableTable<Template>.SortableColumn>
            {
                new() { Title = Loc["Title"], Field = "title", Sortable = true },
                new() { Title = Loc["Description"], Field = "description", Sortable = false },
                new() { Title = Loc["Author"], Field = "author", Sortable = true },
                new() { Title = Loc["FilledCount"], Field = "popular", Sortable = true }
            };

            Navigation.LocationChanged += OnLocationChanged;
            LoadData();
        }

        protected override void OnParametersSet()
        {
            LoadData();
        }

        private void OnLocationChanged(object sender, LocationChangedEventArgs e)
        {
            LoadData();
            StateHasChanged();
        }

        private void NavigateToTemplate(int templateId)
        {
            Navigation.NavigateTo($"/template/{templateId}");
        }

        private string GetUserNickname(string userId)
        {
            if (userId == currentUserId)
                return "Вы";
            if (UserNicknames.TryGetValue(userId, out string nickname))
            {
                int atIndex = nickname.IndexOf('@');
                if (atIndex > 0)
                    return nickname.Substring(0, atIndex);
                return nickname;
            }
            return userId;
        }

        private string GetUserFullName(string userId)
        {
            if (UserNicknames.TryGetValue(userId, out string nickname))
                return nickname;
            return userId;
        }

        private string TruncateDescription(string description)
        {
            if (string.IsNullOrEmpty(description))
                return string.Empty;
            if (description.Length <= 100)
                return description;
            return description.Substring(0, 100) + "...";
        }

        private void SortLatestTemplates(string field)
        {
            if (latestSortField == field)
            {
                latestSortDirection = latestSortDirection == "asc" ? "desc" : "asc";
            }
            else
            {
                latestSortField = field;
                latestSortDirection = "asc";
            }
            ApplyLatestSorting();
            StateHasChanged();
        }

        private void SortPopularTemplates(string field)
        {
            if (popularSortField == field)
            {
                popularSortDirection = popularSortDirection == "asc" ? "desc" : "asc";
            }
            else
            {
                popularSortField = field;
                popularSortDirection = "asc";
            }
            ApplyPopularSorting();
            StateHasChanged();
        }

        private void SortSearchResults(string field)
        {
            if (searchSortField == field)
            {
                searchSortDirection = searchSortDirection == "asc" ? "desc" : "asc";
            }
            else
            {
                searchSortField = field;
                searchSortDirection = "asc";
            }
            ApplySearchSorting();
            CurrentPage = 1;
            ApplyPagination();
            UpdateSearchUrl();
            StateHasChanged();
        }

        private void UpdateSearchUrl()
        {
            var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            if (!string.IsNullOrEmpty(searchQuery))
                query["search"] = searchQuery;
            if (!string.IsNullOrEmpty(filterTag))
                query["tag"] = filterTag;
            if (!string.IsNullOrEmpty(filterAuthor))
                query["author"] = filterAuthor;
            if (!string.IsNullOrEmpty(filterTopic))
                query["topic"] = filterTopic;
            query["sort"] = searchSortField;
            query["order"] = searchSortDirection;
            query["page"] = CurrentPage.ToString();
            var newUri = $"{uri.GetLeftPart(UriPartial.Path)}?{query}";
            Navigation.NavigateTo(newUri, false, true);
        }

        private void ChangePage(int page)
        {
            if (page >= 1 && page <= TotalPages)
            {
                CurrentPage = page;
                ApplyPagination();
                UpdateSearchUrl();
                StateHasChanged();
            }
        }

        private void ApplyPagination()
        {
            var startIndex = (CurrentPage - 1) * PageSize;
            SearchResults = AllSearchResults.Skip(startIndex).Take(PageSize).ToList();
        }

        private List<Template> GetFilteredTemplates()
        {
            var templates = AllTemplates.ToList();
            templates = FilterTemplatesByAccess(templates);
            if (!string.IsNullOrEmpty(searchQuery))
            {
                var searchTerms = searchQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Where(term => !string.IsNullOrWhiteSpace(term))
                    .Select(term => term.ToLowerInvariant())
                    .ToList();
                templates = templates.Where(template =>
                    searchTerms.All(searchTerm =>
                        template.Title.ToLowerInvariant().Contains(searchTerm) ||
                        template.Description.ToLowerInvariant().Contains(searchTerm) ||
                        template.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(tag => tag.Trim().ToLowerInvariant()).Any(tag => tag.Contains(searchTerm)) ||
                        (!string.IsNullOrEmpty(template.Topic) && template.Topic.ToLowerInvariant().Contains(searchTerm)) ||
                        GetUserFullName(template.AuthorId).ToLowerInvariant().Contains(searchTerm) ||
                        template.Fields.Any(field => field.Label.ToLowerInvariant().Contains(searchTerm) || field.Description.ToLowerInvariant().Contains(searchTerm))
                    )
                ).ToList();
            }
            if (!string.IsNullOrEmpty(filterAuthor))
            {
                var authorSearchTerm = filterAuthor.ToLowerInvariant();
                templates = templates.Where(template => GetUserFullName(template.AuthorId).ToLowerInvariant().Contains(authorSearchTerm)).ToList();
            }
            if (!string.IsNullOrEmpty(filterTag))
                templates = templates.Where(t => t.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).Contains(filterTag)).ToList();
            if (!string.IsNullOrEmpty(filterTopic))
                templates = templates.Where(t => t.Topic == filterTopic).ToList();
            return templates;
        }

        private List<Template> FilterTemplatesByAccess(List<Template> templates)
        {
            var filteredTemplates = new List<Template>();
            foreach (var template in templates)
            {
                if (HasAccessToTemplate(template))
                {
                    filteredTemplates.Add(template);
                }
            }
            return filteredTemplates;
        }

        private bool HasAccessToTemplate(Template template)
        {
            var accessRules = TemplateService.GetAccessRules(template.Id);
            if (!accessRules.Any())
                return true;
            if (accessRules.Any(r => r.Email == null && r.Role == null))
                return true;
            if (currentUserId == null)
                return accessRules.Any(r => r.Email == null && r.Role == null);
            var userEmail = UserService.GetUserById(currentUserId)?.Email;
            if (!string.IsNullOrEmpty(userEmail) && accessRules.Any(r => r.Email == userEmail))
                return true;
            var userRoles = new List<string>();
            var user = UserService.GetUserById(currentUserId);
            if (user != null && !string.IsNullOrEmpty(user.Role))
                userRoles.Add(user.Role);
            if (userRoles.Any() && accessRules.Any(r => r.Role != null && userRoles.Contains(r.Role)))
                return true;
            if (template.AuthorId == currentUserId)
                return true;
            if (user?.Role == "Admin")
                return true;
            return false;
        }

        private void LoadData()
        {
            var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            var newSearchQuery = query["search"] ?? string.Empty;
            filterTopic = query["topic"] ?? string.Empty;
            filterTag = query["tag"] ?? string.Empty;
            filterAuthor = query["author"] ?? string.Empty;
            AllTemplates = TemplateService.GetAllTemplates();
            var userIds = AllTemplates.Select(t => t.AuthorId).Distinct().ToList();
            var users = UserService.GetAllUsers().Where(u => userIds.Contains(u.Id)).ToList();
            UserNicknames = users.ToDictionary(u => u.Id, u => u.UserName ?? u.Id);
            if (!string.IsNullOrEmpty(newSearchQuery) || !string.IsNullOrEmpty(filterTag) || !string.IsNullOrEmpty(filterAuthor) || !string.IsNullOrEmpty(filterTopic))
            {
                searchQuery = newSearchQuery;
                AllSearchResults = GetFilteredTemplates();
                TotalSearchResults = AllSearchResults.Count;
                TotalPages = (int)Math.Ceiling((double)TotalSearchResults / PageSize);
                var sortParam = query["sort"];
                var orderParam = query["order"];
                var pageParam = query["page"];
                if (!string.IsNullOrEmpty(sortParam))
                    searchSortField = sortParam;
                else
                    searchSortField = "popular";
                if (!string.IsNullOrEmpty(orderParam))
                    searchSortDirection = orderParam;
                else
                    searchSortDirection = "desc";
                if (!string.IsNullOrEmpty(pageParam) && int.TryParse(pageParam, out int page))
                    CurrentPage = Math.Max(1, Math.Min(page, TotalPages));
                else
                    CurrentPage = 1;
                ApplySearchSorting();
                ApplyPagination();
                LastTemplates = new List<Template>();
                PopularTemplates = new List<Template>();
                LastTemplatesSorted = new List<Template>();
                PopularTemplatesSorted = new List<Template>();
            }
            else
            {
                searchQuery = string.Empty;
                SearchResults = new List<Template>();
                AllSearchResults = new List<Template>();
                TotalSearchResults = 0;
                TotalPages = 0;
                var filtered = GetFilteredTemplates();
                LastTemplates = filtered.OrderByDescending(t => t.CreatedAt).Take(5).ToList();
                PopularTemplates = filtered.OrderByDescending(t => t.Forms.Count).Take(5).ToList();
                latestSortField = "created";
                latestSortDirection = "desc";
                popularSortField = "count";
                popularSortDirection = "desc";
                ApplyLatestSorting();
                ApplyPopularSorting();
            }
            Tags = AllTemplates.SelectMany(t => t.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .GroupBy(tag => tag.Trim())
                .Select(g => (g.Key, g.Count()))
                .OrderByDescending(x => x.Item2)
                .ToList();
        }

        private List<Template> SortTemplates(List<Template> templates, string field, string direction)
        {
            Func<Template, object> keySelector = field switch
            {
                "title" => t => t.Title,
                "author" => t => GetUserNickname(t.AuthorId),
                "recent" => t => t.CreatedAt,
                "created" => t => t.CreatedAt,
                "popular" => t => t.Forms.Count,
                "count" => t => t.Forms.Count,
                _ => t => t.Title
            };
            return direction == "asc"
                ? templates.OrderBy(keySelector).ToList()
                : templates.OrderByDescending(keySelector).ToList();
        }

        private void ApplyLatestSorting()
        {
            LastTemplatesSorted = SortTemplates(LastTemplates, latestSortField, latestSortDirection);
        }

        private void ApplyPopularSorting()
        {
            PopularTemplatesSorted = SortTemplates(PopularTemplates, popularSortField, popularSortDirection);
        }

        private void ApplySearchSorting()
        {
            AllSearchResults = SortTemplates(AllSearchResults, searchSortField, searchSortDirection);
        }

        public void Dispose()
        {
            Navigation.LocationChanged -= OnLocationChanged;
        }
    }
} 