# Correções finais de Kairo e Korrag — revisão v3, 08/10/2026

Esta revisão responde ao playtest posterior à v2.

## Korrag

- `charge_windup` não usa mais o recorte que continha uma perna traseira separada do corpo.
- `stomp` também deixou de reutilizar esse quadro defeituoso.
- A folha de revisão mostra os 13 estados realmente usados pela cena do boss.

Captura: `korrag-animation-v3.png`.

## Espada de Grama

- O cabo continua seguindo a mão direita medida em cada quadro.
- Nas poses neutras, a espada fica atrás do corpo e a mão original cobre o cabo.
- Durante o ataque, o pivô entra mais fundo no punho fechado e a lâmina passa à frente do corpo, permanecendo legível nos três quadros sem criar uma segunda mão.

Capturas: `sword-grip-v9-idle.png` e `sword-grip-v9-attack.png`.

## Apoio na parede e dash

- Ambos estão ligados aos estados reais de `PlayerController`, sem alterar o desenho normal de Kairo.
- O apoio na parede acrescenta pequenas raspas junto ao contato.
- O dash usa linhas curtas e poeira atrás do personagem, mantendo as cores normais; o tom verde antigo foi removido.
- O dash continua sendo desbloqueado após derrotar Korrag. O apoio na parede está disponível desde o início.

Captura: `kairo-movement-v3.png`.

## Verificação

- Compilação: 0 erros.
- `KorragAnimationCheck.gd`: 0 falhas, incluindo exclusão do quadro defeituoso no windup e stomp.
- `SwordHudCheck.tscn`: 0 falhas, incluindo cabo, camada, punho, sete estados e três quadros do ataque.
- `MovementAnimationRuntimeCheck.tscn`: 0 falhas; entrada, colisão, estado, animação e efeitos do dash e do apoio na parede foram exercitados.

Aprovação visual no jogo permanece pendente.
