# Script de prueba de API
$apiUrl = "http://localhost:5013"

Write-Host "=== Prueba de Health Check ===" -ForegroundColor Cyan
try {
    $health = Invoke-RestMethod -Uri "$apiUrl/api/health" -Method Get
    Write-Host "✓ Health Check OK" -ForegroundColor Green
    Write-Host ($health | ConvertTo-Json)
} catch {
    Write-Host "✗ Health Check FALLÓ: $_" -ForegroundColor Red
}

Write-Host "`n=== Prueba de Login ===" -ForegroundColor Cyan
try {
    $loginBody = @{
        username = "admin"
        password = "admin123"
    } | ConvertTo-Json

    $login = Invoke-RestMethod -Uri "$apiUrl/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
    Write-Host "✓ Login OK (o requiere 2FA)" -ForegroundColor Green
    Write-Host ($login | ConvertTo-Json)
    
    if ($login.token) {
        $token = $login.token
        Write-Host "`n=== Prueba de Branches (con token) ===" -ForegroundColor Cyan
        $headers = @{
            Authorization = "Bearer $token"
        }
        $branches = Invoke-RestMethod -Uri "$apiUrl/api/branches" -Method Get -Headers $headers
        Write-Host "✓ Branches OK - Total: $($branches.Count)" -ForegroundColor Green
    }
} catch {
    Write-Host "✗ Login FALLÓ: $_" -ForegroundColor Red
    Write-Host "Error completo: $($_.Exception.Message)"
}
