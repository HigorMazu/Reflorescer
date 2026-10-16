# Reskins pós-demo — Deserto e Tundra (15/10/2026)

Seis espécies têm arte e cenas próprias, mantendo a lógica, os stats e as colisões dos três arquétipos existentes. As cenas herdam `Enemy_Voador`, `Enemy_Rapido` ou `Enemy_Robusto` e substituem apenas `SpriteFrames` e `EnemyId`. Não há fase jogável do Deserto ou da Tundra nesta entrega.

| Bioma | Espécie | Arquétipo | Cena |
|---|---|---|---|
| Deserto | Abelha-Africanizada | Voador | `scenes/enemies/Enemy_AbelhaAfricanizada.tscn` |
| Deserto | Rato-Preto | Rápido | `scenes/enemies/Enemy_RatoPreto.tscn` |
| Deserto | Burro Selvagem | Robusto | `scenes/enemies/Enemy_BurroSelvagem.tscn` |
| Tundra | Mosquito-Ártico | Voador | `scenes/enemies/Enemy_MosquitoArtico.tscn` |
| Tundra | Lebre-Europeia | Rápido | `scenes/enemies/Enemy_LebreEuropeia.tscn` |
| Tundra | Ganso-das-Neves | Robusto | `scenes/enemies/Enemy_GansoDasNeves.tscn` |

Cada personagem usa uma folha-fonte gerada com transparência real e orientação para a direita. A geração teve como referência de **estilo e grade**, não como alvo de substituição, a folha do respectivo arquétipo da Floresta. Os prompts pediram exatamente doze poses em grade 4×3: quatro de repouso, quatro de deslocamento e quatro de ataque, com anatomia e paleta específicas de cada espécie. `tools/art/build_post_demo_enemies.py` recorta, remove fragmentos soltos de células vizinhas, aplica escala única e ancora todos os quadros no canvas 128×96. `detect` e `hurt` reaproveitam poses do personagem; `dead` é uma pose derivada e deitada usada durante o fade. Os seis nomes de animação esperados continuam presentes.

Prévias: [Deserto](post-demo-deserto-sprites.gif) · [Tundra](post-demo-tundra-sprites.gif). As imagens estáticas equivalentes têm sufixo `.png`. Os PNGs usados pelo jogo estão em `assets/sprites/enemies/post_demo/` e as folhas-fonte em `assets/sprites/enemies/sources/post_demo/`.

Validação: `dotnet build --no-restore` passou sem avisos. O Godot importou os recursos. `EnemyIntegrationCheck.tscn` repetiu os mesmos cenários para os nove inimigos e terminou com **478 verificações, zero falhas**, incluindo canvas, pivô, flip, movimento, detecção, ataque para ambos os lados, dano, interrupção e morte. O log está em [post-demo-enemy-validation.txt](post-demo-enemy-validation.txt).

Higor aprovou os seis designs e animações no chat em 15/10/2026. A02-T4/T5 foram concluídas no roadmap de arte. Continua pendente apenas o playtest em fases reais do Deserto e da Tundra quando forem criadas; por isso a tarefa gêmea segue parcial no QA de integração.
