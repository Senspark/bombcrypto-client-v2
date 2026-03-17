import { describe, it, expect } from 'vitest';
import getAllRpc from '../RpcAddress';

describe('RpcAddress', () => {
    describe('getAllRpc', () => {
        it('should return multiple RPC endpoints for Polygon (chainId 137) as a fallback strategy', () => {
            const polygonRpcs = getAllRpc(137);

            // The test must fail if the array only has 1 item, proving the fallback logic is in place
            expect(Array.isArray(polygonRpcs)).toBe(true);
            expect(polygonRpcs.length).toBeGreaterThan(1);
        });

        it('should return multiple RPC endpoints for Polygon Testnet (chainId 80002)', () => {
            const amoyRpcs = getAllRpc(80002);
            expect(Array.isArray(amoyRpcs)).toBe(true);
            expect(amoyRpcs.length).toBeGreaterThan(1);
        });

        it('should return multiple RPC endpoints for BSC (chainId 56)', () => {
            const bscRpcs = getAllRpc(56);
            expect(Array.isArray(bscRpcs)).toBe(true);
            expect(bscRpcs.length).toBeGreaterThan(1);
        });

        it('should return an empty array for unknown chainId', () => {
            const unknownRpcs = getAllRpc(999999);
            expect(Array.isArray(unknownRpcs)).toBe(true);
            expect(unknownRpcs.length).toBe(0);
        });
    });
});
