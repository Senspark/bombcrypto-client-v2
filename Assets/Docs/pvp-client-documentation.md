# PvP Wagered System - Unity Client Documentation

## 🎮 Overview
Documentation for the PvP Wagered System client-side implementation in Unity (C#).

## 🧩 Key Scripts
- **PvpModeSelector.cs**: Handles the selection between Free and Wagered modes. Manages Wager Tier buttons and UI overlays.
- **PvpChatPanel.cs**: Implements the multi-tab chat system (Global/Room) with rate-limiting feedback.
- **DialogPvpWagerDisclaimer.cs**: Legal disclaimer popup that must be accepted before entering any wagered match.
- **PvpLobbyManager.cs**: Manages the transition to the match scene and updates online player statistics.

## 🔄 Lifecycle
1. **Lobby Entry**: Join SFS Room -> Update Stats -> Initialize Chat.
2. **Mode Selection**: Choose Mode -> Toggle Wagered -> Select Tier.
3. **Disclaimer**: Accept Terms -> Join Queue.
4. **Match Finish**: Display signed results -> Update rewards locally.

## 🧪 Verification (Phase 4)
- **Manual Audit**: Verified event-driven UI updates (`OnModeSelected`, `OnWagerModeChanged`).
- **Integration**: Validated SFS2X extension request flow for chat and matchmaking.

## 🛡️ Security & Network Isolation
As of Version 2.1, the system enforces **Strict Network Isolation**:
- **Rule**: A player can only join a PvP match using a token that belongs to their account's native network (BSC or Polygon).
- **Validation**:
    - Backend (`LegacyUserController`) validates the `wagerToken.network` against `userInfo.dataType`.
    - Matchmaker matches players ONLY with others on the same `network`.
    - **No Swaps**: The system does NOT support cross-chain matching or token swapping.
- **Anti-Cheat**: Real-time position and speed validation is active. Hero speed is capped at `10.0` units/sec unless buffed by specific map items.

---
*Last Updated: 2026-04-29 by AI (Antigravity)*
