# Empunhadura direita da Espada de Grama — 07/10/2026

Correção pedida por Higor: a espada precisa estar na mão direita anatômica do Kairo, a outra mão no sprite, e o punho deve parecer segurá-la.

Em sprites voltados para a direita, a mão direita do Kairo está no lado esquerdo da imagem. As posições de idle, corrida, pulo e queda foram ancoradas nessa mão fechada. O nó da espada recebeu `z_index = -1`: o corpo e o punho já desenhados no frame são renderizados por cima do cabo. Assim, o punho cobre a pegada e a lâmina aparece pelo lado externo da perna, voltada para baixo. O ataque continua com sua própria posição horizontal para a estocada.

Validação: build .NET sem erros/avisos. SwordHudCheck passou 22/22, incluindo a posição na mão direita e a ordem de desenho punho sobre cabo. AttackThrustCheck passou 7/7 novamente. Não foram alterados dano, colisões, alcance, velocidade, cooldown ou a animação de sacar/guardar.

Prévia: `sword-right-grip-v5.png` usa as seis poses reais. Reproduza com `scenes/art/SwordToggleReview.tscn -- --poses`. A faixa no pulso já desenhada no Kairo foi preservada; a bandagem como sprite independente ainda está pendente na Task A01-T2.
