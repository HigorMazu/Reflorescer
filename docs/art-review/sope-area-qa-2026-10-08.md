# QA dirigido do Sopé da Mata — 08/10/2026

`tools/art/SopeAreaCheck.gd` carrega a cena pela rota real do `SceneManager`, sem apagar saves. Confirmou o jogador em `SpawnInicial` (a cena configurada para Novo Jogo), limites de câmera 0..2400 / -200..520, ponto de restauração e cinco inimigos. Depois carregou o Dossel Vivo e usou os campos reais do trigger de volta para confirmar o jogador em `SpawnFromDosselVivo`. Quatro verificações passaram; registro em `sope-area-validation.txt`.

O mesmo script mediu a cena renderizada no Godot 4.7.2 Mono, renderer `gl_compatibility`, GPU NVIDIA GeForce GTX 1650, janela de 1280×720. Após aquecimento de 1,5 s e descarte dos primeiros 60 quadros de cada trecho, colheu 120 amostras em x=160, 1050, 2000 e novamente 160. Todos ficaram em média e mínimo de 60 FPS (`sope-performance-validation.txt`). Uma rodada curta anterior, sem aquecimento suficiente, apresentou mínimo de 46 FPS no trecho inicial; por isso a conclusão é estabilidade após carregar, não ausência de queda na abertura.

Este é um teste dirigido com inimigos e HUD carregados, mas sem combate contínuo ou percurso jogado de ponta a ponta. A confirmação de fluidez perceptiva durante luta, travessia e transições permanece aberta no roadmap.
