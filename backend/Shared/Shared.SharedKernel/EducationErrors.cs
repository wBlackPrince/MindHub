namespace Shared.SharedKernel;

public static class EducationErrors
{
    public static Error TitleConflict(string title) =>
        Error.Conflict(
            new ErrorMessage(
            "lesson.title.conflict",
            $"Урок с заголовком {title} уже существует"));

    public static Error DatabaseError() =>
        Error.Failure(
            new ErrorMessage(
            "education.database.error",
            "Ошибка базы данных при работе с сервисом - education"));

    public static Error OperationCancelled() =>
        Error.Failure(
            new ErrorMessage(
                "education.operation.cancelled",
                "Операция была отменена"));

}