# Estocada da Espada de Grama — 07/10/2026

Pedido de Higor: melhorar o movimento de ataque inspirado no ferrão de Hollow Knight. Implementado em `master`, sem commit ou push.

A sequência agora tem três leituras claras: preparação curta com lâmina recuada, estocada reta na horizontal e recuperação. O corpo desloca visualmente 2 px para trás e 4 px à frente durante o golpe, com compressão/alongamento leve. É apenas apresentação: o controlador, velocidade de caminhada, salto e dano continuam iguais.

A hitbox deixa de seguir um arco inclinado. Ela fica horizontal, 34 px à frente do Kairo, abre após 0,10 s quando a estocada está no segundo quadro, e fecha em 0,20 s para a recuperação. O cooldown total continua 0,30 s. A espada reta v2 permanece separada e recebe posição e ângulo por quadro, sem a lâmina curva antiga nos sprites de ataque.

Validação: build .NET sem erros/avisos. `AttackThrustCheck.tscn` executa a rota de input real e passou sete verificações: animação, preparação sem dano, abertura no impacto, posição e orientação da hitbox, quadro de impacto, fechamento e retorno ao estado disponível. `SwordHudCheck.tscn` continua com 20 PASS/0 FAIL. A prévia foi capturada de uma cena Godot real com piso físico, sem saves ou progresso.

Arquivos para revisão: `thrust-sequence-v3.gif` mostra o ciclo; `thrust-sequence-v3.png` mostra preparação, estocada, recuperação e neutro. Cena de reprodução: `scenes/art/AttackThrustReview.tscn`. O ataque em jogo usa os mesmos scripts e assets; a cena só automatiza o pressionamento da ação para a captura.

Referência usada: as duas imagens de Hollow Knight enviadas por Higor. Foram usadas como direção de movimento e ritmo, sem copiar personagens, arte ou efeitos visuais da referência.
