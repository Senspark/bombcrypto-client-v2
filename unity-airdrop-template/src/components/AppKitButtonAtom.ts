import { atom } from 'jotai';

export type AppKitButtonState = {
  showCustomUI: boolean;
  network?: string;
  address?: string;
};

export const appKitButtonAtom = atom<AppKitButtonState>({
  showCustomUI: false,
  network: undefined,
  address: undefined,
});

