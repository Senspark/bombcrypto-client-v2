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

---
*Last Updated: 2026-04-28 by AI (Antigravity)*
