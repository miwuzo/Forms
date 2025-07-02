using FormsApp.Data;
using System.Collections.Generic;

public interface ICommentService
{
    List<Comment> GetCommentsByTemplateId(int templateId);
    void AddComment(Comment comment);
    void DeleteComment(int commentId);
} 