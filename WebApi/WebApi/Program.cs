using System.Data;
using Dapper;
using MySqlConnector;
using BCrypt.Net;


var builder = WebApplication.CreateBuilder(args);

// CORS if you’ll call this from a browser app
builder.Services.AddCors(o =>
{
    o.AddDefaultPolicy(p => p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
});

builder.Services.AddScoped<IDbConnection>(_ =>
    new MySqlConnection(builder.Configuration.GetConnectionString("MySql")));

var app = builder.Build();
app.UseCors();
app.MapGet("/", () => Results.Ok(new { status = "ok" }));

// Health
app.MapGet("/health", async (IDbConnection db) =>
{
    await db.ExecuteAsync("SELECT 1");
    return Results.Ok(new { status = "healthy" });
});

// --- Domain: Users ---------------------------------------------


app.MapGet("/users/{username}", async (string email, IDbConnection db) =>
{
    const string sql = @"SELECT Username FROM tetraversus.users
                        WHERE Username = @username;";

    var rows = await db.QueryAsync<User>(sql, new { email });
    return Results.Ok(rows);
});


// app.MapGet("/employees/{id:int}", async (int id, IDbConnection db) =>
// {
//     var row = await db.QuerySingleOrDefaultAsync<Employee>(
//         "SELECT id, first_name AS FirstName, last_name AS LastName, email FROM employees WHERE id = @id",
//         new { id });
//
//     return row is null ? Results.NotFound() : Results.Ok(row);
// });

app.MapPost("/users", async (User dto, IDbConnection db) =>
{
    // hash the password before storing
    var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

    var sql = @"
        INSERT INTO users(`Username`, `Password`)
        VALUES (@Username, @PasswordHash);";

    var id = await db.ExecuteScalarAsync<long>(sql, new
    {
        dto.Username,
        PasswordHash = passwordHash
    });

    return ;
});

// app.MapPut("/employees/{id:int}", async (int id, EmployeeUpdate dto, IDbConnection db) =>
// {
//     var sql = @"UPDATE employees
//                 SET first_name=@FirstName, last_name=@LastName, email=@Email
//                 WHERE id=@Id";
//     var count = await db.ExecuteAsync(sql, new { Id = id, dto.FirstName, dto.LastName, dto.Email });
//     return count == 0 ? Results.NotFound() : Results.NoContent();
// });
//
// app.MapDelete("/employees/{id:int}", async (int id, IDbConnection db) =>
// {
//     var count = await db.ExecuteAsync("DELETE FROM employees WHERE id=@id", new { id });
//     return count == 0 ? Results.NotFound() : Results.NoContent();
// });

// --- Safe parameterized query endpoint (read-only) ------------------
// Allows SELECT from whitelisted tables/columns and optional WHERE/ORDER BY.
// Block anything outside the allowlist to avoid SQL injection attack surface.
// var allowedTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
// {
//     "employees"
// };
//
// var allowedColumns = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
// {
//     ["employees"] = new(StringComparer.OrdinalIgnoreCase)
//     { "id", "first_name", "last_name", "email" }
// };

// app.MapPost("/query/select", async (SelectQueryRequest req, IDbConnection db) =>
// {
//     if (!allowedTables.Contains(req.Table))
//         return Results.BadRequest(new { error = "Table not allowed." });
//
//     if (req.Columns is null || req.Columns.Count == 0)
//         return Results.BadRequest(new { error = "At least one column is required." });
//
//     // Validate columns
//     foreach (var c in req.Columns)
//         if (!allowedColumns[req.Table].Contains(c))
//             return Results.BadRequest(new { error = $"Column '{c}' not allowed." });
//
//     // Optional ORDER BY validation
//     string orderBy = "";
//     if (!string.IsNullOrWhiteSpace(req.OrderBy))
//     {
//         if (!allowedColumns[req.Table].Contains(req.OrderBy))
//             return Results.BadRequest(new { error = $"ORDER BY column '{req.OrderBy}' not allowed." });
//         orderBy = $" ORDER BY `{req.OrderBy}` {(req.Desc ? "DESC" : "ASC")}";
//     }
//
//     // Build SELECT with parameterized WHERE
//     var selectCols = string.Join(", ", req.Columns.Select(c => $"`{c}`"));
//     var sql = $"SELECT {selectCols} FROM `{req.Table}`";
//
//     var dynParams = new DynamicParameters();
//     if (req.Filters is not null && req.Filters.Count > 0)
//     {
//         var clauses = new List<string>();
//         int i = 0;
//         foreach (var f in req.Filters)
//         {
//             if (!allowedColumns[req.Table].Contains(f.Column))
//                 return Results.BadRequest(new { error = $"Filter column '{f.Column}' not allowed." });
//
//             string paramName = $"p{i++}";
//             string op = f.Operator?.ToUpperInvariant() switch
//             {
//                 "=" or "<>" or ">" or "<" or ">=" or "<=" => f.Operator!,
//                 "LIKE" => "LIKE",
//                 "IN" => "IN",
//                 _ => "="
//             };
//
//             if (op == "IN")
//             {
//                 // Expect an array for IN; Dapper expands lists with IN @param
//                 dynParams.Add(paramName, f.Value);
//                 clauses.Add($"`{f.Column}` IN @{paramName}");
//             }
//             else
//             {
//                 dynParams.Add(paramName, f.Value);
//                 clauses.Add($"`{f.Column}` {op} @{paramName}");
//             }
//         }
//         sql += " WHERE " + string.Join(" AND ", clauses);
//     }
//
//     // Limit/Offset (guardrails)
//     int limit = Math.Clamp(req.Limit ?? 100, 1, 1000);
//     int offset = Math.Max(req.Offset ?? 0, 0);
//     sql += orderBy + " LIMIT @limit OFFSET @offset";
//     dynParams.Add("limit", limit);
//     dynParams.Add("offset", offset);
//
//     var rows = await db.QueryAsync(sql, dynParams);
//     return Results.Ok(rows);
// });

app.Run();

// record Employee(int Id, string FirstName, string LastName, string Email);
// record EmployeeCreate(string FirstName, string LastName, string Email);
// record EmployeeUpdate(string FirstName, string LastName, string Email);
//
// record SelectQueryRequest(
//     string Table,
//     List<string> Columns,
//     List<Filter>? Filters,
//     string? OrderBy,
//     bool Desc = false,
//     int? Limit = 100,
//     int? Offset = 0
// );
//
// record Filter(string Column, string? Operator, object? Value);

public class User
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}
