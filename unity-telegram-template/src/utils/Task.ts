import {sleep} from "../services/Utils.ts";

export async function waitUntil(condition: () => boolean, interval: number = 1000): Promise<void> {
    while (!condition()) {
        await sleep(interval);
    }
}