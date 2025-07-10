using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using FormsApp.Data;

namespace FormsApp.Shared
{
    public static class TableUtils
    {
        public static List<T> GetSorted<T>(List<T> items, string column, bool ascending, Func<T, object?> selector)
        {
            if (ascending)
                return items.OrderBy(selector).ToList();
            else
                return items.OrderByDescending(selector).ToList();
        }

        public static List<T> GetSorted<T>(List<T> items, string column, bool ascending, Dictionary<string, Func<T, object?>> selectors, string defaultColumn)
        {
            if (!selectors.TryGetValue(column, out var selector))
                selector = selectors[defaultColumn];
            return ascending ? items.OrderBy(selector).ToList() : items.OrderByDescending(selector).ToList();
        }

        public static void OnSort(ref string sortColumn, ref bool sortAscending, string column)
        {
            if (sortColumn == column)
                sortAscending = !sortAscending;
            else
            {
                sortColumn = column;
                sortAscending = true;
            }
        }

        public static void UpdateSelection<TId>(ChangeEventArgs e, TId id, List<TId> selected, List<TId> all)
        {
            var isChecked = (bool)e.Value;
            if (isChecked)
            {
                if (!selected.Contains(id))
                    selected.Add(id);
            }
            else
            {
                selected.Remove(id);
            }
        }

        public static bool ShouldSelectAll<TId>(List<TId> selected, List<TId> all)
        {
            return selected.Count == all.Count && all.Count > 0;
        }

        public static void DeleteSelected<TId, TItem>(List<TId> selectedIds, List<TItem> items, Action<TId> deleteAction, Func<TItem, TId> idSelector)
        {
            foreach (var id in selectedIds.ToList())
            {
                deleteAction(id);
                items.RemoveAll(x => EqualityComparer<TId>.Default.Equals(idSelector(x), id));
            }
            selectedIds.Clear();
        }

        public static void DeleteItem<T, TId>(TId id, List<T> items, Action<TId> deleteAction, Func<T, TId> idSelector)
        {
            deleteAction(id);
            items.RemoveAll(item => EqualityComparer<TId>.Default.Equals(idSelector(item), id));
        }

        public static string TruncateDescription(string description, int maxLength = 100)
        {
            if (string.IsNullOrEmpty(description))
                return string.Empty;
            if (description.Length <= maxLength)
                return description;
            return description.Substring(0, maxLength) + "...";
        }

        public static List<Template> SortTemplates(List<Template> templates, string sortField, string sortDirection, Func<string, Template, object?>? customSelector = null)
        {
            Func<Template, object?> keySelector = sortField switch
            {
                "title" => t => t.Title,
                "author" => t => t.AuthorId, 
                "created" or "recent" => t => t.CreatedAt,
                "popular" or "count" => t => t.Forms.Count,
                _ => t => t.CreatedAt
            };
            if (customSelector != null && (sortField == "author"))
                keySelector = t => customSelector("author", t);
            return sortDirection == "asc"
                ? templates.OrderBy(keySelector).ToList()
                : templates.OrderByDescending(keySelector).ToList();
        }

        public static List<Template> FilterTemplates(
            List<Template> templates,
            string searchQuery,
            string filterAuthor,
            string filterTag,
            string filterTopic,
            Func<string, string> getUserFullName,
            Func<Template, bool> hasAccessToTemplate)
        {
            return templates
                .Where(hasAccessToTemplate)
                .Where(t => string.IsNullOrEmpty(searchQuery) || SearchMatch(t, searchQuery, getUserFullName))
                .Where(t => string.IsNullOrEmpty(filterAuthor) || getUserFullName(t.AuthorId).Contains(filterAuthor, StringComparison.OrdinalIgnoreCase))
                .Where(t => string.IsNullOrEmpty(filterTag) || t.Tags.Split(',').Select(x => x.Trim()).Contains(filterTag))
                .Where(t => string.IsNullOrEmpty(filterTopic) || t.Topic == filterTopic)
                .ToList();
        }

        public static bool SearchMatch(Template template, string query, Func<string, string> getUserFullName)
        {
            var searchTerms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(term => term.ToLowerInvariant());
            return searchTerms.All(term =>
                template.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                template.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                template.Tags.Split(',').Any(tag => tag.Trim().Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(template.Topic) && template.Topic.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                getUserFullName(template.AuthorId).Contains(term, StringComparison.OrdinalIgnoreCase) ||
                template.Fields.Any(f => f.Label.Contains(term, StringComparison.OrdinalIgnoreCase) || f.Description.Contains(term, StringComparison.OrdinalIgnoreCase))
            );
        }

        public static string GetUserNickname(string userId, string? currentUserId, Dictionary<string, string> userNicknames)
        {
            if (userId == currentUserId)
                return "Вы";
            if (userNicknames.TryGetValue(userId, out string nickname))
            {
                int atIndex = nickname.IndexOf('@');
                if (atIndex > 0)
                    return nickname.Substring(0, atIndex);
                return nickname;
            }
            return userId;
        }

        public static string GetUserFullName(string userId, Dictionary<string, string> userNicknames)
        {
            if (userNicknames.TryGetValue(userId, out string nickname))
                return nickname;
            return userId;
        }

        public static bool HasAccessToTemplate(
            Template template,
            string? currentUserId,
            ITemplateService templateService,
            IUserService userService)
        {
            var accessRules = templateService.GetAccessRules(template.Id);
            if (!accessRules.Any())
                return true;
            if (accessRules.Any(r => r.Email == null && r.Role == null))
                return true;
            if (currentUserId == null)
                return accessRules.Any(r => r.Email == null && r.Role == null);
            var userEmail = userService.GetUserById(currentUserId)?.Email;
            if (!string.IsNullOrEmpty(userEmail) && accessRules.Any(r => r.Email == userEmail))
                return true;
            var userRoles = new List<string>();
            var user = userService.GetUserById(currentUserId);
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

        public static List<T> ApplyPagination<T>(List<T> items, int currentPage, int pageSize)
        {
            var startIndex = (currentPage - 1) * pageSize;
            return items.Skip(startIndex).Take(pageSize).ToList();
        }

        public static string BuildQueryString(
            string basePath,
            string searchQuery,
            string filterTag,
            string filterAuthor,
            string filterTopic,
            string sortField,
            string sortDirection,
            int currentPage)
        {
            var query = System.Web.HttpUtility.ParseQueryString("");
            void SetIfNotEmpty(string key, string value)
            {
                if (!string.IsNullOrEmpty(value)) query[key] = value;
            }
            SetIfNotEmpty("search", searchQuery);
            SetIfNotEmpty("tag", filterTag);
            SetIfNotEmpty("author", filterAuthor);
            SetIfNotEmpty("topic", filterTopic);
            query["sort"] = sortField;
            query["order"] = sortDirection;
            query["page"] = currentPage.ToString();
            return $"{basePath}?{query}";
        }
    }
} 