### Deploy to local testnet

```bash
cd contracts
forge c --private-key=0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80 --broadcast contracts/RivalsToken.sol:RivalsToken --constructor-args 0xf39Fd6e51aad88F6F4ce6aB8827279cffFb92266 && cp out/RivalsToken.sol/RivalsToken.json ../server/RivalsToken.json
```

### Deploy to Flow testnet

```bash
cd contracts
forge c --private-key=<owner-key> -r "https://testnet.evm.nodes.onflow.org" --broadcast contracts/RivalsToken.sol:RivalsToken --constructor-args <owner-addr> && cp out/RivalsToken.sol/RivalsToken.json ../server/RivalsToken.json
forge verify-contract \
  --rpc-url https://testnet.evm.nodes.onflow.org/ \
  --verifier blockscout \
  --verifier-url 'https://evm-testnet.flowscan.io/api/' \
  <contract-addr> contracts/RivalsToken.sol:RivalsToken
```

### Get `$RIVAL` token balance

```bash
cast call 0xe7f1725E7734CE288F8367e1Bb143E90bb3F0512 "balanceOf(address)(uint256)" "0x70997970C51812dc3A010C7d01b50e0d17dc79C8"
```
