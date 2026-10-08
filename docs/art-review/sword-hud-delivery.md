# Indicador de espada — 07/10/2026

Implementado em `master`, sem commit/pull/push. Indicador nativo do Godot, 156×32 unidades de UI, abaixo da vida. Espada de folha + ATIVA em verde; bandagem + GUARDADA em tom neutro; tecla Q permanente. A borda realça por 0,28 segundo na troca. As margens do HUD são 16 horizontais e 12 verticais. Com o stretch do projeto, as dimensões acompanham a escala da janela.

`SwordStatusIndicator.cs` desenha os símbolos e o painel. `HUDController.cs` sincroniza o estado inicial com o jogador ou GameManager e recebe SwordToggled, sem reutilizar o texto de habilidades. Reencontra o jogador quando a referência anterior é liberada. HideAll/ShowAll incluem o indicador.

Validação: build .NET sem erros/avisos e 11 verificações em `tools/art/SwordHudCheck.tscn`, com jogador e HUD de produção. Não grava saves. Cobre criação tardia do jogador, bloqueio/liberação do ataque, trocas rápidas, conservação do texto de habilidade, visibilidade, dimensões, recriação do HUD e pausa. O teste chama o mesmo ToggleSword usado por Q; não simula teclado físico nem carrega save do usuário.

Prévia: `scenes/art/SwordHudReview.tscn` instancia o Sopé com processamento do gameplay desativado, captura os dois estados e encerra. `sword-hud-active.png` e `sword-hud-stowed.png` são capturas do Godot; `sword-hud-comparison.png` apenas recorta e amplia esses pixels para revisão. Não é captura de um playtest completo.

Atualização (07/10): indicador aprovado por Higor. Espada reta separada em todas as poses e extensão/recolhimento integrados depois desta captura inicial; tom verde removido. Teste ampliado para 20 verificações. Detalhes e pendências de A01-T2 em `straight-sword-v2-delivery.md`. Teste manual de teclado/partida e SFX sword_on/off continuam pendentes. Depois segue A05-T1, terreno modular do Sopé.

Reprodução: compilar `Joguim.csproj`; executar Godot .NET com `--headless --path . tools/art/SwordHudCheck.tscn`. Para as capturas, executar `scenes/art/SwordHudReview.tscn` com renderer `gl_compatibility`.
