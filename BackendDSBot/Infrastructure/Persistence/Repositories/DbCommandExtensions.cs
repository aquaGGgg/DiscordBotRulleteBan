using System.Data.Common;

namespace Infrastructure.Persistence.Repositories;

internal static class DbCommandExtensions
{
    public static async Task EnsureOpenAsync(this DbConnection conn, CancellationToken ct)
    {
        if (conn.State != System.Data.ConnectionState.Open)
            await conn.OpenAsync(ct);
    }

    public static string? GetNullableString(this DbDataReader r, string name)
    {
        var i = r.GetOrdinal(name);
        return r.IsDBNull(i) ? null : r.GetString(i);
    }

    public static DateTimeOffset? GetNullableDateTimeOffset(this DbDataReader r, string name)
    {
        var i = r.GetOrdinal(name);
        return r.IsDBNull(i) ? null : r.GetFieldValue<DateTimeOffset>(i);
    }
}
