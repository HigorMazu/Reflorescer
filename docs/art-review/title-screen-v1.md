# Tela de título animada — 09/10/2026

Implementada na entrada real `scenes/ui/MainMenu.tscn`, na branch master. Aprovação visual pendente. O pedido anterior de sheet da lama e pegada da corrida foi aprovado por Higor em 09/10 e já estava commitado ao iniciar esta entrega.

## Design

Floresta ao amanhecer, ruínas com um broto dourado, título em marfim e seleção verde/dourada. Fundo exclusivo em `assets/ui/title/forest_dawn_v1.png`. Fonte Cormorant Garamond incluída com sua licença SIL OFL em `assets/fonts/cormorant/`.

Animações: entrada em fade, leve deslocamento do cenário com o mouse, movimento lento da água, névoa e raios discretos, 34 vaga-lumes com trajetórias e brilhos desencontrados, indicador de seleção pulsante e fade de saída. O texto fica estável. As animações só existem na cena inicial.

Mantidas as ações existentes: Novo jogo, Continuar (somente com save) e Sair. A lista vertical `Box` acomoda futuras opções. As ações de save continuam no controlador C#; o script de apresentação não grava saves. Cliques repetidos são bloqueados durante a saída. Se Continuar falhar, o menu volta a ficar utilizável.

## Prévias e validação

- `title-screen-v1.png`: captura real em 1280×720.
- `title-screen-v1-960.png`: captura real em 960×540.
- `title-screen-v1.gif`: 24 capturas reais da animação ambiental, reduzidas para 960×540.
- Compilação .NET: 0 erros e 0 avisos.
- `TitleScreenReview.gd`: renderização nas duas resoluções, relógio de animação, foco de teclado e visibilidade de Continuar passaram.
- `TitleFlowCheck.gd`: Novo jogo, bloqueio de ativação duplicada, Continuar e recuperação de save ausente passaram em processos separados, em projeto e diretório de saves isolados. Sair encerrou normalmente após o fade.

### Pendência técnica encontrada

Salvar, descarregar e recarregar o Sopé no mesmo processo provocou falha nativa de gerenciamento de recursos .NET (`gchandle.is_released()` / `Handle is not initialized`). A falha também foi reproduzida carregando Sopé → cena vazia → ContinueGame, sem instanciar MainMenu ou TitlePresentation. Isso evidencia que a nova apresentação não é necessária para disparar o problema; a causa raiz ainda não foi diagnosticada. Log: `title-save-reload-diagnostic.txt`. A investigação de save/recarga fica fora desta entrega de design; não considerar essa sequência validada.

## Geração do cenário

Ferramenta: ImageGen integrado, sem CLI. Imagem criada sem texto; título e botões são controles nativos do Godot.

Prompt utilizado:

> Create a premium hand-painted pixel-art-inspired background for the title screen of a lush tropical forest metroidvania game named Reflorescer. Landscape 16:9 1920x1080 composition. No text, no lettering, no logos, no UI, no characters. Deep emerald and muted teal ancient rainforest at dawn, dark framing roots and delicate fern silhouettes at the outer edges, an atmospheric turquoise misty clearing on the RIGHT HALF with a small ancient moss-covered stone shrine and a single tiny luminous golden seedling. Huge mature trees, hanging vines, layered depth, a few warm firefly points, soft angled pale golden sunlight. LEFT HALF is calm dark deep forest with subtle silhouettes and very low contrast, clear negative space for the game title and menu overlay. Lower foreground is dark fern and root silhouettes, upper edge is canopy framing, avoid busy decoration in left center. Painterly detailed game art with delicate crisp textured foliage, refined natural muted palette, coherent lighting, elegant melancholic hopeful atmosphere, no coarse giant pixels, no photorealism, no cartoon toy look, no oversaturation. Full-bleed background only.
