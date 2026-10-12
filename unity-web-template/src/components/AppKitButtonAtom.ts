import { atom } from 'jotai';

export type AppKitButtonState = {
  showCustomUI: boolean;
  network?: string;
  address?: string;
  showFakeConnect?: boolean;
  // Logged in with a wallet: network/address are the ones this tab plays with
  isWallet?: boolean;
};

export const appKitButtonAtom = atom<AppKitButtonState>({
  showCustomUI: false,
  network: undefined,
  address: undefined,
  showFakeConnect: true,
});
