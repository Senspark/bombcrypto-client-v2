# Scribe's Journal

## Ambiguities & Unclear Logic

1. **Undocumented Configuration Context**: `AppConfig.json` holds all the sensitive data but there is no documentation specifying what fields are required in the sample or what the expected JSON structure must be to achieve a successful initialization.
2. **Exception Handling Ambiguity**: Missing explicit definition of what triggers `NoInternetException` across all external APIs. The exception is caught but the failure conditions (e.g., timeout duration, specific HTTP error codes) are not well documented, which creates a gap in writing Troubleshooting manuals.
3. **Magic Numbers in API Limits**: SmartFoxServer (SFS) pagination payloads use hardcoded limits in various pagination requests. It is unclear if these limits are enforced client-side or dynamically fetched from the server config.