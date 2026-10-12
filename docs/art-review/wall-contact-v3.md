# Revisão de apoio e menu — 12/10/2026

## Apoio no tronco

A captura anterior mostrou a garra perto da parede, mas o pé avançado longe dela. A pose atual inclina o Kairo inteiro em direção ao tronco. A inclinação inclui mão, espada e corpo; a garra e o pé avançado se aproximam da borda da colisão. As marcas e os fragmentos saem da altura da garra e do pé. Não foi acrescentado outro membro ao sprite.

Prévia animada ampliada, nos dois sentidos: [direita](wall-contact-v3-right.gif) e [esquerda](wall-contact-v3-left.gif). [Detalhe estático da garra e do pé](wall-contact-v3-detail.png). Capturas completas em escala de jogo: [direita](wall-contact-v3-right.png) e [esquerda](wall-contact-v3-left.png). As ampliações servem para examinar os contatos; o tamanho real aparece nas capturas completas.

## Menu inicial

A dica mostra `W / S` e `Enter` em pequenas molduras douradas, enquanto `Navegar` e `Selecionar` aparecem em verde claro. W e S movem o foco entre as opções visíveis; a navegação anterior por setas permanece. [1280×720](title-hint-v3.png) e [960×540](title-hint-v3-small.png).

## Verificação

- Compilação sem erros ou avisos.
- Teste de movimento: 18 verificações passaram. Inclui contato à direita e à esquerda, retorno da pose normal, salto na parede e dash.
- Captura na cena real do Dossel com colisão e entrada de jogador nos dois lados.
- Teste da tela de título passou nas duas resoluções, incluindo eventos W/S que mudam o foco.
- O playtest manual de Higor ainda está pendente no roadmap. Esta revisão visual aguarda aprovação.

O teste de movimento prepara o dash sem disparar o evento de salvamento. O save de teste gerado pela versão anterior do teste permanece guardado, fora do slot ativo, como `save_0.runtime-test-20261012.json.bak` na pasta de dados do jogo. Não havia cópia confirmada do progresso anterior para restaurar.
