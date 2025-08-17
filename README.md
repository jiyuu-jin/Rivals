## Running server

```bash
docker run -it --rm -e POSTGRES_PASSWORD=password -p 127.0.0.1:5432:5432 -v $PWD/server/pg_schema/initial.sql:/docker-entrypoint-initdb.d/initial.sql postgres
```

```bash
cd server && npm run dev
```

```bash
cd contracts
forge c --private-key=0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80 --broadcast contracts/RivalToken.sol:RivalToken --constructor-args 0xf39Fd6e51aad88F6F4ce6aB8827279cffFb92266
```

### Get `$RIVAL` token balance

```bash
cast call 0xe7f1725E7734CE288F8367e1Bb143E90bb3F0512 "balanceOf(address)(uint256)" "0x70997970C51812dc3A010C7d01b50e0d17dc79C8"
```
