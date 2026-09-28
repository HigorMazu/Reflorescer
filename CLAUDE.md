# Reflorescer — orientação pro Claude Code

Projeto: Metroidvania 2D em Godot 4.7 + C# (assembly `Joguim`), tema ODS 15 (Vida Terrestre). Documentação de design completa (decisões, bestiário, sistemas) mora no Notion — não neste repo.

## Fluxo de entrega deste projeto

Antes de qualquer mudança de código, ler nesta ordem:

1. **`docs/ROADMAP_QA_CODIGO.md`** — o que "funcionando" significa pra cada sistema de código (movimento, combate, inimigos, boss, save, etc). É a fonte de verdade pra saber se uma mudança quebrou algo.
2. **`docs/ROADMAP_DEV.md`** — o roadmap de implementação, organizado em fases, cada uma ligada aos casos de teste do arquivo acima. **Trabalhar seguindo a ordem das fases** — a Fase 0 (Input Map + unificar sistema de movimento) é pré-requisito de tudo.
3. **`docs/ROADMAP_QA_INTEGRACAO.md`** — testes que só fazem sentido depois que a arte do Higor entra (nomes de animação, pivô, contrato visual). Consultar ao integrar qualquer asset novo.
4. **`docs/ROADMAP_ARTE.md`** — o que o Higor está produzindo e em que ordem, pra saber o que já deve estar disponível pra integrar.

## Estrutura: Épico → Task → Subtask

Todos os 4 arquivos usam a mesma hierarquia, sem camada de História. `ROADMAP_DEV.md` e `ROADMAP_QA_CODIGO.md` compartilham os mesmos IDs de Épico/Task (ex: `EPIC-C03`); o mesmo vale entre `ROADMAP_ARTE.md` e `ROADMAP_QA_INTEGRACAO.md` (ex: `EPIC-A02`). Fluxo por Task: implementar as subtasks → rodar as subtasks de verificação da Task equivalente no roadmap de QA correspondente → marcar concluída → seguir pra próxima.

### Convenção de formatação (importante ao editar os `.md`)

Os 4 roadmaps usam `<details><summary>` aninhado (renderiza como seção recolhível nativa no GitHub e na maioria dos editores) em vez de headings `##`/`###` soltos. Ao editar qualquer roadmap, seguir exatamente este padrão:

- **Épico** = `<details><summary><strong>EPIC-Cxx — Título</strong> <ícone de status> <status> · <ícone de prioridade> <prioridade></summary>`. O status inline (`✅ Passa`/`🔴 Falha`/`🟡 Parcial` nos QA; `⬜ Não iniciado`/`✅ Concluído` no Dev/Arte) só aparece se relevante — um Épico já concluído fica com **✅ Concluído.** em negrito, terminado em ponto.
- Logo abaixo do `<summary>`, uma linha `*Pronto quando*: <definição objetiva de "pronto" em uma frase>.` — é a Definition of Done do Épico. Prosa adicional (contexto, por que o Épico existe, dependências) pode vir depois, solta.
- **Task** = `<details>` aninhado *dentro* do `<details>` do Épico: `<summary>Task Cxx-Ty — Título</summary>`. Não existe um terceiro nível de `<details>` — se precisar de mais detalhe técnico dentro de uma Task, usar bloco de código, não outro toggle.
- Corpo da Task: checklist `- [ ]`/`- [x]` das subtasks, depois bullets com rótulo em negrito pra specs/decisões/achados (`- **Achado da auditoria (DD/MM):** ...`, `- **Decisão do Gustavo:** ...`), terminando sempre com uma linha `Status: <resultado real, não "não iniciado" nos arquivos de QA — lá reflete o estado real da auditoria>.`
- **Notas de desenvolvimento** (bugfix, decisão, mudança de escopo) que surgirem depois de uma Task já existir viram um bullet novo dentro dela, no formato **Tipo (DD/MM[, ref]) — resumo curto**, com bullets de explicação/decisão logo abaixo (padrões: `**Decisão do Gustavo:**`, `**Perguntado ao Gustavo... confirmado:**`) e sua própria linha `Status:` atualizada ao final. Tipos usados: Bugfix, Bugfix cosmético, Bugfix CRÍTICO, Melhoria, UX, Revertida, Pendência resolvida.
- Task revertida ganha sufixo `-R` no ID e `(reversão)` no título.
- Sem tabela-resumo de Épicos dentro dos `.md` — isso fica só no Notion (Seção 15), se fizer sentido lá.

## Regras práticas

- Depois de qualquer mudança em `scripts/player/`, `scripts/enemies/` ou `scripts/bosses/`, rodar as subtasks de QA da Task equivalente em `ROADMAP_QA_CODIGO.md` antes de considerar a Task concluída.
- Ao concluir uma Task, marcar o checkbox correspondente no `ROADMAP_DEV.md` (ou `ROADMAP_ARTE.md`).
- Toda decisão de design nova (nome, mecânica, escopo) é documentada primeiro no Notion (fonte de verdade do GDD) — este repo só documenta o *como*, não o *o quê*/*por quê*.
- Escopo atual é o da **demo**: Floresta Tropical → Deserto → Tundra, boss Korrag, 3 arquétipos de inimigo comum. Não implementar conteúdo de outros biomas sem confirmar com o Gustavo primeiro — não é retrabalho perdido, só não é prioridade agora.
