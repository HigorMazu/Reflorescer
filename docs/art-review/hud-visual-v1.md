# HUD — revisão visual v1, 08/10/2026

Higor aprovou esta revisão em 08/10. A barra de vida deixa o padrão do Godot e passa a ter moldura verde discreta, fundo escuro e divisões internas que facilitam perceber a perda de vida. O prompt contínuo de interação ganhou painel compacto escuro com borda verde, mantendo `[E] Restaurar` legível sobre qualquer área.

O indicador persistente de dash foi removido a pedido de Higor. A notificação temporária existente quando a habilidade é desbloqueada continua sendo o único retorno visual do dash.

Captura: `hud-visual-v1.png`. A cena de prévia é `scenes/art/HudVisualReview.tscn`. `HealthBar.cs` desenha o visual sobre o mesmo `TextureProgressBar`, portanto o valor, o tween e o comportamento do HUD não foram alterados.
