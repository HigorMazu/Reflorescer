# Bordas do terreno do Sopé da Mata — 08/10/2026

As quatro faixas de chão, as três plataformas e os dois visuais do ponto de restauração passaram a preservar 24 px das extremidades da textura. O miolo continua repetido horizontalmente. Isso mantém um começo e um fim completos em cada faixa sem tocar nos `StaticBody2D` e nas colisões existentes.

As capturas `sope-terrain-borders-0.png` e `sope-terrain-borders-900.png` mostram o início e o trecho intermediário da cena de produção. A visualização é reproduzível com `scenes/art/SopeTerrainReview.tscn`. A repetição do motivo central ainda é perceptível em trechos longos: esta é uma melhoria das bordas, não o tileset modular final do A05-T1.

Higor aprovou esta revisão das bordas em 08/10. A variação seguinte do miolo está documentada em `sope-ground-modular-v1.md`.
