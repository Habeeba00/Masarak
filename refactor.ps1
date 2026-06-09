$replacements = @{
    "namespace Masarak.Models" = "namespace Masarak.Domain.Entities"
    "using Masarak.Models;" = "using Masarak.Domain.Entities;"
    '"Masarak.Models.' = '"Masarak.Domain.Entities.'
    
    "namespace Masarak.DTOs" = "namespace Masarak.Application.DTOs"
    "using Masarak.DTOs;" = "using Masarak.Application.DTOs;"
    
    "namespace Masarak.Services" = "namespace Masarak.Application.Interfaces"
    "using Masarak.Services;" = "using Masarak.Application.Interfaces;`nusing Masarak.Infrastructure.Services;"
    
    "namespace Masarak.Data" = "namespace Masarak.Infrastructure.Persistence"
    "using Masarak.Data;" = "using Masarak.Infrastructure.Persistence;"
    
    "namespace Masarak.Seeders" = "namespace Masarak.Infrastructure.Persistence.Seeders"
    "using Masarak.Seeders;" = "using Masarak.Infrastructure.Persistence.Seeders;"
    
    "namespace Masarak.Migrations" = "namespace Masarak.Infrastructure.Persistence.Migrations"
    "using Masarak.Migrations;" = "using Masarak.Infrastructure.Persistence.Migrations;"
    
    "namespace Masarak.Configurations" = "namespace Masarak.Infrastructure.Configurations"
    "using Masarak.Configurations;" = "using Masarak.Infrastructure.Configurations;"
    
    "namespace Masarak.Controllers" = "namespace Masarak.API.Controllers"
    "using Masarak.Controllers;" = "using Masarak.API.Controllers;"
    
    "namespace Masarak.Policies" = "namespace Masarak.API.Policies"
    "using Masarak.Policies;" = "using Masarak.API.Policies;"
    
    "namespace Masarak.Extensions" = "namespace Masarak.API.Extensions"
    "using Masarak.Extensions;" = "using Masarak.API.Extensions;"
}

$files = Get-ChildItem -Path "D:\ITI\GradProj\Masarak" -Recurse -Include *.cs, *.cshtml, Program.cs | Where-Object { $_.FullName -notmatch "\\obj\\" -and $_.FullName -notmatch "\\bin\\" }

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $modified = $false

    foreach ($key in $replacements.Keys) {
        if ($content -match [regex]::Escape($key)) {
            $content = $content -replace [regex]::Escape($key), $replacements[$key]
            $modified = $true
        }
    }

    # Fix specific interface implementations namespace mismatch
    if ($file.FullName -match "Infrastructure\\Services") {
        if ($content -match "namespace Masarak.Application.Interfaces") {
            $content = $content -replace "namespace Masarak.Application.Interfaces", "namespace Masarak.Infrastructure.Services"
            $modified = $true
        }
    }

    if ($modified) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        Write-Host "Updated: $($file.FullName)"
    }
}
