cd Authorization/Infrastructure
dotnet ef migrations remove -- "Server=localhost,1433;Initial Catalog=Identity;Trusted_Connection=False;User ID=sa;Password=Password12!"
cd ..\..