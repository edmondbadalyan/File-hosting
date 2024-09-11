# Как запустить сервак
2. `docker-compose build`
3. `docker-compose run -d`
4. Перейти в папку с проектом
5. На ЛОКАЛЬНОЙ машине - `dotnet-ef database update --connection 'Data Source=localhost,1434;Initial Catalog=Hosting_TestDb;User Id=sa;Password=StrongPassword123!;TrustServerCertificate=True;Encrypt=False;'`
6. `docker-compose down`
7. `docker-compose up`
8. ???
9. **PROFIT!**

P.S.
Подключаться к серверу через свой айпи и порт 8080
