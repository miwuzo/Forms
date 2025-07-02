using FormsApp.Data;
using System.Collections.Generic;

public interface ILikeService
{
    int GetLikesCount(int templateId);
    bool UserLiked(int templateId, string userId);
    void AddLike(int templateId, string userId);
    void RemoveLike(int templateId, string userId);
} 