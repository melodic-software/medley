---
paths:
  - "**/*.cs"
  - "**/*.ts"
  - "**/*.tsx"
  - "**/*.js"
  - "**/*.jsx"
  - "**/*.json"
  - "**/*.yaml"
  - "**/*.yml"
  - "**/*.bicep"
  - "**/*.tf"
  - "**/*.ps1"
  - "**/*.sh"
  - "**/*.pfx"
  - "**/*.pem"
  - "**/*.key"
  - "**/*.cer"
  - "**/*.env*"
  - "**/Dockerfile*"
---

<!-- ~300 tokens -->

# Secrets Handling

## Never Commit Secrets

The following must NEVER be committed to the repository:

- Connection strings with credentials
- API keys and tokens
- Certificates and private keys
- Passwords and secrets
- OAuth client secrets

## Allowed Patterns

1. **User Secrets (Development)**
   ```bash
   dotnet user-secrets set "ConnectionStrings:Default" "Server=..."
   ```

2. **Environment Variables**
   ```csharp
   builder.Configuration.AddEnvironmentVariables();
   ```

3. **Azure Key Vault (Production)**
   ```csharp
   builder.Configuration.AddAzureKeyVault(vaultUri, credential);
   ```

4. **Placeholder values in appsettings.json**
   ```json
   {
     "ConnectionStrings": {
       "Default": "Server=localhost;Database=Medley;Integrated Security=true"
     }
   }
   ```

## Detection & Scanning

**Enable automated secret scanning in CI/CD:**
- GitHub secret scanning (enabled for public repos)
- GitHub Advanced Security (push protection)
- Pre-commit hooks with gitleaks or detect-secrets

These patterns trigger security warnings:

- `password=` followed by non-placeholder values
- `secret` or `key` followed by base64 or hex strings
- Bearer tokens, JWT secrets
- AWS/Azure/GCP credential patterns

**Response**: Treat any detected secret as immediately compromised - revoke and rotate.

## CI/CD

- Use GitHub Actions secrets for CI credentials
- Use Azure OIDC (workload identity) for deployments - no secrets needed
- Never echo or log secret values

## Prohibited

- Hardcoded secrets in source code
- Secrets in appsettings.json (except localhost development placeholders)
- Committing `.env` files with real credentials
- Logging or echoing secret values
- Storing secrets in plain text anywhere in the repository
- Disabling secret scanning or push protection
