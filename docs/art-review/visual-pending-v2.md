# Revisão visual — 27/10/2026

Entrega visual aprovada pelo Higor em 27/10/2026. Mudanças locais na master, sem commit ou push.

## Revisão v3 — retorno de 27/10

- A Serpente foi baixada visualmente em 8 px e recebeu uma sombra de contato que se adapta à largura de cada pose. Colisão, alcance e dano não mudaram.
- A abertura ganhou moldura de folhas, ramos perto do título, brilhos discretos e teclas com borda em relevo. A leitura em 960×540 permanece íntegra.
- Capturas atuais: `desert-ground-contact-v3.png`, `desert-combat-review-v3.gif`, `tutorial-keyboard-v3.png` e `tutorial-controller-960-v3.png`.
- Higor aprovou os ajustes v3 em 27/10. A06-T7, A08-T3 e A08-T7 estão fechadas na parte visual.

## A06-T7 — Abertura e dicas

- Teclas/botões em caixas próprias, separados das descrições; textos longos quebram linha.
- Paleta verde, dourado e título Cormorant preservam a identidade das telas existentes.
- Faixas limitadas a 680 px, abaixo do HUD, com recálculo após mudança de dispositivo ou tamanho.
- Capturas reais: `tutorial-keyboard-v2.png`, `tutorial-controller-v2.png`, `tutorial-keyboard-960-v2.png`, `tutorial-controller-960-v2.png`.
- A captura usa o HUD da cena de produção do Deserto para inspecionar o componente. No jogo, a abertura continua sendo apresentada no Novo jogo, no Sopé.

## A08-T3 — Serpente

- Reinício explícito no primeiro quadro a cada golpe, inclusive caudas consecutivas.
- Velocidade dos sprites sincronizada com as durações configuradas; nenhum ajuste de dano, alcance, IA ou duração das janelas de acerto.
- Marcador subterrâneo achatado para ler como movimento na superfície da areia.
- Barra de vida em tons de arenito, substituindo o roxo provisório.
- Corrigido o exportador da folha especial: as poses não respeitam uma grade regular de 3×3. Limites próprios agora preservam as cabeças na emergência e a cauda completa na varrida. Os nove quadros foram reexportados em canvas 512×320, com base no chão. A barra de vida foi elevada para não atravessar a cabeça.
- `desert-combat-review-v2.gif`: bote, cauda e mergulho capturados no renderer com a Serpente de produção. Kairo está imóvel/invulnerável e a escolha automática de golpes está suspensa somente no roteiro de captura, para tornar a sequência reproduzível.

## A08-T7 — Arena

- Cortina de areia com correntes onduladas, grãos ascendentes e redemoinhos junto ao chão.
- Usa os mesmos eventos de visibilidade da arena: fecha durante o confronto e abre após a vitória. A colisão existente permanece intacta.
- `desert-gate-banner-v2.png` e o GIF mostram a integração ao cenário noturno.

## Validação e limites

- `dotnet build Joguim.csproj`: sem erros ou avisos.
- TutorialFallCheck: 19/19; GamepadCheck: 27/27; SerpentBossCheck: 24/24; DesertLinkCheck: 25/25.
- Após reexportar os nove quadros: DesertReviewCheck sem falhas e SerpentBossCheck 24/24 novamente.
- Capturas inspecionadas em 1280×720 e 960×540; entrada de controle simulada, sem teste de controle físico.
- Os testes ainda registram avisos de recursos retidos no encerramento da Godot; não são apresentados como execução sem avisos.
- Save do usuário preservado por cópia externa e restaurado após a revisão.
- Aprovação estética e playtest humano continuam pendentes; os roadmaps não contam esta entrega como aprovada.

### Pendência de mecânica para Gustavo

Na emergência da Serpente, `SerpentBoss._PhysicsProcess` chama a implementação base antes de aplicar `EruptionRect`. Como `ContactDamageEnabled` permite contato em `BurrowExit`, uma sobreposição pode aplicar o dano de contato e ativar a invulnerabilidade antes do dano especial. O teste atual verifica apenas que a vida diminui. Revisar a precedência e testar o valor exato esperado antes de fechar o QA de combate. Esta entrega visual não altera essa regra.

As quatro salas jogáveis do Deserto, recompensa pós-chefe e restauração continuam nas tarefas C15-T3/T4/T5. Não foram substituídas por conteúdo visual fictício.
