# Caminhos que crescem após a restauração — proposta visual v1

Higor aprovou em 30/10/2026 a direção dos três atalhos. As imagens abaixo são
capturas nas cenas reais com a arte sobreposta **somente para revisão**. Os
sprites não foram adicionados ao percurso jogável: pareceriam plataformas
usáveis antes de existirem as colisões e a persistência da C14-T5.

Higor aprovou também os três visuais v1 em 30/10/2026.

| Área | O que cresce | Arte | Captura | Posicionamento de revisão |
| --- | --- | --- | --- | --- |
| Sopé | Ponte inclinada de raízes sobre o desnível após a restauração | `assets/environment/tropical/restoration_root_bridge_v1.png` | `restoration-path-sope-preview-v2.png` | origem (1260, 415), 180×60 px, rotação 0,18 rad; topo vai do chão em y≈440 à margem em y≈480 |
| Dossel | Vinha contínua com três apoios entre o galho central e os superiores | `assets/environment/tropical/restoration_vine_steps_v1.png` | `restoration-path-dossel-preview-v1.png` | origem (650, 0), 170×255 px; apoios próximos de y≈185, 122 e 35 |
| Igarapé | Folhas e raízes formando passarela acima da segunda poça | `assets/environment/tropical/restoration_lily_bridge_v1.png` | `restoration-path-igarape-preview-v2.png` | origem (1420, 352), 540×180 px; folhas ficam acima da lama, raízes entram nas margens |

O estado fechado usa a ausência do caminho; o estado aberto revela o sprite
com o crescimento de baixo para cima já implementado em `RestorationPoint`.
Para integrar, cada sprite deve entrar em `RestoredVisuals` do respectivo ponto
e receber colisão habilitada somente após a restauração, inclusive ao voltar
à área ou carregar um save. No Igarapé, a colisão da passarela deve manter o
Kairo e os inimigos acima da lama, sem esconder seus sprites. Os três trajetos
precisam passar no teste de alcance e na revisão das alturas antes de marcar
o QA A05-T6.

Capturas reproduzíveis: `tools/art/RestorationPathsReview.gd`.
