# Créditos de áudio

## Música da demo

As cinco faixas abaixo são composições instrumentais autorais do projeto,
geradas de maneira reproduzível por `tools/audio/generate_demo_music.py`.
Elas não usam samples, loops ou gravações de terceiros.

| Arquivo | Uso |
| --- | --- |
| `music/title_autoral.wav` | Tela inicial |
| `music/sope_autoral.wav` | Sopé da Mata |
| `music/dossel_autoral.wav` | Dossel Vivo |
| `music/igarape_autoral.wav` | Igarapé Sufocado |
| `music/covil_autoral.wav` | Covil de Korrag |

## Efeitos sonoros

Os doze efeitos da demo em `sfx/` são sons autorais sintetizados por
`tools/audio/generate_placeholder_sfx.py`. Eles não usam material de terceiros
e permanecem separados por evento para que possam receber gravações finais sem
alterar o código.

## Prévia pós-demo do Deserto

`ambience/desert_wind.wav` (vento em loop) e `sfx/snake_hiss.wav` (sibilo da
cobra) são sons autorais sintetizados por `tools/audio/generate_desert_audio.py`,
sem samples ou gravações de terceiros. A cena isolada `DesertReview.tscn` usa
esses sons; o restante da demo não foi alterado.

`music/deserto_autoral.wav` é a faixa instrumental autoral da área jogável do
Deserto, gerada por `tools/audio/generate_desert_music.py`. Inclui uma camada
discreta de vento sintetizado que permanece na arena da cobra. Não contém
samples, loops ou gravações de terceiros.
