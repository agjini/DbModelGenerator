# DbModelGenerator

A .NET Roslyn incremental source generator that generates POCO `record` classes from SQL migration scripts.

It pairs well with [Dapper](https://github.com/DapperLib/Dapper), [DapperExtensions](https://github.com/tmsmith/Dapper-Extensions), and [DbUp](https://dbup.readthedocs.io/en/latest/) for a true **database-first** workflow.

## How it works

1. You write SQL migration scripts (CREATE TABLE, ALTER TABLE, …) organised in sub-directories under `Scripts/`.
2. DbModelGenerator reads those scripts at **compile time** (as a Roslyn analyzer) and generates one C# `record` per table.
3. Generated classes are injected directly into your compilation — no files to commit, no pre-build step to configure.

## Installation

Add the package to your `.csproj`:

```xml
<ItemGroup>
    <PackageReference Include="DbModelGenerator" Version="0.5.2"/>
</ItemGroup>
```

Then expose your SQL migration scripts as `AdditionalFiles` so the analyzer can read them:

```xml
<ItemGroup>
    <AdditionalFiles Include="Scripts\**\*"/>
</ItemGroup>
```

That's all. The generator runs automatically on every build.

## Scripts directory structure

```
MyProject/
└── Scripts/
    ├── db.json          ← optional configuration file
    ├── Global/          ← one namespace per sub-directory
    │   └── 01_create_tables.sql
    └── Tenant/
        ├── 01_create_tables.sql
        └── 02_alter_table.sql
```

- Each **sub-directory** maps to a generated C# namespace: `{ProjectName}.Generated.Db.{SubDirectoryName}`.
- Scripts inside a sub-directory are applied **in alphabetical order** (standard DbUp convention).
- The generator supports `CREATE TABLE`, `ALTER TABLE` (add/drop/rename column, add/drop constraint), and `DROP TABLE`.

### Example

Given `Scripts/Tenant/01_create_tables.sql`:

```sql
CREATE TABLE user_profile
(
    id        SERIAL       NOT NULL,
    email     VARCHAR(255) NOT NULL,
    firstName VARCHAR(255) NOT NULL,
    disabled  BOOLEAN      NOT NULL DEFAULT '0',
    PRIMARY KEY (id)
);
```

The generator produces (in `MyProject.Generated.Db.Tenant`):

```csharp
public sealed record UserProfile(
    int Id,
    string Email,
    string FirstName,
    bool Disabled
);
```

## Configuration — `Scripts/db.json`

Place an optional `db.json` file directly inside the `Scripts/` directory to customise code generation:

```json
{
  "interfaces": [
    "My.Namespace.IEntity",
    "My.Namespace.IDbEntity(created_by)"
  ],
  "primaryKeyAttribute": "My.Namespace.PrimaryKey",
  "autoIncrementAttribute": "My.Namespace.AutoIncrement",
  "suffix": "Db",
  "ignores": ["audit_log", "migration_history"]
}
```

| Field | Description |
|---|---|
| `interfaces` | List of C# interfaces that matching classes should implement (see below). |
| `primaryKeyAttribute` | Fully-qualified attribute applied to primary key properties. |
| `autoIncrementAttribute` | Fully-qualified attribute applied to auto-increment (`SERIAL`) properties. |
| `suffix` | Text appended to every generated class name (e.g. `"Db"` → `UserProfileDb`). |
| `ignores` | Table names to exclude from generation (case-insensitive). |

### Interface matching

An interface is applied to a class only when **all** the listed properties exist on the table.

```json
"interfaces": [
    "My.App.IEntity",
    "My.App.IDbEntity(created_by)"
]
```

- `My.App.IEntity` — applied to every table that has an `id` column (default property when none specified).
- `My.App.IDbEntity(created_by)` — applied only to tables that have a `created_by` column.

**Generic type parameters** — suffix a property name with `!` to use its C# type as a generic type argument:

```json
"My.App.IEntity(id!)"
```

This generates `IEntity<int>` when `id` is `SERIAL`, `IEntity<string>` when `id` is `TEXT`, etc.

### Custom scripts directory

By default the generator looks for scripts in `{ProjectDir}/Scripts`. Override this with the `ScriptsDir` MSBuild property:

```xml
<PropertyGroup>
    <ScriptsDir>Migrations</ScriptsDir>
</PropertyGroup>
```

## SQL type mapping

| SQL type | C# type |
|---|---|
| `SERIAL` / `INTEGER` / `INT` | `int` |
| `BIGSERIAL` / `BIGINT` | `long` |
| `BOOLEAN` | `bool` |
| `REAL` / `FLOAT` | `float` |
| `DECIMAL` / `NUMERIC` | `decimal` |
| `TEXT` / `VARCHAR` / `CHAR` | `string` |
| `DATE` / `TIMESTAMP` | `DateTime` |
| `UUID` | `Guid` |

Nullable columns (without `NOT NULL`) produce nullable C# types (`int?`, `string?`, …).

## Complete example

See the [`Example.Service`](./Example.Service) project for a working end-to-end setup.
