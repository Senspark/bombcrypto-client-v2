# Polygon RPC & EIP-1559 Gas Premium Test Report

**Resumo:**
Este relatório comprova o funcionamento da matemática de EIP-1559 (prêmio de 30% na taxa da Polygon) em `getDoubleGasFeeOption` e garante que o fallback de RPC para a rede Polygon (`getAllRpc`) retorna um array válido com múltiplos endpoints em vez de um único. Os testes de simulação foram executados isoladamente sem contato com redes externas.

**Saída dos Testes (Vitest):**
```
 RUN  v3.2.4 /app/unity-web-template

 ✓ src/controllers/BlockChain/Module/RpcToken/__tests__/RpcAddress.test.ts (4 tests) 5ms
 ✓ src/controllers/BlockChain/Module/Utils/__tests__/NetworkUtils.test.ts (4 tests) 10ms

 Test Files  2 passed (2)
      Tests  8 passed (8)
   Start at  20:45:55
   Duration  582ms (transform 181ms, setup 0ms, collect 342ms, tests 15ms, environment 1ms, prepare 176ms)
```