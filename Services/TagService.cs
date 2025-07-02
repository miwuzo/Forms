using FormsApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;

public class TagService : ITagService
{
    private readonly ApplicationDbContext _db;

    public TagService(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<Tag> GetAllTags()
    {
        return _db.Tags.ToList();
    }

    public void SyncTagsFromTemplates()
    {
        var templates = _db.Templates.ToList();
        var allTags = templates
            .SelectMany(t => t.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => tag.ToLowerInvariant())
            .Distinct()
            .ToList();
        var existingTags = _db.Tags.Select(t => t.Name.ToLowerInvariant()).ToHashSet();
        var newTags = allTags.Except(existingTags).ToList();
        foreach (var tag in newTags)
        {
            _db.Tags.Add(new Tag { Name = tag });
        }
        if (newTags.Count > 0)
            _db.SaveChanges();
    }
} 