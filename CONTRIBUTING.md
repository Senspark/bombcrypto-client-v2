# 🤝 Contributing to Bombcrypto Game Client

First off, thank you for considering contributing to Bombcrypto! It's people like you that make Bombcrypto such a great gaming experience.

This document is a set of guidelines for contributing to the open-source Bombcrypto Unity client.

---

## 💻 Developer Setup (Local Environment)

### Unity
1. Install **Unity Hub**.
2. Install Unity Version `2022.3`.
3. Clone the repository and add the project in Unity Hub.
4. Set your Build Target to **WebGL**.

### Web Templates (Frontend UI/Wallet Bridges)
To modify the wallet interactions or web-level logic, you must compile the TypeScript templates.

```bash
cd unity-web-template
npm install
npm run build-test
```
*Note: Make sure to resolve any TS linting errors before pushing.*

### Configuration
Sensitive keys are not committed.
```bash
cp Assets/Resources/configs/AppConfig.json.sample Assets/Resources/configs/AppConfig.json
```
Populate `AppConfig.json` with the required API and RPC URLs to test against your own server.

---

## 📜 Code Guidelines

- **Optimization First**: Avoid using `GetComponent<T>()` in `Update()` loops. Cache references in `Awake()` or implement lazy-loaded properties (e.g. `DamageDealer` on `Entity.cs`).
- **Security Context**: Ensure `Uri.EscapeDataString` is applied to dynamic API URL segments (wallets, parameters).
- **Log Sanitation**: Do not log raw HTTP responses containing sensitive fields. Rely on `Utils.RedactSensitiveData()`.

## 🧪 Testing

We require Unit Tests for logic modifications where applicable.

- **C# / Unity Tests**: Found in `Assets/Editor/Tests/`. Run using the Unity Test Runner window.
- **TypeScript Tests**: Use `vitest`.
  ```bash
  cd unity-web-template
  npx vitest
  ```

---

## 📬 Pull Request Process

1. **Fork the repo** and create your branch from `main`.
2. **Commit clearly**: Use standard Conventional Commits.
   - `feat: added solana wallet integration`
   - `fix: resolved memory leak in DynamicScroll`
3. **Run Pre-Commit Checks**: Ensure Unity compiles and all `npm run build-test` commands pass for web templates.
4. **Submit PR**: Provide a clear description referencing any issues your PR solves.

---

## ⚖️ Code of Conduct

By participating in this project, you are expected to uphold standards of professional behavior.
- Be welcoming and inclusive.
- Provide constructive feedback.
- Respect the licensing and terms (AGPL v3).
