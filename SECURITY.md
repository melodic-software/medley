# Security Policy

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| main    | :white_check_mark: |

## Reporting a Vulnerability

We take security seriously. If you discover a security vulnerability, please report it responsibly.

### How to Report

1. **Do NOT** open a public GitHub issue for security vulnerabilities
2. Use **GitHub's private vulnerability reporting**: Navigate to the repository's **Security** tab > **Advisories** > **New draft security advisory**

### What to Include

Please provide as much of the following as possible:

- Type of issue (e.g., SQL injection, XSS, authentication bypass, etc.)
- Full paths of source file(s) related to the issue
- Location of affected source code (branch/commit or direct URL)
- Any special configuration required to reproduce
- Step-by-step instructions to reproduce
- Proof of concept or exploit code (if available, non-destructive)
- Impact assessment, including how an attacker might exploit the issue

### Response Timeline

| Action | Timeframe |
|--------|-----------|
| Acknowledgment | Within 48 hours |
| Initial assessment | Within 7 days |
| Status update | Every 7 days until resolved |
| Fix timeline | Based on severity (Critical: 7 days, High: 30 days, Medium: 90 days, Low: best effort) |

### Safe Harbor

We will not take legal action against security researchers who:

- Act in good faith to avoid privacy violations, data destruction, or service disruption
- Provide us reasonable time to address the issue before public disclosure
- Do not exploit the vulnerability beyond what is necessary to demonstrate it

### Preferred Languages

We prefer all communications to be in English.

## Security Measures

This repository implements:

- **GitHub Secret Scanning** - Detects accidentally committed secrets
- **Dependabot Security Updates** - Automated dependency patching
- **CodeQL Analysis** - Static code analysis for security vulnerabilities
- **Branch Protection** - All changes require PR review
- **SHA-Pinned Actions** - Supply chain security for CI/CD workflows

## For .NET/NuGet Dependencies

If you discover a vulnerability in a NuGet package we use:

1. Report to the package maintainer first
2. If unresponsive after 30 days, notify us
3. We will evaluate workarounds or package alternatives
