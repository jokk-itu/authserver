Push-Location ../Authorization/Infrastructure
dotnet ef migrations remove --"Data Source=./Identity.db"
Pop-Location