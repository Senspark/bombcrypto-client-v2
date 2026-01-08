import {SolanaAdapter} from '@reown/appkit-adapter-solana/react'
import {PhantomWalletAdapter, SolflareWalletAdapter} from '@solana/wallet-adapter-wallets'
import {createAppKit} from "@reown/appkit/react";
import {solanaDevnet} from "@reown/appkit/networks";
import {EnvConfig} from "../configs/EnvConfig.ts";

let initialized = false;

export default function AppKitInitializer() {
    if (initialized) {
        return;
    }
    initialized = true;
    console.log('called AppKitInitializer');
    const solanaWeb3JsAdapter = new SolanaAdapter({
        wallets: [new PhantomWalletAdapter(), new SolflareWalletAdapter()]
    })
    const projectId = EnvConfig.walletProjectId()
    const metadata = {
        name: 'AppKit',
        description: 'AppKit Solana Example',
        url: 'https://reown.com/appkit',
        icons: ['https://assets.reown.com/reown-profile-pic.png']
    }
    createAppKit({
        adapters: [solanaWeb3JsAdapter],
        networks: [solanaDevnet],
        metadata,
        projectId,
        features: {
            analytics: false
        }
    });
};