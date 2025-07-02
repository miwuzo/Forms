using FormsApp.Data;
using System.Linq;

public class LikeService : ILikeService
{
    private readonly ApplicationDbContext _db;

    public LikeService(ApplicationDbContext db)
    {
        _db = db;
    }

    public int GetLikesCount(int templateId)
    {
        return _db.Likes.Count(l => l.TemplateId == templateId);
    }

    public bool UserLiked(int templateId, string userId)
    {
        return _db.Likes.Any(l => l.TemplateId == templateId && l.UserId == userId);
    }

    public void AddLike(int templateId, string userId)
    {
        if (!UserLiked(templateId, userId))
        {
            _db.Likes.Add(new Like { TemplateId = templateId, UserId = userId });
            _db.SaveChanges();
        }
    }

    public void RemoveLike(int templateId, string userId)
    {
        var like = _db.Likes.FirstOrDefault(l => l.TemplateId == templateId && l.UserId == userId);
        if (like != null)
        {
            _db.Likes.Remove(like);
            _db.SaveChanges();
        }
    }
} 