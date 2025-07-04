using System.Collections.Generic;
using System;

public class LocalizationService
{
    public string CurrentCulture { get; private set; } = "ru";

    public event Action? OnCultureChanged;

    private readonly Dictionary<string, Dictionary<string, string>> _resources = new()
    {
        ["ru"] = new()
        {
            ["AppTitle"] = "Формы",
            ["Login"] = "Вход",
            ["Register"] = "Регистрация",
            ["Logout"] = "Выйти",
            ["SearchPlaceholder"] = "Поиск шаблонов...",
            ["ThemeLight"] = "Светлая тема",
            ["ThemeDark"] = "Тёмная тема",
            ["AllTopics"] = "Все темы",
            ["Author"] = "Автор",
            ["Find"] = "Найти",
            ["EditFilledFormTitle"] = "Редактировать ответы",
            ["Loading"] = "Загрузка...",
            ["NoEditRights"] = "У вас нет прав для редактирования этой формы.",
            ["FormNotFound"] = "Форма не найдена.",
            ["Send"] = "Отправить",
            ["Like"] = "Лайк",
            ["Likes"] = "Лайки",
            ["Comments"] = "Комментарии",
            ["LeaveComment"] = "Оставьте комментарий...",
            ["AddComment"] = "Отправить",
            ["EditFormTitle"] = "Редактирование формы",
            ["ComponentUnavailable"] = "Этот компонент временно недоступен.",
            ["MyTemplates"] = "Мои шаблоны",
            ["MyForms"] = "Мои формы",
            ["AddQuestion"] = "Добавить вопрос:",
            ["TitleLabel"] = "Заголовок:",
            ["DescriptionLabel"] = "Описание:",
            ["ShowInResults"] = "Отображать в таблице заполненных форм",
            ["AccessAllowed"] = "Доступ разрешён:",
            ["Emails"] = "Emails",
            ["Roles"] = "Роли",
            ["PopularAnswers"] = "Популярные ответы (строковые поля):",
            ["PopularNumeric"] = "Средние значения (числовые поля):",
            ["Update"] = "Обновить",
            ["NoResultsYet"] = "Пока нет информации о результатах",
            ["NoAnalyticsYet"] = "Пока нет аналитики",
            ["String"] = "Строка",
            ["MultilineText"] = "Текст в несколько строк",
            ["Integer"] = "Целое положительное",
            ["Checkbox"] = "Чекбокс",
            ["Add"] = "Добавить",
            ["Save"] = "Сохранить",
            ["SaveAccess"] = "Сохранить доступ",
            ["FillTemplate"] = "Заполнить шаблон",
            ["CreateTemplate"] = "Создать шаблон",
            ["GeneralSettings"] = "Общие настройки",
            ["Access"] = "Доступ",
            ["Results"] = "Результаты",
            ["Questions"] = "Вопросы",
            ["Analytics"] = "Аналитика",
            ["Topic"] = "Топик",
            ["Tags"] = "Теги (через запятую)",
            ["User"] = "Пользователь",
            ["Date"] = "Дата",
            ["View"] = "Просмотр",
            ["Edit"] = "Изменить",
            ["Delete"] = "Удалить",
            ["Warning"] = "Внимание",
            ["ExistingFormWarning"] = "У вас уже есть заполненная форма для этого шаблона. При отправке ваши предыдущие ответы будут обновлены.",
            ["EditAnswer"] = "Заполнить шаблон",
            ["MainPageTitle"] = "Главная страница",
            ["MyData"] = "Мои данные",
            ["Admin"] = "Администрирование",
            ["Users"] = "Пользователи",
            ["AllTemplates"] = "Все шаблоны",
            ["AllForms"] = "Все формы",
            ["Title"] = "Заголовок",
            ["Description"] = "Описание",
            ["AddTemplate"] = "Добавить шаблон",
            ["CreatedDate"] = "Дата создания",
            ["Template"] = "Шаблон",
            ["AccessDenied"] = "Доступ запрещен",
            ["NoAccessToView"] = "У вас нет доступа для просмотра этого шаблона.",
            ["ContactTemplateOwner"] = "Обратитесь к владельцу шаблона для получения доступа.",
            ["BackToMain"] = "Вернуться на главную",
            ["NoAccessToFill"] = "У вас нет доступа для заполнения этого шаблона.",
            ["SingleLineText"] = "Однострочный текст",
            ["MultiLineText"] = "Многострочный текст",
            ["Number"] = "Число",
            ["YesNo"] = "Да/Нет",
            ["MaxQuestionsReached"] = "Достигнуто максимальное количество вопросов (50)",
            ["NoQuestionsYet"] = "Пока нет вопросов",
            ["QuestionsList"] = "Список вопросов",
            ["DeleteQuestion"] = "Удалить вопрос",
            ["EnterQuestionTitle"] = "Введите заголовок вопроса",
            ["EnterQuestionDescription"] = "Введите описание вопроса",
            ["ShowInResultsTable"] = "Показывать в таблице результатов",
            ["AccessType"] = "Тип доступа",
            ["AllAuthorized"] = "Все авторизованные",
            ["AllAuthorizedUsers"] = "Все авторизованные пользователи",
            ["ByEmail"] = "По email",
            ["OnlyUsersWithEmail"] = "Только пользователи с указанными email",
            ["ByRole"] = "По роли",
            ["OnlyUsersWithRoles"] = "Только пользователи с указанными ролями",
            ["EmailAddresses"] = "Email адреса",
            ["EmailAddressesPlaceholder"] = "email1@example.com, email2@example.com",
            ["EmailAddressesHelp"] = "Введите email адреса через запятую",
            ["RolesPlaceholder"] = "Admin, User, Moderator",
            ["RolesHelp"] = "Введите роли через запятую",
            ["NoAccessRights"] = "Нет настроек доступа",
            ["StringDescription"] = "Однострочный текст",
            ["MultilineTextDescription"] = "Многострочный текст",
            ["IntegerDescription"] = "Целое число",
            ["CheckboxDescription"] = "Да/Нет",
            ["UnknownTypeDescription"] = "Неизвестный тип",
            ["AccessSettingsSaved"] = "Настройки доступа сохранены",
            ["UnknownAccessType"] = "Неизвестный тип доступа",
            ["CheckingAccess"] = "Проверка доступа",
            ["PleaseWait"] = "Пожалуйста, подождите...",
            ["TemplateNotFound"] = "Шаблон не найден",
            ["DeleteComment"] = "Удалить комментарий",
            ["LatestTemplates"] = "Последние шаблоны",
            ["PopularTemplates"] = "Популярные шаблоны",
            ["FilledCount"] = "Количество заполнений",
            ["TotalResponses"] = "Всего ответов",
            ["TotalQuestions"] = "Всего вопросов",
            ["LastResponse"] = "Последний ответ",
            ["AvgAnswersPerForm"] = "Среднее кол-во ответов",
            ["UpdateAnalytics"] = "Обновить аналитику",
            ["NumericAnalytics"] = "Числовая аналитика",
            ["PopularAnswers"] = "Популярные ответы",
            ["FieldStatistics"] = "Статистика по полям",
            ["Question"] = "Вопрос",
            ["Type"] = "Тип",
            ["Responses"] = "Ответы",
            ["CompletionRate"] = "Процент заполнения",
            ["Average"] = "Среднее",
            ["MinMax"] = "Мин/Макс",
            ["NothingFound"] = "Ничего не найдено",
            ["ChooseAction"] = "Выберите действие...",
            ["Block"] = "Заблокировать",
            ["Unblock"] = "Разблокировать",
            ["MakeAdmin"] = "Сделать администратором",
            ["RemoveAdmin"] = "Убрать права администратора",
            ["Apply"] = "Применить",
            ["Role"] = "Роль",
            ["Blocked"] = "Заблокирован",
            ["DeleteSelected"] = "Удалить выбранные",
            ["FilledAt"] = "Дата",
            ["Templates"] = "Шаблоны",
            ["Forms"] = "Формы",
            ["Answer"] = "Ответ",
            ["NoAnswer"] = "Нет ответа",
            ["FilledFormTitle"] = "Заполненная форма",
            ["NotFound"] = "Страница не найдена",
        },
        ["en"] = new()
        {
            ["AppTitle"] = "Forms",
            ["Login"] = "Login",
            ["Register"] = "Register",
            ["Logout"] = "Logout",
            ["SearchPlaceholder"] = "Search forms...",
            ["ThemeLight"] = "Light theme",
            ["ThemeDark"] = "Dark theme",
            ["AllTopics"] = "All topics",
            ["Author"] = "Author",
            ["Find"] = "Find",
            ["EditFilledFormTitle"] = "Edit answers",
            ["Loading"] = "Loading...",
            ["NoEditRights"] = "You do not have rights to edit this form.",
            ["FormNotFound"] = "Form not found.",
            ["Send"] = "Send",
            ["Like"] = "Like",
            ["Likes"] = "Likes",
            ["Comments"] = "Comments",
            ["LeaveComment"] = "Leave a comment...",
            ["AddComment"] = "Send",
            ["EditFormTitle"] = "Edit form",
            ["ComponentUnavailable"] = "This component is temporarily unavailable.",
            ["MyTemplates"] = "My templates",
            ["MyForms"] = "My forms",
            ["AddQuestion"] = "Add question:",
            ["TitleLabel"] = "Title:",
            ["DescriptionLabel"] = "Description:",
            ["ShowInResults"] = "Show in filled forms table",
            ["AccessAllowed"] = "Access allowed:",
            ["Emails"] = "Emails",
            ["Roles"] = "Roles",
            ["PopularAnswers"] = "Popular answers (string fields):",
            ["PopularNumeric"] = "Average values (numeric fields):",
            ["Update"] = "Update",
            ["NoResultsYet"] = "No results yet",
            ["NoAnalyticsYet"] = "No analytics yet",
            ["String"] = "String",
            ["MultilineText"] = "Multiline text",
            ["Integer"] = "Integer",
            ["Checkbox"] = "Checkbox",
            ["Add"] = "Add",
            ["Save"] = "Save",
            ["SaveAccess"] = "Save access",
            ["FillTemplate"] = "Fill template",
            ["CreateTemplate"] = "Create template",
            ["GeneralSettings"] = "General settings",
            ["Access"] = "Access",
            ["Results"] = "Results",
            ["Questions"] = "Questions",
            ["Analytics"] = "Analytics",
            ["Topic"] = "Topic",
            ["Tags"] = "Tags (comma separated)",
            ["User"] = "User",
            ["Date"] = "Date",
            ["View"] = "View",
            ["Edit"] = "Edit",
            ["Delete"] = "Delete",
            ["Warning"] = "Warning",
            ["ExistingFormWarning"] = "You already have a filled form for this template. Your previous answers will be updated when you submit.",
            ["EditAnswer"] = "Fill template",
            ["MainPageTitle"] = "Main page",
            ["MyData"] = "My data",
            ["Admin"] = "Administration",
            ["Users"] = "Users",
            ["AllTemplates"] = "All templates",
            ["AllForms"] = "All forms",
            ["Title"] = "Title",
            ["Description"] = "Description",
            ["AddTemplate"] = "Add template",
            ["CreatedDate"] = "Created date",
            ["Template"] = "Template",
            ["AccessDenied"] = "Access denied",
            ["NoAccessToView"] = "You don't have access to view this template.",
            ["ContactTemplateOwner"] = "Contact the template owner to get access.",
            ["BackToMain"] = "Back to main",
            ["NoAccessToFill"] = "You don't have access to fill this template.",
            ["SingleLineText"] = "Single line text",
            ["MultiLineText"] = "Multi line text",
            ["Number"] = "Number",
            ["YesNo"] = "Yes/No",
            ["MaxQuestionsReached"] = "Maximum number of questions reached (50)",
            ["NoQuestionsYet"] = "No questions yet",
            ["QuestionsList"] = "Questions list",
            ["DeleteQuestion"] = "Delete question",
            ["EnterQuestionTitle"] = "Enter question title",
            ["EnterQuestionDescription"] = "Enter question description",
            ["ShowInResultsTable"] = "Show in results table",
            ["AccessType"] = "Access type",
            ["AllAuthorized"] = "All authorized",
            ["AllAuthorizedUsers"] = "All authorized users",
            ["ByEmail"] = "By email",
            ["OnlyUsersWithEmail"] = "Only users with specified emails",
            ["ByRole"] = "By role",
            ["OnlyUsersWithRoles"] = "Only users with specified roles",
            ["EmailAddresses"] = "Email addresses",
            ["EmailAddressesPlaceholder"] = "email1@example.com, email2@example.com",
            ["EmailAddressesHelp"] = "Enter email addresses separated by commas",
            ["RolesPlaceholder"] = "Admin, User, Moderator",
            ["RolesHelp"] = "Enter roles separated by commas",
            ["NoAccessRights"] = "No access settings",
            ["StringDescription"] = "Single line text",
            ["MultilineTextDescription"] = "Multi line text",
            ["IntegerDescription"] = "Integer number",
            ["CheckboxDescription"] = "Yes/No",
            ["UnknownTypeDescription"] = "Unknown type",
            ["AccessSettingsSaved"] = "Access settings saved",
            ["UnknownAccessType"] = "Unknown access type",
            ["CheckingAccess"] = "Checking access",
            ["PleaseWait"] = "Please wait...",
            ["TemplateNotFound"] = "Template not found",
            ["DeleteComment"] = "Delete comment",
            ["LatestTemplates"] = "Latest templates",
            ["PopularTemplates"] = "Popular templates",
            ["FilledCount"] = "Filled count",
            ["TotalResponses"] = "Total responses",
            ["TotalQuestions"] = "Total questions",
            ["LastResponse"] = "Last response",
            ["AvgAnswersPerForm"] = "Average answers per form",
            ["UpdateAnalytics"] = "Update analytics",
            ["NumericAnalytics"] = "Numeric analytics",
            ["PopularAnswers"] = "Popular answers",
            ["FieldStatistics"] = "Field statistics",
            ["Question"] = "Question",
            ["Type"] = "Type",
            ["Responses"] = "Responses",
            ["CompletionRate"] = "Completion rate",
            ["Average"] = "Average",
            ["MinMax"] = "Min/Max",
            ["NothingFound"] = "Nothing found",
            ["ChooseAction"] = "Select action...",
            ["Block"] = "Block",
            ["Unblock"] = "Unblock",
            ["MakeAdmin"] = "Make admin",
            ["RemoveAdmin"] = "Remove admin rights",
            ["Apply"] = "Apply",
            ["Role"] = "Role",
            ["Blocked"] = "Blocked",
            ["DeleteSelected"] = "Delete selected",
            ["FilledAt"] = "Date",
            ["Templates"] = "Templates",
            ["Forms"] = "Forms",
            ["Answer"] = "Answer",
            ["NoAnswer"] = "No answer",
            ["FilledFormTitle"] = "Filled form",
            ["NotFound"] = "Page not found",
        }
    };

    public void SetCulture(string culture)
    {
        if (_resources.ContainsKey(culture))
        {
            CurrentCulture = culture;
            OnCultureChanged?.Invoke();
        }
    }

    public string this[string key]
    {
        get
        {
            if (_resources.TryGetValue(CurrentCulture, out var dict) && dict.TryGetValue(key, out var value))
                return value;
            return key;
        }
    }
} 