using Microsoft.Data.Sqlite;
using WitchDrawer.Core.Localization;
using WitchDrawer.Core.Models;

namespace WitchDrawer.Core.Storage;

internal sealed class TodoRepository(Func<SqliteConnection> createConnection)
{
    private readonly Func<SqliteConnection> _createConnection = createConnection;

    public async Task<IReadOnlyList<TodoItem>> GetTodosAsync(
        Guid boxId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _createConnection();
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT Id, BoxId, Title, IsCompleted, SortOrder, CreatedAt, UpdatedAt, CompletedAt, IsArchived, ArchivedAt
            FROM Todos
            WHERE BoxId = $boxId AND IsArchived = 0
            ORDER BY IsCompleted, SortOrder, CreatedAt;
            """;
        command.Parameters.AddWithValue("$boxId", boxId.ToString());

        var todos = new List<TodoItem>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            todos.Add(ReadTodo(reader));
        }

        return todos;
    }

    public async Task<IReadOnlyList<TodoItem>> GetArchivedTodosAsync(
        Guid? boxId = null,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _createConnection();
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = boxId is null
            ? """
              SELECT Id, BoxId, Title, IsCompleted, SortOrder, CreatedAt, UpdatedAt, CompletedAt, IsArchived, ArchivedAt
              FROM Todos
              WHERE IsArchived = 1
              ORDER BY ArchivedAt DESC, UpdatedAt DESC;
              """
            : """
              SELECT Id, BoxId, Title, IsCompleted, SortOrder, CreatedAt, UpdatedAt, CompletedAt, IsArchived, ArchivedAt
              FROM Todos
              WHERE BoxId = $boxId AND IsArchived = 1
              ORDER BY ArchivedAt DESC, UpdatedAt DESC;
              """;
        if (boxId is not null)
        {
            command.Parameters.AddWithValue("$boxId", boxId.Value.ToString());
        }

        var todos = new List<TodoItem>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            todos.Add(ReadTodo(reader));
        }

        return todos;
    }

    public async Task<TodoItem?> GetTodoAsync(Guid todoId, CancellationToken cancellationToken = default)
    {
        await using var connection = _createConnection();
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT Id, BoxId, Title, IsCompleted, SortOrder, CreatedAt, UpdatedAt, CompletedAt, IsArchived, ArchivedAt
            FROM Todos
            WHERE Id = $id;
            """;
        command.Parameters.AddWithValue("$id", todoId.ToString());

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadTodo(reader) : null;
    }

    public async Task AddTodoAsync(TodoItem todo, CancellationToken cancellationToken = default)
    {
        await using var connection = _createConnection();
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            INSERT INTO Todos (
                Id, BoxId, Title, IsCompleted, IsArchived, SortOrder,
                CreatedAt, UpdatedAt, CompletedAt, ArchivedAt)
            VALUES (
                $id, $boxId, $title, $isCompleted, $isArchived, $sortOrder,
                $createdAt, $updatedAt, $completedAt, $archivedAt);
            """;
        command.Parameters.AddWithValue("$id", todo.Id.ToString());
        command.Parameters.AddWithValue("$boxId", todo.BoxId.ToString());
        command.Parameters.AddWithValue("$title", todo.Title);
        command.Parameters.AddWithValue("$isCompleted", todo.IsCompleted ? 1 : 0);
        command.Parameters.AddWithValue("$isArchived", todo.IsArchived ? 1 : 0);
        command.Parameters.AddWithValue("$sortOrder", todo.SortOrder);
        command.Parameters.AddWithValue("$createdAt", ToDb(todo.CreatedAt));
        command.Parameters.AddWithValue("$updatedAt", ToDb(todo.UpdatedAt));
        command.Parameters.AddWithValue(
            "$completedAt",
            todo.CompletedAt is null ? DBNull.Value : ToDb(todo.CompletedAt.Value));
        command.Parameters.AddWithValue(
            "$archivedAt",
            todo.ArchivedAt is null ? DBNull.Value : ToDb(todo.ArchivedAt.Value));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> ArchiveCompletedTodosAsync(
        Guid boxId,
        DateTimeOffset archivedAt,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _createConnection();
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            UPDATE Todos
            SET IsArchived = 1,
                ArchivedAt = $archivedAt,
                UpdatedAt = $archivedAt
            WHERE BoxId = $boxId
              AND IsCompleted = 1
              AND IsArchived = 0;
            """;
        command.Parameters.AddWithValue("$boxId", boxId.ToString());
        command.Parameters.AddWithValue("$archivedAt", ToDb(archivedAt));

        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpdateTodoArchiveStateAsync(
        Guid todoId,
        bool isArchived,
        DateTimeOffset? archivedAt,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _createConnection();
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            UPDATE Todos
            SET IsArchived = $isArchived,
                ArchivedAt = $archivedAt,
                UpdatedAt = $updatedAt
            WHERE Id = $id;
            """;
        command.Parameters.AddWithValue("$id", todoId.ToString());
        command.Parameters.AddWithValue("$isArchived", isArchived ? 1 : 0);
        command.Parameters.AddWithValue(
            "$archivedAt",
            archivedAt is null ? DBNull.Value : ToDb(archivedAt.Value));
        command.Parameters.AddWithValue("$updatedAt", ToDb(updatedAt));

        if (await command.ExecuteNonQueryAsync(cancellationToken) != 1)
        {
            throw new InvalidOperationException("Todo does not exist.");
        }
    }

    public async Task UpdateTodoCompletionAsync(
        Guid todoId,
        bool isCompleted,
        DateTimeOffset? completedAt,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _createConnection();
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText =
            """
            UPDATE Todos
            SET IsCompleted = $isCompleted,
                CompletedAt = $completedAt,
                UpdatedAt = $updatedAt
            WHERE Id = $id;
            """;
        command.Parameters.AddWithValue("$id", todoId.ToString());
        command.Parameters.AddWithValue("$isCompleted", isCompleted ? 1 : 0);
        command.Parameters.AddWithValue(
            "$completedAt",
            completedAt is null ? DBNull.Value : ToDb(completedAt.Value));
        command.Parameters.AddWithValue("$updatedAt", ToDb(updatedAt));

        if (await command.ExecuteNonQueryAsync(cancellationToken) != 1)
        {
            throw new InvalidOperationException("Todo does not exist.");
        }
    }

    public async Task UpdateTodoTitleAsync(
        Guid todoId,
        string title,
        string expectedTitle,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _createConnection();
        await connection.OpenAsync(cancellationToken);
        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE Todos SET Title = $title, UpdatedAt = $updatedAt
            WHERE Id = $id AND Title = $expectedTitle;
            """;
        command.Parameters.AddWithValue("$id", todoId.ToString());
        command.Parameters.AddWithValue("$title", title);
        command.Parameters.AddWithValue("$expectedTitle", expectedTitle);
        command.Parameters.AddWithValue("$updatedAt", ToDb(updatedAt));
        if (await command.ExecuteNonQueryAsync(cancellationToken) != 1)
        {
            throw new InvalidOperationException(Strings.Get("TheTaskWasChangedOrDeletedCancelEditingAnd"));
        }
    }

    public async Task RemoveTodoAsync(Guid todoId, CancellationToken cancellationToken = default)
    {
        await using var connection = _createConnection();
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Todos WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", todoId.ToString());

        if (await command.ExecuteNonQueryAsync(cancellationToken) != 1)
        {
            throw new InvalidOperationException("Todo does not exist.");
        }
    }

    public async Task<int> GetNextTodoSortOrderAsync(
        Guid boxId,
        CancellationToken cancellationToken = default)
    {
        await using var connection = _createConnection();
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = "SELECT COALESCE(MAX(SortOrder), -1) + 1 FROM Todos WHERE BoxId = $boxId;";
        command.Parameters.AddWithValue("$boxId", boxId.ToString());
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }

    private static TodoItem ReadTodo(SqliteDataReader reader)
    {
        return new TodoItem(
            Guid.Parse(reader.GetString(0)),
            Guid.Parse(reader.GetString(1)),
            reader.GetString(2),
            reader.GetInt32(3) != 0,
            reader.GetInt32(4),
            FromDb(reader.GetString(5)),
            FromDb(reader.GetString(6)),
            reader.IsDBNull(7) ? null : FromDb(reader.GetString(7)),
            reader.GetInt32(8) != 0,
            reader.IsDBNull(9) ? null : FromDb(reader.GetString(9)));
    }

    private static string ToDb(DateTimeOffset value)
    {
        return value.ToUniversalTime().ToString("O");
    }

    private static DateTimeOffset FromDb(string value)
    {
        return DateTimeOffset.Parse(value);
    }
}
