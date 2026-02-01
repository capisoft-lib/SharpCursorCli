using Microsoft.Data.Sqlite;

namespace Capisoft.Lib.SharpCursorCli;

/// <summary>
/// SQLite implementation of <see cref="ICursorAgentStorage"/>.
/// Lightweight persistence for conversation IDs across restarts; not a full transcript system.
/// Tables are created on first use.
/// </summary>
public sealed class SqliteCursorAgentStorage : ICursorAgentStorage
{
    private readonly string _connectionString;
    private static readonly object InitLock = new();

    /// <summary>Creates storage using the given database file path.</summary>
    /// <param name="dbPath">Path to the SQLite database file (e.g. "conversations.db").</param>
    public SqliteCursorAgentStorage(string dbPath)
    {
        if (string.IsNullOrWhiteSpace(dbPath))
            throw new ArgumentException("Database path must not be null or empty.", nameof(dbPath));
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Mode = SqliteOpenMode.ReadWriteCreate
        };
        _connectionString = builder.ConnectionString;
    }

    /// <summary>Creates storage using an existing connection string.</summary>
    public SqliteCursorAgentStorage(SqliteConnectionStringBuilder connectionStringBuilder)
    {
        ArgumentNullException.ThrowIfNull(connectionStringBuilder);
        _connectionString = connectionStringBuilder.ConnectionString ?? throw new ArgumentException("ConnectionString must not be null.", nameof(connectionStringBuilder));
    }

    /// <inheritdoc />
    public void SaveConversation(string sessionId, string? requestId = null, string? promptPreview = null, string? metadata = null)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            throw new ArgumentException("Session ID must not be null or empty.", nameof(sessionId));
        EnsureSchema();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Conversations (SessionId, RequestId, PromptPreview, CreatedAtUtc, Metadata)
            VALUES ($sid, $rid, $preview, $created, $meta)
            """;
        cmd.Parameters.AddWithValue("$sid", sessionId);
        cmd.Parameters.AddWithValue("$rid", (object?)requestId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$preview", (object?)promptPreview ?? DBNull.Value);
        cmd.Parameters.AddWithValue("$created", DateTime.UtcNow.ToString("O"));
        cmd.Parameters.AddWithValue("$meta", (object?)metadata ?? DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    /// <inheritdoc />
    public ConversationRecord? GetConversation(string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
            return null;
        EnsureSchema();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, SessionId, RequestId, PromptPreview, CreatedAtUtc, Metadata FROM Conversations WHERE SessionId = $sid LIMIT 1";
        cmd.Parameters.AddWithValue("$sid", sessionId);
        using var r = cmd.ExecuteReader();
        if (!r.Read())
            return null;
        return RowToRecord(r);
    }

    /// <inheritdoc />
    public IReadOnlyList<ConversationRecord> GetRecentConversations(int limit = 10)
    {
        EnsureSchema();
        var list = new List<ConversationRecord>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, SessionId, RequestId, PromptPreview, CreatedAtUtc, Metadata FROM Conversations ORDER BY CreatedAtUtc DESC LIMIT $lim";
        cmd.Parameters.AddWithValue("$lim", limit);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(RowToRecord(r));
        return list;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> ListSessionIds()
    {
        EnsureSchema();
        var list = new List<string>();
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT SessionId FROM Conversations ORDER BY CreatedAtUtc DESC";
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(r.GetString(0));
        return list;
    }

    private void EnsureSchema()
    {
        lock (InitLock)
        {
            using var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = """
                    CREATE TABLE IF NOT EXISTS Conversations (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        SessionId TEXT NOT NULL,
                        RequestId TEXT,
                        PromptPreview TEXT,
                        CreatedAtUtc TEXT NOT NULL,
                        Metadata TEXT
                    )
                    """;
                cmd.ExecuteNonQuery();
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "CREATE INDEX IF NOT EXISTS IX_Conversations_SessionId ON Conversations(SessionId)";
                cmd.ExecuteNonQuery();
            }
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "CREATE INDEX IF NOT EXISTS IX_Conversations_CreatedAtUtc ON Conversations(CreatedAtUtc DESC)";
                cmd.ExecuteNonQuery();
            }
        }
    }

    private static ConversationRecord RowToRecord(SqliteDataReader r)
    {
        var createdAt = r.GetString(4);
        DateTime.TryParse(createdAt, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt);
        return new ConversationRecord
        {
            Id = r.GetInt64(0),
            SessionId = r.GetString(1),
            RequestId = r.IsDBNull(2) ? null : r.GetString(2),
            PromptPreview = r.IsDBNull(3) ? null : r.GetString(3),
            CreatedAtUtc = dt,
            Metadata = r.IsDBNull(5) ? null : r.GetString(5)
        };
    }
}
