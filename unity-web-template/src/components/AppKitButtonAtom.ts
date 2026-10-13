import { atom } from 'jotai';

export type AppKitButtonState = {
  showCustomUI: boolean;
  network?: string;
  address?: string;
  showFakeConnect?: boolean;
  // Logged in with a wallet: network/address are the ones this tab plays with
  isWallet?: boolean;
  // Saved profile the login popup is asking about, shown instead of the live wallet
  saved?: { network: string; address: string };
};

export const appKitButtonAtom = atom<AppKitButtonState>({
  showCustomUI: false,
  network: undefined,
  address: undefined,
  showFakeConnect: true,
});
