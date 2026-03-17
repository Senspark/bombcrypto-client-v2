import { describe, it, expect, vi, beforeEach } from 'vitest';
import { getDoubleGasFeeOption } from '../NetworkUtils';
import * as Storage from '../Storage';

// Mock the entire Storage module to intercept getBrowserProvider
vi.mock('../Storage', () => ({
    getBrowserProvider: vi.fn(),
}));

describe('NetworkUtils', () => {
    describe('getDoubleGasFeeOption', () => {
        beforeEach(() => {
            vi.clearAllMocks();
        });

        it('should calculate EIP-1559 30% premium correctly for Polygon (chainId 137) - Scenario A', async () => {
            // Setup fake provider for Polygon
            const mockProvider = {
                getNetwork: vi.fn().mockResolvedValue({ chainId: 137 }),
                getFeeData: vi.fn().mockResolvedValue({
                    maxPriorityFeePerGas: 100n, // Simulated fee
                    maxFeePerGas: 300n,         // Base fee would be 200n
                }),
            };

            // eslint-disable-next-line @typescript-eslint/ban-ts-comment
            // @ts-ignore - Ignore type issues with mock
            vi.mocked(Storage.getBrowserProvider).mockReturnValue(mockProvider);

            const estimateGas = 50000n;
            const result = await getDoubleGasFeeOption(estimateGas);

            // Assert EIP-1559 1.3x premium
            expect(result.maxPriorityFeePerGas).toBe(130n); // 100n * 1.3x
            // Base fee (300n - 100n = 200n). New maxFee = 200n + 130n = 330n
            expect(result.maxFeePerGas).toBe(330n);
            expect(result.gasLimit).toBe(100000n); // 50000n * 2 multiplier
        });

        it('should calculate EIP-1559 30% premium correctly for Polygon Testnet (chainId 80002)', async () => {
             // Setup fake provider for Polygon Testnet
             const mockProvider = {
                getNetwork: vi.fn().mockResolvedValue({ chainId: 80002 }),
                getFeeData: vi.fn().mockResolvedValue({
                    maxPriorityFeePerGas: 50n,
                    maxFeePerGas: 200n,         // Base fee would be 150n
                }),
            };

            // eslint-disable-next-line @typescript-eslint/ban-ts-comment
            // @ts-ignore
            vi.mocked(Storage.getBrowserProvider).mockReturnValue(mockProvider);

            const estimateGas = 10000n;
            const result = await getDoubleGasFeeOption(estimateGas);

            expect(result.maxPriorityFeePerGas).toBe(65n); // 50n * 1.3x
            // Base fee (200n - 50n = 150n). New maxFee = 150n + 65n = 215n
            expect(result.maxFeePerGas).toBe(215n);
            expect(result.gasLimit).toBe(20000n);
        });

        it('should NOT apply EIP-1559 logic and return only gasLimit for BSC (chainId 56) - Scenario B', async () => {
            // Setup fake provider for BSC
            const mockProvider = {
                getNetwork: vi.fn().mockResolvedValue({ chainId: 56 }),
                getFeeData: vi.fn().mockResolvedValue({}), // Should not be called anyway
            };

            // eslint-disable-next-line @typescript-eslint/ban-ts-comment
            // @ts-ignore
            vi.mocked(Storage.getBrowserProvider).mockReturnValue(mockProvider);

            const estimateGas = 50000n;
            const result = await getDoubleGasFeeOption(estimateGas);

            // Assert EIP-1559 is NOT activated
            expect(result.maxPriorityFeePerGas).toBeUndefined();
            expect(result.maxFeePerGas).toBeUndefined();
            expect(result.gasLimit).toBe(100000n); // 50000n * 2 multiplier
        });

        it('should handle provider errors gracefully and return fallback gasLimit', async () => {
             // Setup fake provider that throws an error
             const mockProvider = {
                getNetwork: vi.fn().mockRejectedValue(new Error('Network failure')),
            };

            // eslint-disable-next-line @typescript-eslint/ban-ts-comment
            // @ts-ignore
            vi.mocked(Storage.getBrowserProvider).mockReturnValue(mockProvider);

            const consoleSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

            const estimateGas = 50000n;
            const result = await getDoubleGasFeeOption(estimateGas);

            // Assert EIP-1559 is NOT activated and gasLimit falls back
            expect(result.maxPriorityFeePerGas).toBeUndefined();
            expect(result.maxFeePerGas).toBeUndefined();
            expect(result.gasLimit).toBe(100000n);

            expect(consoleSpy).toHaveBeenCalledWith("Error fetching fee data for EIP-1559", expect.any(Error));
            consoleSpy.mockRestore();
        });
    });
});
