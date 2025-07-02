using FormsApp.Data;
using System.Collections.Generic;

public interface ITagService
{
    List<Tag> GetAllTags();
    void SyncTagsFromTemplates();
} 