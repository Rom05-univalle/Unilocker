# Script para generar hash BCrypt
Add-Type -Path "C:\Proyecto de sistemas-Unilocker\UnilockerProyecto\Unilocker.Api\bin\Debug\net8.0\BCrypt.Net-Next.dll"

$password = "Admin123!"
$hash = [BCrypt.Net.BCrypt]::HashPassword($password)

Write-Host "Password: $password"
Write-Host "Hash: $hash"
Write-Host ""
Write-Host "Actualizando en base de datos..."

# Actualizar en BD
$query = "UPDATE [User] SET PasswordHash = '$hash' WHERE Username = 'cmamani'; SELECT Username, PasswordHash FROM [User] WHERE Username = 'cmamani'"
sqlcmd -S localhost -d UnilockerDBV1 -E -Q $query
