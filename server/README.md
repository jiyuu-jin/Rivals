## Running server

```bash
docker run -it --rm -e POSTGRES_PASSWORD=password -p 127.0.0.1:5432:5432 -v $PWD/server/pg_schema/initial.sql:/docker-entrypoint-initdb.d/initial.sql postgres
```

```bash
cd server && npm run dev
```