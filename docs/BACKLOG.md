# Backlog - Future Considerations

This document tracks future enhancements and configurations to revisit as the project evolves.

---

## Immediate (After First PR)

### Add Status Checks to Branch Ruleset
- [ ] After first PR runs CI, add required status checks to the `main` branch ruleset
- Navigate to: Settings → Rules → Rulesets → `main`
- Enable "Require status checks to pass"
- Add checks: `build`, `validate`
- Enable "Require branches to be up to date before merging"

---

## Short-Term Enhancements

### Branch Ruleset - Additional Settings
- [ ] Consider enabling "Require conversation resolution before merging"
- [ ] Consider enabling "Dismiss stale pull request approvals when new commits are pushed"
- [ ] Enable "Require review from Code Owners" once CODEOWNERS patterns are finalized

### Copilot Code Review Ruleset
- [ ] Review and customize the auto-generated Copilot review ruleset
- [ ] Consider adding custom review instructions

---

## Team Growth Considerations

### When Adding Team Members
- [ ] Increase "Required approvals" in branch ruleset from 0 to 1+
- [ ] Update CODEOWNERS with specific team/module ownership
- [ ] Consider enabling "Require approval of the most recent reviewable push"
- [ ] Set up team-based review assignments

### Code Review Process
- [ ] Define review guidelines in CONTRIBUTING.md
- [ ] Consider enabling merge queue for high-traffic periods

---

## Security Enhancements (Currently Skipped - Evaluate Later)

### Require Signed Commits
- **Status**: Not enabled (adds friction for solo dev)
- **Revisit when**: Team grows or compliance requires it
- **How**: Branch ruleset → Enable "Require signed commits"

### Private Repository Considerations
If repo becomes private again:
- [ ] Secret Protection becomes paid (~$19/committer/month)
- [ ] Code Security becomes paid (~$30/committer/month)
- [ ] Evaluate cost vs. benefit at that time

---

## CI/CD Pipeline Enhancements

### Deployment Workflows
- [ ] Create `.github/workflows/deploy.yml` for CD pipeline
- [ ] Implement build-once-deploy-many pattern
- [ ] Configure Azure deployment (App Service / Container Apps)

### Quality Gates
- [ ] Add code coverage reporting
- [ ] Set minimum coverage thresholds
- [ ] Add architecture tests (ArchUnitNET)

### Automated CHANGELOG
- [ ] Add `release-please` or `semantic-release` to auto-generate CHANGELOG.md from conventional commits
- [ ] Configure to run on merge to `main`
- Reference: [release-please](https://github.com/googleapis/release-please)

### Security Scanning
- [ ] Review CodeQL findings once PRs start flowing
- [ ] Configure custom CodeQL queries if needed
- [ ] Set up SARIF upload for IDE integration

---

## Infrastructure

### Azure Setup
- [ ] Create Azure resource groups (staging, production)
- [ ] Set up Azure Key Vault for secrets
- [ ] Set up Application Insights for monitoring

### GitHub OIDC for Azure Authentication (Recommended - No Secrets!)

Configure [Workload Identity Federation](https://learn.microsoft.com/en-us/entra/workload-id/workload-identity-federation) for secure, secretless deployments:

1. **Create Microsoft Entra Application**
   - Register app in Azure portal or via CLI
   - Note the **Client ID**, **Subscription ID**, and **Tenant ID**

2. **Configure Federated Credentials** (3 needed for full CI/CD)
   - `Environment`: entity type `Environment`, value `Production`
   - `Environment`: entity type `Environment`, value `Staging`
   - `Branch`: entity type `Branch`, value `main`

3. **Assign Azure Role**
   - Grant `Contributor` role (or custom role) to the app on target resource group(s)

4. **Add GitHub Secrets** (per environment)
   - `AZURE_CLIENT_ID` - Application (client) ID
   - `AZURE_TENANT_ID` - Directory (tenant) ID
   - `AZURE_SUBSCRIPTION_ID` - Subscription ID

5. **Workflow Permissions** (required in workflow YAML)
   ```yaml
   permissions:
     id-token: write
     contents: read
   ```

Reference: [Azure Login with OIDC](https://learn.microsoft.com/en-us/azure/developer/github/connect-from-azure)

### Environment Variables
- [ ] Document required environment variables
- [ ] Set up GitHub environment secrets for Staging/Production

### Gateway Architecture (YARP via Aspire)

When AppHost is implemented, add dual YARP gateways for external traffic routing:

- [ ] Add `Aspire.Hosting.Yarp` package to AppHost
- [ ] Configure App Gateway (`apps.melodicsoftware.com:5000`)
  - Routes: `/` → Blazor, `/bff/*` → BFF, `/connect/*` → IdentityServer
- [ ] Configure API Gateway (`api.melodicsoftware.com:5001`)
  - Routes: `/v1/*` → Module APIs (JWT auth)
- [ ] Configure production DNS/Azure Front Door for subdomain routing
- [ ] Configure session affinity for Blazor Server (multi-server deployments)

#### Defense-in-Depth Pattern (OWASP/Zero Trust)

| Layer | Handles |
|-------|---------|
| App Gateway | TLS, CORS, rate limiting, coarse auth, routing |
| Medley.Web (BFF) | CSRF via `.AsBffApiEndpoint()`, cookie session, fine-grained auth |
| API Gateway | TLS, rate limiting, JWT validation, routing |
| Module Services | Fine-grained authorization, domain validation, business rules |

#### Communication Channels

| Channel | Protocol | Routed Through |
|---------|----------|----------------|
| External HTTP | HTTPS | YARP Gateways |
| Internal Messaging | AMQP | RabbitMQ (not YARP) |
| Real-time | WebSocket | App Gateway (with affinity) |

#### Module Extraction Path

When extracting modules to microservices:
1. YARP routes stay stable (`/v1/orders/*` → same external contract)
2. Internal routing changes from monolith to extracted service
3. Messaging contracts unchanged (integration events via MassTransit)
4. Database migrated via Strangler Fig pattern (shadow writes → cutover)

**References:**
- [Aspire YARP Integration](https://aspire.dev/integrations/reverse-proxies/yarp/)
- [Strangler Fig Pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/strangler-fig)

---

## Documentation

### README Enhancements
- [ ] Add architecture diagram
- [ ] Add getting started guide
- [ ] Add deployment instructions

### ADRs (Architecture Decision Records)
- [ ] Set up ADR template
- [ ] Document key architectural decisions

### Documentation Freshness (Periodic Review)

Items with temporal context that may become stale:

- [ ] **README.md** - Aspire "Historical Note" (in Tech Stack section) about version renumbering
  - Review when: Aspire 14 is released
  - Action: Remove the note or update if still relevant
  - Search: `> **Historical Note**` in README.md

- [ ] **docs/feature-flags.md** - Microsoft.FeatureManagement version note (in "Version Note" blockquote)
  - Review when: Quarterly or when updating packages
  - Action: Verify current version on NuGet, update "Last verified" date
  - Search: `> **Version Note**` in docs/feature-flags.md

- [ ] **.github/CODEOWNERS:7-8** - Team creation setup note
  - Review when: Team is created in GitHub Organization
  - Action: Remove setup comment once @melodic-software/core-team exists

- [ ] **.github/ISSUE_TEMPLATE/1-bug-report.yml:67-71** - .NET version dropdown
  - Review when: Dropping support for older .NET versions
  - Action: Remove unsupported versions from dropdown

---

## AI Agent Autonomy Progression

### Phase 1: Current (Human-in-the-loop)
- AI agents create PRs
- Human reviews and approves all merges
- All status checks must pass

### Phase 2: Partial Autonomy
- [ ] Enable auto-merge for `chore`, `docs`, `style` commit types
- [ ] Require all status checks pass
- [ ] Maintain human review for `feat`, `fix`, `refactor`

### Phase 3: Full Autonomy (Future)
- [ ] Auto-merge for all types if comprehensive checks pass
- [ ] Requires robust test coverage and architecture tests
- [ ] Consider adding AI-specific review rules

---

*Last updated: 2026-01-04*
