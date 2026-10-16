# Deserto pós-demo — direção visual e ataques aprovados (15/10/2026)

O Higor aprovou a ordem e o arco visual: fazer o Deserto antes da Tundra; mostrar um sol na entrada, avançar para tarde e terminar à noite; o chefe final deve ser uma cobra grande do deserto. Também aprovou três ataques distintos para a cobra: bote/mordida frontal, varrida de cauda e mergulho/emergência na areia.

A cena isolada [DesertReview.tscn](../../scenes/art/DesertReview.tscn) permite caminhar por essa primeira direção sem mudar o menu, as quatro áreas da demo ou o save. O fundo mantém os mesmos cânions, dunas e ruínas nas três horas do dia; `DesertSkyCycle.gd` faz a transição por posição de Kairo e `DesertLighting.gd` adapta o chão e os personagens. Os três inimigos do Deserto já aprovados aparecem no percurso. O chão tem colisão alinhada à faixa de areia. Um vento autoral toca em loop.

| Trecho | Captura real do Godot | Leitura esperada |
|---|---|---|
| Entrada | [Dia](desert-ingame-dia-v1.png) | Sol claro e céu aberto |
| Meio | [Tarde](desert-ingame-tarde-v1.png) | Sol baixo, laranja e sombra quente |
| Final | [Noite](desert-ingame-noite-v1.png) | Lua, estrelas, luz azul e cobra grande |

A cobra tem 12 quadros-base — quatro de repouso, quatro de deslocamento e quatro de bote — e os estados derivados `windup`, `hurt` e `dead`, com canvas estável de 512×320. [Prévia animada da cobra](desert-snake-boss-v1.gif). `Boss_CobraDesertoVisual.tscn` mostra a arte em escala grande; ainda **não é uma luta funcional**. O sibilo é acionado no começo do bote da prévia. Para os outros dois ataques, faltam quadros próprios de cauda e de entrada/saída da areia; o trabalho está na A08-T3 do roadmap de arte. IA, hitboxes, dano, fases e progressão estão na C15-T2/T3 do roadmap do Gustavo.

As ilustrações foram geradas pelo ImageGen integrado: o fundo tropical e o Korrag serviram somente como referência de estilo; o fundo diurno foi a base visual para as variantes de tarde/noite, com pedido de preservar composição e marcos. A faixa de terreno usou o solo tropical apenas como referência de corte lateral. As fontes finais ficam em `assets/environment/desert/` e `assets/sprites/bosses/sand_viper/`; `tools/art/build_desert_art.py` recorta, escala e ancora os quadros. O vento e o sibilo são síntese autoral reproduzível em `tools/audio/generate_desert_audio.py`.

Verificação: importação do Godot sem erros e `DesertReviewCheck.gd` sem falhas, cobrindo sol/tarde/noite, iluminação, alinhamento do solo, presença dos três inimigos, recursos de áudio e canvas/animações da cobra. Log em [desert-review-validation.txt](desert-review-validation.txt). A direção visual e o trio de ataques foram aprovados em 15/10. Faltam o mapa jogável completo, as animações específicas de cauda/areia, a luta funcional e a decisão de recompensa e destino depois do chefe.
