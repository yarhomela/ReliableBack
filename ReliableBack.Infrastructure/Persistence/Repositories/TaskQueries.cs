namespace ReliableBack.Infrastructure.Persistence.Repositories;

internal static class TaskQueries
{
    internal const string GetById = """
                                    SELECT id, type, payload, status, priority,
                                           retry_count AS RetryCount,
                                           max_retries AS MaxRetries,
                                           error_message AS ErrorMessage,
                                           scheduled_at AS ScheduledAt,
                                           created_at AS CreatedAt,
                                           updated_at AS UpdatedAt
                                    FROM tasks
                                    WHERE id = @Id
                                    """;

    internal const string GetAll = """
                                   SELECT id, type, payload, status, priority,
                                          retry_count AS RetryCount,
                                          max_retries AS MaxRetries,
                                          error_message AS ErrorMessage,
                                          scheduled_at AS ScheduledAt,
                                          created_at AS CreatedAt,
                                          updated_at AS UpdatedAt
                                   FROM tasks
                                   WHERE (@Status IS NULL OR status = @Status)
                                   ORDER BY created_at DESC
                                   LIMIT @PageSize OFFSET @Offset
                                   """;
    
    internal const string GetScheduledForRetry = """
                                   SELECT id, type, payload, status, priority,
                                          retry_count AS RetryCount,
                                          max_retries AS MaxRetries,
                                          error_message AS ErrorMessage,
                                          scheduled_at AS ScheduledAt,
                                          created_at AS CreatedAt,
                                          updated_at AS UpdatedAt
                                   FROM tasks
                                   WHERE status = 'Retrying'
                                     AND scheduled_at <= @Now
                                   ORDER BY priority DESC, scheduled_at ASC
                                   LIMIT 100
                                   """;

    internal const string GetStalledTasks = """
                                     SELECT id, type, payload, status, priority,
                                            retry_count AS RetryCount,
                                            max_retries AS MaxRetries,
                                            error_message AS ErrorMessage,
                                            scheduled_at AS ScheduledAt,
                                            created_at AS CreatedAt,
                                            updated_at AS UpdatedAt
                                     FROM tasks
                                     WHERE status = 'Running'
                                       AND updated_at < @StalledBefore
                                     """;
}