# Database Migrations

This folder contains all database migration scripts.

## Naming Convention

```
V{version}__{description}.sql
```

- **Version**: Zero-padded sequential number (001, 002, 003...)
- **Description**: Snake_case description of change

## Examples

- `V001__Initial_Schema.sql`
- `V002__Add_Users_Table.sql`
- `V003__Add_Email_Index.sql`

## Adding a New Migration

1. Determine the next version number
2. Create file: `V{next}__{description}.sql`
3. Write idempotent SQL (use `IF NOT EXISTS` where possible)
4. Test in development environment
5. Commit and push

## EF Core Integration

Generate SQL from EF Core migrations:

```bash
dotnet ef migrations script -o migrations/V{version}__{description}.sql
```

See [docs/database-migrations.md](../docs/database-migrations.md) for full documentation.
