﻿using System.Data;
using System.Threading.Tasks;
using NpgsqlTypes;
using NUnit.Framework;
using static Npgsql.Tests.TestUtil;

namespace Npgsql.Tests.Types;

public class JsonPathTests(MultiplexingMode multiplexingMode) : MultiplexingTestBase(multiplexingMode)
{
    static readonly object[] ReadWriteCases =
    [
        new object[] { "'$'", "$" },
        new object[] { "'$\"varname\"'", "$\"varname\"" }
    ];

    [Test]
    [TestCase("$")]
    [TestCase("$\"varname\"")]
    public async Task JsonPath(string jsonPath)
    {
        using var conn = await OpenConnectionAsync();
        MinimumPgVersion(conn, "12.0", "The jsonpath type was introduced in PostgreSQL 12");
        await AssertType(
            jsonPath, jsonPath, "jsonpath", NpgsqlDbType.JsonPath, isDefaultForWriting: false, isNpgsqlDbTypeInferredFromClrType: false,
            inferredDbType: DbType.Object);
    }

    [Test]
    [TestCaseSource(nameof(ReadWriteCases))]
    public async Task Read(string query, string expected)
    {
        using var conn = await OpenConnectionAsync();
        MinimumPgVersion(conn, "12.0", "The jsonpath type was introduced in PostgreSQL 12");

        using var cmd = new NpgsqlCommand($"SELECT {query}::jsonpath", conn);
        using var rdr = await cmd.ExecuteReaderAsync();

        rdr.Read();
        Assert.That(rdr.GetFieldValue<string>(0), Is.EqualTo(expected));
        Assert.That(rdr.GetTextReader(0).ReadToEnd(), Is.EqualTo(expected));
    }

    [Test]
    [TestCaseSource(nameof(ReadWriteCases))]
    public async Task Write(string query, string expected)
    {
        using var conn = await OpenConnectionAsync();
        MinimumPgVersion(conn, "12.0", "The jsonpath type was introduced in PostgreSQL 12");

        using var cmd = new NpgsqlCommand($"SELECT 'Passed' WHERE @p::text = {query}::text", conn) { Parameters = { new NpgsqlParameter("p", NpgsqlDbType.JsonPath) { Value = expected } } };
        using var rdr = await cmd.ExecuteReaderAsync();

        Assert.True(rdr.Read());
    }
}
