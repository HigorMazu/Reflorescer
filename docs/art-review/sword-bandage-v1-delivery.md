# Bandagem da Espada de Grama — 08/10/2026

O estado com a espada guardada agora exibe uma pequena faixa de tecido no pulso direito do Kairo. Ela é um nó próprio em `RightHand/Bandage`, acompanha os quadros de idle, corrida, pulo e queda, e some imediatamente ao ativar a espada. Ao guardar, reaparece quando a lâmina termina de recolher, evitando a sobreposição dos dois visuais. A arte já desenhada no antebraço continua servindo de base; a faixa independente acrescenta as voltas claras do tecido sobre o pulso.

Prévia da pose parada: `sword-bandage-idle-v1.png`. As demais poses podem ser reproduzidas em `scenes/art/SwordToggleReview.tscn -- --stowed-poses`.

Validação: compilação .NET sem erros e `SwordHudCheck` com 26 verificações aprovadas, incluindo estado inicial, ativação, recolhimento e trocas rápidas. Higor aprovou a revisão visual da faixa em 08/10.
