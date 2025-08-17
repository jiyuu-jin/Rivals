## Dependencies

- https://getfoundry.sh/

## Running server

```bash
docker run -it --rm -e POSTGRES_PASSWORD=password -p 127.0.0.1:5432:5432 -v $PWD/server/pg_schema/initial.sql:/docker-entrypoint-initdb.d/initial.sql postgres
```

```
cd server && npm run dev
```

```
cd contracts
forge c --private-key=0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80 --broadcast contracts/token.sol:RivalToken --constructor-args 1000
```
