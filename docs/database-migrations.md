# Database Migrations

## Philosophy

All database changes **MUST** be in source control via EF Core migrations.

## Pattern: Per-Module Ownership

Each module owns its migrations alongside its `DbContext`:

```
src/
├── Modules/
│   ├── Auth/
│   │   └── Auth.Infrastructure/
│   │       └── Data/
│   │           ├── AuthDbContext.cs
│   │           └── Migrations/           # Auth migrations
│   └── Billing/
│       └── Billing.Infrastructure/
│           └── Data/
│               ├── BillingDbContext.cs
│               └── Migrations/           # Billing migrations
```

**Why this pattern:**

- Microsoft standard (eShopOnWeb, Clean Architecture templates)
- Migrations travel with the code that uses them
- Different `DbContext` = different database = no version collisions
- `dotnet ef` CLI works without extra configuration

## Common Commands

### Create a Migration

```bash
dotnet ef migrations add AddUsersTable \
    --project src/Modules/Auth/Auth.Infrastructure \
    --startup-project src/Medley.Web
```

### Generate SQL Script (for Production)

```bash
dotnet ef migrations script --idempotent \
    --project src/Modules/Auth/Auth.Infrastructure \
    --startup-project src/Medley.Web \
    --output migration.sql
```

### Apply Migrations (Development Only)

```bash
dotnet ef database update \
    --project src/Modules/Auth/Auth.Infrastructure \
    --startup-project src/Medley.Web
```

## Production Deployment

> **Never** use `MigrateAsync()` or `Database.Migrate()` at application startup in production.

### Recommended Workflow

1. Generate idempotent SQL script in CI/CD
2. Review script (manual or automated)
3. Execute via deployment pipeline with appropriate gates
4. Monitor for issues

### Why Script-Based Deployment?

- **Auditability**: Scripts can be reviewed before execution
- **Rollback preparation**: Know exactly what changed
- **DBA gates**: Complex changes can require approval
- **Blue-green deployments**: Scripts work with zero-downtime strategies

## Best Practices

1. **One change per migration** - Easier to debug and rollback
2. **Descriptive names** - `AddEmailIndexToUsers` not `Update1`
3. **Test locally first** - Apply migrations to local database before committing
4. **Keep migrations small** - Large migrations increase deployment risk
5. **Don't modify applied migrations** - Create new migrations for fixes

## CI/CD Integration

Example GitHub Actions step:

```yaml
- name: Generate Migration Script
  run: |
    dotnet ef migrations script --idempotent \
      --project src/Modules/Auth/Auth.Infrastructure \
      --startup-project src/Medley.Web \
      --output ${{ github.workspace }}/migration.sql

- name: Upload Migration Script
  uses: actions/upload-artifact@b7c566a772e6b6bfb58ed0dc250532a479d7789f  # v6.0.0
  with:
    name: migration-script
    path: migration.sql
```

> **Note**: `actions/upload-artifact@v6` has breaking changes from earlier versions:
> - Artifact names must be unique across the entire workflow run
> - Hidden files are excluded by default (use `include-hidden-files: true` if needed)
> - Same-name artifacts from different jobs no longer merge automatically

---

*Last updated: 2026-01-04*
