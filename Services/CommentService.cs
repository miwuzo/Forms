using FormsApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

public class CommentService : ICommentService
{
    private readonly ApplicationDbContext _db;

    public CommentService(ApplicationDbContext db)
    {
        _db = db;
    }

    public List<Comment> GetCommentsByTemplateId(int templateId)
    {
        return _db.Comments
            .Include(c => c.User)
            .Where(c => c.TemplateId == templateId)
            .OrderBy(c => c.CreatedAt)
            .ToList();
    }

    public void AddComment(Comment comment)
    {
        _db.Comments.Add(comment);
        _db.SaveChanges();
    }

    public void DeleteComment(int commentId)
    {
        var comment = _db.Comments.Find(commentId);
        if (comment != null)
        {
            _db.Comments.Remove(comment);
            _db.SaveChanges();
        }
    }
} 