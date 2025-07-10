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
        [Inject] LocalizationService Loc { get; set; }
        [Inject] AuthenticationStateProvider AuthStateProvider { get; set; }
        [Inject] ITemplateService TemplateService { get; set; }
        [Inject] IUserService UserService { get; set; }

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

        private Func<string, string> getUserNickname => userId => TableUtils.GetUserNickname(userId, currentUserId, UserNicknames);
        private Func<string, string> getUserFullName => userId => TableUtils.GetUserFullName(userId, UserNicknames);
        private Func<Template, bool> hasAccessToTemplate => template => TableUtils.HasAccessToTemplate(template, currentUserId, TemplateService, UserService);

        private List<Template> GetFilteredTemplates()
        {
            return TableUtils.FilterTemplates(
                AllTemplates,
                searchQuery,
                filterAuthor,
                filterTag,
                filterTopic,
                getUserFullName,
                hasAccessToTemplate
            );
        }

        private void ApplyLatestSorting()
        {
            LastTemplatesSorted = TableUtils.SortTemplates(
                LastTemplates,
                latestSortField,
                latestSortDirection,
                (field, t) => field == "author" ? getUserNickname(t.AuthorId) : null
            );
        }

        private void ApplyPopularSorting()
        {
            PopularTemplatesSorted = TableUtils.SortTemplates(
                PopularTemplates,
                popularSortField,
                popularSortDirection,
                (field, t) => field == "author" ? getUserNickname(t.AuthorId) : null
            );
        }

        // Для Search — отдельная логика (пагинация, URL)
        private void SortSearchResults(string field)
        {
            if (searchSortField == field)
                searchSortDirection = searchSortDirection == "asc" ? "desc" : "asc";
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
            var newUri = TableUtils.BuildQueryString(
                uri.GetLeftPart(UriPartial.Path),
                searchQuery,
                filterTag,
                filterAuthor,
                filterTopic,
                searchSortField,
                searchSortDirection,
                CurrentPage
            );
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
            SearchResults = TableUtils.ApplyPagination(AllSearchResults, CurrentPage, PageSize);
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
        }

        private void ApplySearchSorting()
        {
            AllSearchResults = TableUtils.SortTemplates(
                AllSearchResults,
                searchSortField,
                searchSortDirection,
                (field, t) => field == "author" ? getUserNickname(t.AuthorId) : null
            );
        }

        private void SortLatestTemplates(string field)
        {
            if (latestSortField == field)
                latestSortDirection = latestSortDirection == "asc" ? "desc" : "asc";
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
                popularSortDirection = popularSortDirection == "asc" ? "desc" : "asc";
            else
            {
                popularSortField = field;
                popularSortDirection = "asc";
            }
            ApplyPopularSorting();
            StateHasChanged();
        }

        public void Dispose()
        {
            Navigation.LocationChanged -= OnLocationChanged;
        }
    }
} 