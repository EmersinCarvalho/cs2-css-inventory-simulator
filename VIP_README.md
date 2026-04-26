# Sistema VIP - Inventory Simulator

## 📋 Visão Geral

Sistema VIP completo integrado ao plugin Inventory Simulator para Counter-Strike 2. Permite restringir o uso de facas e luvas customizadas apenas para jogadores VIP, além de oferecer cooldown reduzido no comando `!ws`.

## ✨ Funcionalidades

### Jogadores VIP
- ✅ Podem usar **todas as skins de armas**
- ✅ Podem usar **facas customizadas**
- ✅ Podem usar **luvas customizadas**
- ✅ Podem usar **agentes**
- ✅ Podem usar **grafites e music kits**
- ⚡ **Cooldown reduzido** no comando `!ws` (10 segundos padrão)
- � **Troca de skins instantânea** (opcional, sem precisar respawn)
- 💎 **Mensagem especial** ao usar `!ws`

### Jogadores Não-VIP
- ✅ Podem usar **todas as skins de armas normais**
- ❌ **NÃO** podem usar **facas customizadas** (usarão faca padrão do time)
- ❌ **NÃO** podem usar **luvas customizadas** (usarão luvas padrão do time)
- ✅ Podem usar **agentes**
- ✅ Podem usar **grafites e music kits**
- ⏱️ **Cooldown maior** no comando `!ws` (60 segundos padrão)
- ⏳ **Precisam dar respawn** para ver mudanças de skins
- 📢 **Incentivo** a se tornar VIP nas mensagens

## 🔧 Configuração

### 1. ConVars do Servidor

Adicione no arquivo `cfg/server.cfg`:

```cfg
// ========== SISTEMA VIP ==========

// Habilitar sistema VIP
invsim_vip_enabled true

// URL da API que retorna lista de VIPs
invsim_vip_api_url "https://seusite.com/api/vips"

// Cooldown do !ws para VIPs (10 segundos)
invsim_ws_cooldown_vip 10

// Cooldown do !ws para não-VIPs (60 segundos)
invsim_ws_cooldown_nonvip 60

// Troca instantânea exclusiva para VIP
invsim_ws_immediately_vip_only true
```

**Importante:** Para que `invsim_ws_immediately_vip_only` funcione, você precisa ter `invsim_ws_immediately true`. A combinação:
- `invsim_ws_immediately true` + `invsim_ws_immediately_vip_only true` = Apenas VIPs têm troca instantânea
- `invsim_ws_immediately true` + `invsim_ws_immediately_vip_only false` = Todos têm troca instantânea
- `invsim_ws_immediately false` = Ninguém tem troca instantânea (precisa respawn)

### 2. Criar API VIP

A API deve retornar um JSON simples:

```json
{
  "vips": [
    76561198123456789,
    76561198987654321,
    76561199012345678
  ]
}
```

**📚 Consulte o arquivo [VIP_API_GUIDE.md](VIP_API_GUIDE.md) para exemplos completos em PHP, Node.js, Python, C# e mais!**

### 3. Reiniciar o Plugin

```
css_plugins reload InventorySimulator
```

ou

```
changelevel de_dust2
```

| `invsim_ws_immediately_vip_only` | bool | `false` | Restringe troca instantânea apenas para VIPs |
## 📝 ConVars Disponíveis

| ConVar | Tipo | Padrão | Descrição |
|--------|------|--------|-----------|
| `invsim_vip_enabled` | bool | `false` | Habilita/desabilita o sistema VIP |
| `invsim_vip_api_url` | string | `""` | URL da API que retorna lista de VIPs |
| `invsim_ws_cooldown_vip` | int | `10` | Cooldown do `!ws` para VIPs (segundos) |
| `invsim_ws_cooldown_nonvip` | int | `60` | Cooldown do `!ws` para não-VIPs (segundos) |

## 🎮 Comandos

| Comando | Descrição | Quem pode usar |
|---------|-----------|----------------|
| `!ws` ou `css_ws` | Atualiza o inventário | Todos os jogadores |
| `!vips` ou `css_vips` | Atualiza lista de VIPs da API | Todos os jogadores |

## 🔄 Como Funciona

### Fluxo de Verificação

1. **Jogador conecta** no servidor
2. Plugin busca o inventário do jogador normalmente
3. **Ao aplicar skins**:
   - **Armas**: Aplicadas normalmente para todos
   - **Facas**: ✅ VIP usa customizada | ❌ Não-VIP usa faca padrão do time
   - **Luvas**: ✅ VIP usa customizada | ❌ Não-VIP usa luvas padrão do time
   - **Agentes**: Aplicados normalmente para todos

### Cache e Atualização

- Lista de VIPs é carregada ao iniciar o plugin
- **Cache de 30 segundos** para evitar sobrecarga na API
- Atualização automática ao usar comando `!vips`
- Verificação O(1) (HashSet) - super performático

### Mensagens no Chat

**VIP ao usar `!ws`:**
```
[Inventory Simulator] Inventário atualizado com sucesso! Obrigado por ser VIP! 💎
```

**Não-VIP ao usar `!ws`:**
```
[Inventory Simulator] Inventário atualizado! Quer cooldown reduzido e acesso a facas/luvas? Seja VIP!
```

## 🧪 Testando

### 1. Verificar se VIP está funcionando

1. Configure um SteamID na API
2. Entre no servidor com esse usuário
3. Pegue uma faca/luva - deve funcionar ✅
4. Entre com outro usuário não-VIP
5. Pegue uma faca/luva - deve usar a padrão do jogo ❌

### 2. Verificar Cooldown

1. Use `!ws` como VIP - cooldown de 10s ⚡
2. Use `!ws` como não-VIP - cooldown de 60s ⏱️

### 3. Verificar Troca Instantânea (VIP Only)

1. Configure `invsim_ws_immediately true` e `invsim_ws_immediately_vip_only true`
2. Use `!ws` como VIP - mudanças aplicadas instantaneamente 🚀
3. Use `!ws` como não-VIP - precisa dar respawn para ver mudanças ⏳

### 4. Verificar Atualização de VIPs

1. Use o comando `!vips` no chat
2. Verifique o console do servidor:
```
[InventorySimulator] Fetching VIP list from https://...
[InventorySimulator] VIP list refreshed successfully. 5 VIPs loaded.
```

## 📋 Exemplos de Configuração

### Exemplo 1: VIP com todos os benefícios
```cfg
invsim_vip_enabled true
invsim_vip_api_url "https://seusite.com/api/vips"
invsim_ws_cooldown_vip 5
invsim_ws_cooldown_nonvip 120
invsim_ws_immediately true
invsim_ws_immediately_vip_only true
```
**Resultado:**
- VIPs: 5s cooldown + troca instantânea + facas/luvas
- Não-VIPs: 120s cooldown + precisa respawn + sem facas/luvas

### Exemplo 2: VIP apenas para facas/luvas (sem troca instantânea)
```cfg
invsim_vip_enabled true
invsim_vip_api_url "https://seusite.com/api/vips"
invsim_ws_cooldown_vip 10
invsim_ws_cooldown_nonvip 60
invsim_ws_immediately false
```
**Resultado:**
- VIPs: 10s cooldown + facas/luvas (precisa respawn)
- Não-VIPs: 60s cooldown + sem facas/luvas (precisa respawn)

### Exemplo 3: Troca instantânea para todos
```cfg
invsim_vip_enabled true
invsim_vip_api_url "https://seusite.com/api/vips"
invsim_ws_immediately true
invsim_ws_immediately_vip_only false
```
**Resultado:**
- VIPs: Troca instantânea + facas/luvas
- Não-VIPs: Troca instantânea + sem facas/luvas

### Exemplo 4: Sistema VIP desabilitado
```cfg
invsim_vip_enabled false
invsim_ws_immediately true
```
**Resultado:**
- Todos: Troca instantânea + acesso a tudo

## 🧪 Testando

### 1. Verificar se VIP está funcionando

1. Configure um SteamID na API
2. Entre no servidor com esse usuário
3. Pegue uma faca/luva - deve funcionar ✅
4. Entre com outro usuário não-VIP
5. Pegue uma faca/luva - deve usar a padrão do jogo ❌

### 2. Verificar Cooldown

1. Use `!ws` como VIP - cooldown de 10s ⚡
2. Use `!ws` como não-VIP - cooldown de 60s ⏱️

### 3. Verificar Atualização de VIPs

1. Use o comando `!vips` no chat
2. Verifique o console do servidor:
```
[InventorySimulator] Fetching VIP list from https://...
[InventorySimulator] VIP list refreshed successfully. 5 VIPs loaded.
```

## 📊 Logs do Plugin

### Sucesso
```
[InventorySimulator] VIP list refreshed successfully. 5 VIPs loaded.
```

### Erros
```
[InventorySimulator] Failed to fetch VIP list from https://... Status code: 404
[InventorySimulator] Error fetching VIP list: Connection timeout
[InventorySimulator] Invalid VIP response format from https://...
```

## ⚙️ Desabilitar Sistema VIP

Para desabilitar temporariamente e liberar facas/luvas para todos:

```cfg
invsim_vip_enabled false
```

Todos os jogadores voltarão a ter acesso completo às skins.

## 🔒 Compatibilidade

- ✅ Compatível com inventários existentes
- ✅ Não quebra funcionalidades antigas
- ✅ Sistema VIP totalmente **opcional**
- ✅ Funciona com `invsim_fallback_team`
- ✅ Funciona com StatTrak
- ✅ Funciona com grafites e sprays
- ✅ Funciona com agentes

## 📁 Arquivos Adicionados/Modificados

### Novos Arquivos
- `Models/VipResponse.cs` - Modelo de resposta da API
- `Services/VipService.cs` - Serviço de gerenciamento de VIPs
- `VIP_API_GUIDE.md` - Guia completo de implementação da API

### Arquivos Modificados
- `Services/ConVars.cs` - Adicionadas 4 novas ConVars
- `Services/CSS.cs` - Adicionado VipService
- `Models/CCSPlayerControllerState.cs` - Adicionado método `IsVip()`
- `Models/PlayerInventory.cs` - Modificados `GetKnife()` e `GetGloves()`
- `Extensions/CCSPlayerControllerExtensions.cs` - Modificados `RegiveGloves()` e `RegiveWeapons()`
- `InventorySimulator.cs` - Inicialização do VipService
- `InventorySimulator.Commands.cs` - Comando `!ws` com cooldown diferenciado + novo comando `!vips`
- `InventorySimulator.Core.cs` - Verificação VIP no StatTrak
- `InventorySimulator.Hooks.cs` - Verificação VIP nos hooks

## 🛠️ Troubleshooting

### Problema: VIPs não são reconhecidos

**Solução:**
1. Execute `!vips` no chat para forçar atualização
2. Verifique se a API está retornando o JSON correto
3. Teste a API manualmente: `curl https://seusite.com/api/vips`
4. Verifique os logs do servidor

### Problema: API não responde

**Solução:**
1. Verifique se a URL está correta em `invsim_vip_api_url`
2. Teste a URL no navegador
3. Verifique firewall do servidor
4. Verifique logs do servidor web

### Problema: Erro "Invalid VIP response format"

**Solução:**
1. Certifique-se de que o JSON tem a chave `"vips"`
2. Verifique se os SteamIDs são números inteiros
3. Valide o JSON em https://jsonlint.com

## 💡 Dicas de Performance

1. **Cache na API**: Implemente cache de 30-60s na sua API
2. **Índices no Banco**: Crie índices nas colunas `is_vip` e `steamid`
3. **Limite de Resultados**: Limite a query para no máximo 10.000 VIPs
4. **CDN/Proxy**: Use CDN se tiver muitos servidores consultando a mesma API

## 📞 Suporte

Para problemas ou dúvidas:
1. Verifique os logs do servidor
2. Consulte o [VIP_API_GUIDE.md](VIP_API_GUIDE.md)
3. Teste a API manualmente
4. Verifique se todas as ConVars estão configuradas

## 📄 Licença

Copyright (c) Ian Lucas. Todos os direitos reservados.
Licenciado sob a Licença MIT.
