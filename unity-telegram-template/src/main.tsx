import {createRoot} from 'react-dom/client'
import App from './App.tsx'
import './index.css'
import './main.css'
import {TonConnectUIProvider} from "@tonconnect/ui-react";

const manifestUrl = 'https://game.bombcrypto.io/tonconnect-manifest2.json'

createRoot(document.getElementById('root')!).render(
    <TonConnectUIProvider manifestUrl={manifestUrl}>
        <App/>
    </TonConnectUIProvider>,
)
