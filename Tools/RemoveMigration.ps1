Push-Location ../Authorization/Infrastructure
dotnet ef migrations remove -- "SqlServer" "Server=localhost,1433;Initial Catalog=Identity;Trusted_Connection=False;User ID=sa;Password=Password12!"
Pop-Location