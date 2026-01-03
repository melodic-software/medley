# Database Migration Strategy

## Philosophy

All database changes **MUST** be in source control. The approach is tool-agnostic to allow future flexibility (EF Core, DbUp, Flyway, etc.).

## Folder Structure

```
migrations/
├── V001__Initial_Schema.sql
├── V002__Add_Users_Table.sql
├── V003__Add_Email_Index.sql
└── README.md
```

## Naming Convention

```
V{version}__{description}.sql
```

- **Version**: Zero-padded sequential number (001, 002, 003...)
- **Description**: Snake_case description of change

### Examples

- `V001__Initial_Schema.sql`
- `V002__Add_Users_Table.sql`
- `V003__Add_Audit_Columns.sql`
- `V004__Create_Orders_Table.sql`

## Dual-Track Approach

### 1. EF Core Migrations (Primary for Development)

- Code-first schema changes
- Generate SQL scripts: `dotnet ef migrations script`
- Store generated SQL in `migrations/` folder

### 2. Raw SQL Scripts (For Complex Scenarios)

- Data migrations, complex transforms
- Stored in same `migrations/` folder
- Same versioning convention

## Tracking Table (Tool-Agnostic)

```sql
CREATE TABLE schema_versions (
    version VARCHAR(50) PRIMARY KEY,
    description VARCHAR(255),
    applied_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    checksum VARCHAR(64)
);
```

## Future Tool Integration

| Tool | Compatibility |
|------|---------------|
| **Flyway** | Recognizes `V{version}__` naming natively |
| **DbUp** | Can read from `migrations/` folder |
| **EF Core** | Can coexist, track separately or unified |

## Best Practices

1. **One change per migration** - Easier to debug and rollback
2. **Idempotent scripts** - Use `IF NOT EXISTS` where possible
3. **Test in staging first** - Never run untested migrations in production
4. **Keep migrations small** - Large migrations increase deployment risk
5. **Version everything** - Even seed data and configuration changes
