# Cartografia explorável — Floresta Tropical (15/10/2026)

O pedido do Higor é um mapa inspirado na leitura espacial de Hollow Knight: áreas conectadas para percorrer à direita, à esquerda, acima e abaixo, mostrando ao protagonista apenas os trechos que ele explorou. A implementação atual usa as quatro cenas jogáveis da demo, sem antecipar o Deserto nem revelar caminhos ainda não criados.

No percurso existente, Kairo segue do Sopé ao Dossel, sobe pelo Dossel, entra no Igarapé e chega ao Covil. Os portais de retorno já levavam do Igarapé ao Dossel e do Dossel ao Sopé. O retorno Covil → Igarapé foi adicionado, fechando a ida e volta das quatro áreas. A exploração vertical se dá na subida e descida do Dossel; para criar novas bifurcações independentes em quatro sentidos serão necessárias novas salas, layouts e entradas correspondentes.

O mapa abre com **M** e pausa a partida; **M** ou **Esc** fecha. Cada cena é dividida em células de posição. Um trecho entra no mapa ao ser atravessado, e o save guarda `ExploredMapCells`. Nomes de região e passagens ficam ocultos até visitar seus trechos. Saves antigos começam com a lista vazia e são preenchidos conforme Kairo anda.

| Prévia | O que mostra |
|---|---|
| [Névoa inicial](world-map-fog-v1.png) | Apenas a célula inicial do Sopé, como num jogo novo. |
| [Mapa completo para revisão](world-map-complete-v1.png) | Todas as células reveladas artificialmente somente para avaliar cores, moldura e conexão; o jogo não entrega este mapa ao jogador. |

Os quatro cantos usam ramos e folhas espelhados e voltados para dentro. Essa versão substitui os círculos rígidos da primeira captura e preserva espaço livre para os textos inferiores.

Arte e QA visual: [[ROADMAP_ARTE]]/[[ROADMAP_QA_INTEGRACAO]] A09-T1. Revelação, save e portais: [[ROADMAP_DEV]]/[[ROADMAP_QA_CODIGO]] C16. `WorldMapCheck.gd` passou 12 verificações; `DemoRunCheck.gd` passou 89/89 e `SaveReloadCheck.gd` não falhou. Falta a avaliação visual jogada do Higor.
