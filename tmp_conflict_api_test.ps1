$base='http://127.0.0.1:5010'
$loginBody = @{ email='admin@company.com'; password='Password123!' } | ConvertTo-Json
Invoke-RestMethod -Method Post -Uri 'http://127.0.0.1:5001/api/Utility/update-passwords' -TimeoutSec 15 | Out-Null
$login = Invoke-RestMethod -Method Post -Uri 'http://127.0.0.1:5001/api/Auth/login' -ContentType 'application/json' -Body $loginBody -TimeoutSec 20
$token = $login.data.accessToken
$h = @{ Authorization = "Bearer $token"; 'Content-Type'='application/json' }
$id = [guid]::NewGuid().ToString()

$tests = @(
    @{n='GET /api/ProductBatch/batches';m='GET';u="$base/api/ProductBatch/batches"},
    @{n='GET /api/restock-requests';m='GET';u="$base/api/restock-requests"},
    @{n='GET /api/Transfer/transfers';m='GET';u="$base/api/Transfer/transfers"},
    @{n='GET /api/ProductBatch/batch/{id}';m='GET';u="$base/api/ProductBatch/batch/$id"},
    @{n='GET /api/restock-requests/{id}';m='GET';u="$base/api/restock-requests/$id"},
    @{n='GET /api/Transfer/transfer/{id}';m='GET';u="$base/api/Transfer/transfer/$id"},
    @{n='POST /api/ProductBatch/batch/allocate';m='POST';u="$base/api/ProductBatch/batch/allocate";b='{}'},
    @{n='POST /api/restock-requests';m='POST';u="$base/api/restock-requests";b='{}'},
    @{n='POST /api/Transfer/transfer';m='POST';u="$base/api/Transfer/transfer";b='{}'},
    @{n='PUT /api/restock-requests/{id}/approve';m='PUT';u="$base/api/restock-requests/$id/approve"},
    @{n='PUT /api/restock-requests/{id}/reject';m='PUT';u="$base/api/restock-requests/$id/reject";b='{}'},
    @{n='PUT /api/Transfer/transfer/{id}/status';m='PUT';u="$base/api/Transfer/transfer/$id/status";b='{}'},
    @{n='DELETE /api/Transfer/transfer/{id}';m='DELETE';u="$base/api/Transfer/transfer/$id"},
    @{n='PUT /api/Transfer/transfer/{id}/receive';m='PUT';u="$base/api/Transfer/transfer/$id/receive";b='{}'},
    @{n='PATCH /api/Transfer/transferV2/{id}';m='PATCH';u="$base/api/Transfer/transferV2/$id"},
    @{n='GET /api/restock-requests/by-warehouse/{id}';m='GET';u="$base/api/restock-requests/by-warehouse/$id"},
    @{n='GET /api/restock-requests/by-parent-warehouse/{id}';m='GET';u="$base/api/restock-requests/by-parent-warehouse/$id"}
)

foreach($t in $tests){
    try {
        if($t.ContainsKey('b')) {
            $r = Invoke-WebRequest -Method $t.m -Uri $t.u -Headers $h -Body $t.b -UseBasicParsing -TimeoutSec 30
        } else {
            $r = Invoke-WebRequest -Method $t.m -Uri $t.u -Headers $h -UseBasicParsing -TimeoutSec 30
        }
        $snippet = ($r.Content -replace '\s+', ' ')
        if($snippet.Length -gt 120){ $snippet = $snippet.Substring(0,120) + '...' }
        Write-Output ("{0} => {1} | {2}" -f $t.n, $r.StatusCode, $snippet)
    } catch {
        $status = if($_.Exception.Response){ [int]$_.Exception.Response.StatusCode } else { -1 }
        $content = ''
        if($_.Exception.Response){
            try {
                $sr = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
                $content = $sr.ReadToEnd()
                $sr.Close()
            } catch {}
        }
        $snippet = ($content -replace '\s+', ' ')
        if($snippet.Length -gt 120){ $snippet = $snippet.Substring(0,120) + '...' }
        Write-Output ("{0} => {1} | {2}" -f $t.n, $status, $snippet)
    }
}
