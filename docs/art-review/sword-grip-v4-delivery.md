# Empunhadura da Espada de Grama — 07/10/2026

Pedido de Higor: corrigir a pegada. A espada aparecia atrás do Kairo; ela deve sair da mão fechada da frente e pode ficar voltada para baixo, raspando o chão.

`GrassSwordVisual` agora usa a posição da mão fechada visível nos sprites de idle, corrida, pulo e queda. A origem do sprite continua no punho da espada; a lâmina gira 180° nas poses neutras, portanto aponta para baixo a partir da mão e não para trás do corpo. O personagem já tem punho fechado nos frames usados, por isso não foi preciso redesenhar Kairo. A estocada permanece horizontal e usa os pontos específicos dos três quadros de ataque.

Validação: build .NET sem erros/avisos. SwordHudCheck passou 21 verificações, incluindo a nova checagem de que a origem da espada fica à frente do Kairo no idle. AttackThrustCheck passou 7/7 novamente depois da mudança: preparação sem dano, impacto horizontal sincronizado e recuperação. O único aviso de execução vem do SFX de ataque ausente.

Prévia: `sword-grip-v4.png` mostra as seis poses neutras reais. `thrust-sequence-v3.gif` mantém a prévia da estocada. Reprodução: `scenes/art/SwordToggleReview.tscn -- --poses` e `scenes/art/AttackThrustReview.tscn`.

Esta mudança não altera colisões, dano, movimento físico, cooldown nem a animação de sacar/guardar. A bandagem como sprite separado continua pendente na Task A01-T2.
