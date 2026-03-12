# Bombcrypto Game Client

## Overview

This project is the **Game Client** for the **Bombcrypto** project.  
We have decided to open-source it under the **AGPL (GNU Affero General Public License)**.

Please note that this project **cannot operate as a standalone application**.  
A compatible **server is required** for the client to function correctly.

Most sensitive credentials and configuration values have been **intentionally removed**.  
These values must be provided before the project can be fully compiled,  
or they may be bypassed depending on your experimentation needs.

We will continue to provide updates to minimize friction during setup and testing.

---

## Requirements

- **Unity**: 2022.3  
- **Operating System**: macOS  
- **Target Platform**: WebGL  

---

## Initial Setup

```bash
# AppConfig.json is the main configuration file for the project
# We are unable to provide these values until all related projects are fully open-sourced
cp Assets/Resources/configs/AppConfig.json.sample Assets/Resources/configs/AppConfig.json
```

---

## Connecting to Server

**Prerequisites:** A running [bombcrypto-server-v2](https://github.com/Senspark/bombcrypto-server-v2) instance. Follow the server's [README](https://github.com/Senspark/bombcrypto-server-v2?tab=readme-ov-file#bombcrypto-server-v2) for setup instructions.

Before connecting, configure `Assets/Resources/configs/AppConfig.json` and `unity-web-template/.env` with the recommended values from the server's [Unity Client Configuration](https://github.com/Senspark/bombcrypto-server-v2?tab=readme-ov-file#6-unity-client-configuration-optional) section.

### Method 1: From Unity Editor

1. Open `ConnectScene` (located at `Assets/Scenes/ConnectScene/ConnectScene.unity`)
2. Press **Play** — it will automatically connect and log you into the SmartFox Server

### Method 2: From WebGL Build

1. In Unity, build the WebGL project: select the `Build` folder as the output directory, name it `webgl` (or any name you prefer)
2. Run the setup script:
   ```bash
   (cd unity-web-template && bash setup.sh)
   ```
3. When prompted, input the folder name: `webgl` (the name you set during the build step)
4. Install dependencies (if not already installed):
   ```bash
   (cd unity-web-template && npm i)
   ```
5. Start the dev server:
   ```bash
   (cd unity-web-template && npm run start)
   ```
