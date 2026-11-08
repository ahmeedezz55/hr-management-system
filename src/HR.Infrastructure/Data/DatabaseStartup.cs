using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace HR.Infrastructure.Data;

/// <summary>
/// Startup-time database connectivity checks.
/// Used by the host (Program.cs) to decide between SQL Server and the
/// SQLite fallback before any services are registered.
/// </summary>
public static class DatabaseStartup
{
    /// <summary>
    /// Connection string used when no SQL Server is reachable
    /// (e.g. GitHub Codespaces without the devcontainer database sidecar).
    /// </summary>
    public const string SqliteFallbackConnectionString = "Data Source=HRSystem.db";

    /// <summary>
    /// Probes the configured SQL Server, retrying a few times so that a
    /// container/server that is still booting has a chance to become ready.
    /// </summary>
    /// <param name="connectionString">Configured SQL Server connection string (null/empty = not reachable).</param>
    /// <param name="logger">Optional logger for progress lines.</param>
    /// <param name="attempts">Connection attempts before giving up.</param>
    /// <param name="delaySeconds">Wait between attempts, letting a cold container start.</param>
    /// <param name="connectTimeoutSeconds">Hard cap per attempt so the fallback is not slow.</param>
    /// <param name="cancellationToken">Cancels probing during host shutdown.</param>
    /// <returns>true when SQL Server is reachable; false when the caller should fall back to SQLite.</returns>
    public static async Task<bool> SqlServerReachableAsync(
        string? connectionString,
        ILogger? logger = null,
        int attempts = 3,
        int delaySeconds = 5,
        int connectTimeoutSeconds = 5,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            logger?.LogWarning("لا يوجد Connection String - سيتم استخدام SQLite.");
            return false;
        }

        // Cap the per-attempt timeout so the whole probe stays bounded;
        // only append when the caller did not already set one.
        var cs = connectionString.Contains("Connect Timeout", StringComparison.OrdinalIgnoreCase)
            ? connectionString
            : $"{connectionString};Connect Timeout={connectTimeoutSeconds}";

        // Probe the *server*, not the application database: on a fresh SQL
        // Server container the app database (HRSystem) does not exist yet and
        // EF MigrateAsync will create it. Probing the app DB directly would
        // wrongly report the whole server as unreachable.
        cs = BuildProbeConnectionString(cs);

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            try
            {
                using var connection = new SqlConnection(cs);
                await connection.OpenAsync(cancellationToken);
                logger?.LogInformation("SQL Server متاح بعد {Attempt} محاولة/محاولات.", attempt);
                return true;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger?.LogWarning(
                    "محاولة {Attempt}/{Attempts} للاتصال بـ SQL Server فشلت: {Reason}",
                    attempt, attempts, ex.Message);

                if (attempt == attempts)
                {
                    logger?.LogWarning("SQL Server غير متاح - التحويل إلى احتمال القاعدي.");
                    return false;
                }

                // Give a starting SQL Server container/server a bit more time.
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
            }
        }

        return false;
    }

    /// <summary>
    /// Rewrites the connection string to open the "master" system database
    /// instead of the application database, so that a reachable server with
    /// a not-yet-created database still counts as reachable.
    /// </summary>
    private static string BuildProbeConnectionString(string connectionString)
    {
        // Match Database=HRSystem / DB name with or without spaces after '='.
        var probe = System.Text.RegularExpressions.Regex.Replace(
            connectionString,
            @"(?i)(Database|Initial Catalog)\s*=\s*[^;]*",
            "$1=master");

        if (!string.Equals(probe, connectionString, StringComparison.Ordinal))
        {
            probe += ";"; // keep key-value separation intact
        }

        return probe;
    }
}
