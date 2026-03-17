# 🏛 Architecture Manual

Welcome to the Bombcrypto Game Client Architecture Manual. Here we define how the frontend game interacts with the Web3 ecosystem and backend APIs.

## 🗺 System Context (Level 1) & Containers (Level 2)

```mermaid
C4Container
    title Container diagram for Bombcrypto Client Architecture

    Person(user, "Gamer", "A player with a Web3 wallet.")

    System_Boundary(client_boundary, "Bombcrypto Client Ecosystem") {
        Container(unity_app, "Unity Game App", "C#, Unity WebGL", "The core gameplay loop and state rendering.")

        Container(web_template, "Unity Web Template", "TypeScript, Vite, ethers.js", "Handles EVM (Binance, Polygon) wallet interactions via Window object.")
        Container(solana_template, "Solana Template", "TypeScript", "Handles Solana wallet interactions.")
        Container(ton_template, "TON/Telegram Template", "TypeScript", "Handles Telegram Mini App & TON wallets.")

        Rel(unity_app, web_template, "Calls WebGL JSBridge", "jslib")
        Rel(unity_app, solana_template, "Calls WebGL JSBridge", "jslib")
        Rel(unity_app, ton_template, "Calls WebGL JSBridge", "jslib")
    }

    System_Ext(wallet, "Web3 Wallets", "Browser Extensions / Mobile Apps")
    System_Ext(game_server, "Authoritative Game Server", "SmartFoxServer, Node.js, etc.")
    System_Ext(rpc_nodes, "RPC Nodes", "Blockchain Data Providers")

    Rel(user, unity_app, "Plays game via Browser")
    Rel(user, wallet, "Signs transactions")

    Rel(unity_app, game_server, "Syncs state & API requests", "HTTPS/WSS")

    Rel(web_template, wallet, "Requests accounts & signatures", "JSON-RPC")
    Rel(web_template, rpc_nodes, "Queries blockchain state", "JSON-RPC")
```

---

## ⚙️ Core Unity Patterns

### Entity Component System (ECS)
The core logic of the game utilizes a custom **Entity Component System (ECS)** pattern to manage vast numbers of interactive objects (heroes, bombs, blocks) efficiently.

```mermaid
classDiagram
    direction TB
    class IEntity {
        <<interface>>
        +GetComponent<T>() T
    }

    class Entity {
        -IndexTree spatialNode
        -IEntityManager manager
    }

    class IComponent {
        <<interface>>
        +Update()
    }

    IEntity <|-- Entity
    Entity *-- IComponent : Contains many
```

**Location**: `Assets/Scripts/Engine/Entities/`
*Note*: Properties are lazily loaded to minimize `GetComponent` overhead on the hot path.

### WebGL Bridges & Services
Unity communicates with external Web3 components using a **Service/Bridge** pattern.

- `IBlockchainManager`: Defines the interface for blockchain operations (e.g., `UpgradeHero`, `FuseHero`).
- `WebGLBlockchainManager`: Implementations mapped directly to `.jslib` files, which bridge C# calls out to the TypeScript browser environment.
- **Location**: `Assets/Scripts/Services/Blockchain/`

---

## 🔄 Sequence: Example Blockchain Transaction Flow

When a user initiates an action requiring a blockchain transaction (e.g., fusing two heroes):

```mermaid
sequenceDiagram
    participant User as Player
    participant Unity as Unity Game (C#)
    participant Queue as FusionQueueManager
    participant Bridge as WebGL jslib (JS)
    participant Web as web-template (TS)
    participant Wallet as Web3 Wallet
    participant Node as RPC Node

    User->>Unity: Clicks "Fuse"
    Unity->>Queue: Enqueue Fusion Operation
    Queue->>Unity: Set State to PROCESSING
    Unity->>Bridge: CallFuse(heroA, heroB)
    Bridge->>Web: invoke Fusion Smart Contract
    Web->>Wallet: Request Signature (EIP-1559)
    Wallet-->>User: Prompt for Approval
    User->>Wallet: Signs Transaction
    Wallet-->>Web: Returns Signed Tx
    Web->>Node: Broadcast Tx
    Node-->>Web: Return Tx Hash
    Web-->>Bridge: SendTxHash(hash)
    Bridge-->>Unity: Receive Tx Hash (UI Update: "Pending")
    Node-->>Web: Tx Confirmed/Mined
    Web-->>Bridge: TxSuccess()
    Bridge-->>Unity: Resolve Fusion UniTask
    Unity->>Queue: Dequeue & Set State to IDLE
```

---

## 📁 Key Directories

| Directory | Description |
|-----------|-------------|
| `Assets/Scripts/Services/` | Core API Managers (`DefaultApiManager`, WebGL Bridges). Includes `Utils.cs` with Data Redaction. |
| `Assets/Scripts/Engine/` | Core Gameplay mechanics and Custom ECS. |
| `Assets/Scripts/Config/` | Configurations and parsers for `AppConfig.json`. |
| `Assets/Editor/Tests/` | PlayMode and EditMode Unity Test files. |
| `unity-web-template/` | Primary TS web application for EVM blockchain interactions (ethers.js v6). |
| `unity-solana-template/` | TS integration for Solana network and wallets. |
| `unity-telegram-template/` | TON specific integration for Telegram mini-apps. |

---

## 🛡 Security Practices

1. **Parameter Sanitization**: All user inputs in URLs (wallets, emails) must be sanitized using `Uri.EscapeDataString()` within Service Managers.
2. **Log Redaction**: Before logging any HTTP/WSS payloads or responses, `Utils.RedactSensitiveData()` is invoked to remove JWTs, private keys, and signatures.
3. **Exact Allowances**: Using "Approve Maximum" is deprecated. Clients implement `ensureAllowance` to approve the exact amount required per transaction.
