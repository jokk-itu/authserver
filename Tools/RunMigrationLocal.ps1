Push-Location ../Authorization/Infrastructure
dotnet ef database update -- "Data Source=./Identity.db"
Pop-Location