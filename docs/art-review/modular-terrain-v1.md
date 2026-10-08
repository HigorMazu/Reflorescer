# Terreno modular das áreas — entrega v1, 08/10/2026

As faixas de chão das quatro áreas agora usam módulos visuais reutilizáveis, sem redimensionar uma ilustração inteira de maneira desigual:

- **Sopé da Mata e Dossel Vivo:** `TropicalGroundStrip.gd`, com pontas de 24 px e miolo alternado de grama, raízes, pedras e musgo.
- **Igarapé Sufocado e Covil de Korrag:** `ModularTerrainStrip.gd`, com pontas próprias de cada ilustração e módulos alternados do miolo. O desenho mantém a proporção vertical da arte e só repete trechos adequados para o centro da faixa.

O componente é apenas visual e fica como filho do mesmo `StaticBody2D` que já possuía a colisão. Nenhum volume de chão, tronco, lama, parede, teto ou plataforma foi deslocado.

Verificações executadas no Godot .NET 4.7.2:

- `IgarapeAreaCheck.gd`: 12 verificações aprovadas, incluindo margem sobre a colisão, troncos, lama, restauração e inimigos.
- `CovilAreaCheck.gd`: 9 verificações aprovadas, incluindo chão sobre a colisão, limites da arena e boss.
- compilação C#: 0 avisos e 0 erros.

As capturas de revisão confirmam que não há emendas expostas no enquadramento de jogo: `igarape-before-v1.png` e `covil-arena-v1.png`.
