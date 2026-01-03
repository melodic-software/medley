# Contributing to Medley

## Development Workflow

### Branching Strategy: Trunk-Based Development

We follow **trunk-based development** with short-lived feature branches:

1. `main` is always deployable
2. Feature branches are short-lived (< 2 days)
3. All changes go through Pull Requests
4. Merge via squash to maintain linear history

### Branch Naming Convention

```
<type>/<short-description>
```

Examples:
- `feat/user-authentication`
- `fix/login-redirect-bug`
- `docs/api-documentation`
- `chore/update-dependencies`

### Commit Messages: Conventional Commits

All commits must follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

**Types:**
| Type | Description |
|------|-------------|
| `feat` | New feature |
| `fix` | Bug fix |
| `docs` | Documentation only |
| `style` | Formatting, no code change |
| `refactor` | Code change, no new feature or fix |
| `perf` | Performance improvement |
| `test` | Adding/updating tests |
| `chore` | Maintenance tasks |
| `build` | Build system changes |
| `ci` | CI/CD changes |

**Examples:**
```
feat(auth): add passkey authentication support
fix(blazor): resolve hydration issue on login page
docs: update README with deployment instructions
chore: update NuGet packages to latest versions
```

### Pull Request Process

1. Create a feature branch from `main`
2. Make changes with conventional commits
3. Push and open a Pull Request
4. Ensure CI passes (build + tests)
5. Request review (when team grows)
6. Squash and merge to `main`
7. Delete feature branch

### Code Quality Requirements

- All tests must pass
- No new analyzer warnings (treated as errors)
- Architecture tests must pass
