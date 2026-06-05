using System.Data;
using System.Text.Json;
using Dapper;
using Npgsql;
using NpgsqlTypes;

namespace ReliableBack.Infrastructure.Persistence.Repositories;

internal sealed class JsonDocumentTypeHandler : SqlMapper.TypeHandler<JsonDocument>
{
    public override JsonDocument Parse(object value)
    {
        return value switch
        {
            JsonDocument document => document,
            string json => JsonDocument.Parse(json),
            _ => JsonDocument.Parse(value.ToString() ?? "{}")
        };
    }

    public override void SetValue(IDbDataParameter parameter, JsonDocument? value)
    {
        parameter.Value = value is null
            ? DBNull.Value
            : value.RootElement.GetRawText();

        if (parameter is NpgsqlParameter npgsqlParameter)
        {
            npgsqlParameter.NpgsqlDbType = NpgsqlDbType.Jsonb;
        }
    }
}