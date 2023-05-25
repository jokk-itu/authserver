Push-Location ../Authorization/Infrastructure
dotnet ef migrations add $args[0] -- "Data Source=./Identity.db"
Pop-Location