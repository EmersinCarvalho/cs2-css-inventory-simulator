# Guia de Implementação da API VIP

Este guia explica como criar uma API que retorna a lista de VIPs para o sistema VIP do Inventory Simulator.

## Requisitos

A API deve retornar um JSON simples contendo uma lista de SteamID64 dos jogadores VIP.

### Formato da Resposta

```json
{
  "vips": [
    76561198123456789,
    76561198987654321,
    76561199012345678
  ]
}
```

**Importante:**
- Os SteamIDs devem ser números inteiros (SteamID64)
- A chave `"vips"` é obrigatória
- Retornar um array vazio `[]` é válido se não houver VIPs

## Exemplos de Implementação

### PHP (Simples)

```php
<?php
header('Content-Type: application/json');

// Conectar ao banco de dados
$pdo = new PDO('mysql:host=localhost;dbname=seu_banco', 'usuario', 'senha');

// Buscar VIPs (exemplo: tabela 'users' com campo 'is_vip' e 'steamid')
$stmt = $pdo->query("SELECT steamid FROM users WHERE is_vip = 1");
$vips = $stmt->fetchAll(PDO::FETCH_COLUMN);

// Converter para inteiros
$vips = array_map('intval', $vips);

// Retornar JSON
echo json_encode(['vips' => $vips]);
?>
```

### PHP (Com Cache)

```php
<?php
header('Content-Type: application/json');

$cacheFile = '/tmp/vips_cache.json';
$cacheTime = 300; // 5 minutos

// Verificar cache
if (file_exists($cacheFile) && (time() - filemtime($cacheFile) < $cacheTime)) {
    echo file_get_contents($cacheFile);
    exit;
}

// Buscar do banco
$pdo = new PDO('mysql:host=localhost;dbname=seu_banco', 'usuario', 'senha');
$stmt = $pdo->query("SELECT steamid FROM users WHERE is_vip = 1");
$vips = array_map('intval', $stmt->fetchAll(PDO::FETCH_COLUMN));

$response = json_encode(['vips' => $vips]);

// Salvar cache
file_put_contents($cacheFile, $response);

echo $response;
?>
```

### Node.js (Express)

```javascript
const express = require('express');
const mysql = require('mysql2/promise');

const app = express();
const port = 3000;

// Pool de conexões MySQL
const pool = mysql.createPool({
  host: 'localhost',
  user: 'usuario',
  password: 'senha',
  database: 'seu_banco'
});

app.get('/api/vips', async (req, res) => {
  try {
    const [rows] = await pool.query('SELECT steamid FROM users WHERE is_vip = 1');
    const vips = rows.map(row => parseInt(row.steamid));
    res.json({ vips });
  } catch (error) {
    console.error(error);
    res.status(500).json({ vips: [] });
  }
});

app.listen(port, () => {
  console.log(`API VIP rodando na porta ${port}`);
});
```

### Python (Flask)

```python
from flask import Flask, jsonify
import mysql.connector

app = Flask(__name__)

def get_db_connection():
    return mysql.connector.connect(
        host='localhost',
        user='usuario',
        password='senha',
        database='seu_banco'
    )

@app.route('/api/vips')
def get_vips():
    try:
        conn = get_db_connection()
        cursor = conn.cursor()
        cursor.execute("SELECT steamid FROM users WHERE is_vip = 1")
        vips = [int(row[0]) for row in cursor.fetchall()]
        cursor.close()
        conn.close()
        return jsonify({'vips': vips})
    except Exception as e:
        print(f"Erro: {e}")
        return jsonify({'vips': []})

if __name__ == '__main__':
    app.run(port=3000)
```

### ASP.NET Core (C#)

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class VipsController : ControllerBase
{
    private readonly AppDbContext _context;

    public VipsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetVips()
    {
        try
        {
            var vips = await _context.Users
                .Where(u => u.IsVip)
                .Select(u => u.SteamId)
                .ToListAsync();

            return Ok(new { vips });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao buscar VIPs: {ex.Message}");
            return Ok(new { vips = new List<ulong>() });
        }
    }
}
```

## Estrutura de Banco de Dados Sugerida

### Exemplo de Tabela MySQL

```sql
CREATE TABLE users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    steamid BIGINT UNSIGNED NOT NULL UNIQUE,
    username VARCHAR(255),
    is_vip BOOLEAN DEFAULT FALSE,
    vip_expires_at DATETIME NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_is_vip (is_vip)
);
```

### Exemplo com Expiração de VIP

```sql
-- Query para buscar apenas VIPs ativos
SELECT steamid 
FROM users 
WHERE is_vip = 1 
  AND (vip_expires_at IS NULL OR vip_expires_at > NOW());
```

## Configuração no Servidor CS2

Adicione no arquivo `cfg/server.cfg`:

```
// Habilitar sistema VIP
invsim_vip_enabled true

// URL da API VIP
invsim_vip_api_url "https://seusite.com/api/vips"

// Cooldown para VIPs (segundos)
invsim_ws_cooldown_vip 10

// Cooldown para não-VIPs (segundos)
invsim_ws_cooldown_nonvip 60
```

## Testando a API

### Usando cURL

```bash
curl https://seusite.com/api/vips
```

### Resposta esperada

```json
{
  "vips": [76561198123456789, 76561198987654321]
}
```

## Segurança

### Recomendações

1. **Rate Limiting**: Limite requisições por IP para evitar abuse
2. **Cache**: Implemente cache de 30-60 segundos para reduzir carga no banco
3. **HTTPS**: Use sempre HTTPS em produção
4. **Autenticação** (Opcional): Adicione um token de API para validar requisições

### Exemplo com Autenticação (PHP)

```php
<?php
header('Content-Type: application/json');

$apiKey = 'seu_token_secreto_aqui';
$receivedKey = $_SERVER['HTTP_X_API_KEY'] ?? '';

if ($receivedKey !== $apiKey) {
    http_response_code(401);
    echo json_encode(['error' => 'Unauthorized']);
    exit;
}

// ... resto do código
?>
```

## Troubleshooting

### Erro: "Failed to fetch VIP list"

- Verifique se a URL está correta
- Teste a API manualmente com cURL
- Verifique logs do servidor web

### Erro: "Invalid VIP response format"

- Certifique-se de que o JSON está correto
- Verifique se a chave `"vips"` existe
- Verifique se os SteamIDs são números inteiros

### VIPs não são reconhecidos

- Execute `css_vips` no console do servidor para forçar atualização
- Verifique os logs do plugin: `[InventorySimulator] VIP list refreshed successfully`
- Confirme se os SteamID64 estão corretos

## Performance

### Cache no Plugin

O plugin já possui cache interno de 30 segundos. Não é necessário atualizar a lista a cada requisição.

### Cache na API

Recomenda-se implementar cache de 30-60 segundos na API para reduzir consultas ao banco de dados.

### Exemplo de Query Otimizada

```sql
-- Criar índice para melhor performance
CREATE INDEX idx_vip_lookup ON users(is_vip, vip_expires_at);

-- Query otimizada
SELECT steamid 
FROM users 
WHERE is_vip = 1 
  AND (vip_expires_at IS NULL OR vip_expires_at > NOW())
LIMIT 10000;
```

## Comandos Disponíveis

- `css_vips` - Atualiza a lista de VIPs manualmente (qualquer jogador pode usar)

## Logs do Plugin

O plugin registra informações úteis no console:

```
[InventorySimulator] Fetching VIP list from https://...
[InventorySimulator] VIP list refreshed successfully. 5 VIPs loaded.
```

Em caso de erro:

```
[InventorySimulator] Failed to fetch VIP list from https://... Status code: 404
[InventorySimulator] Error fetching VIP list: Connection timeout
```
