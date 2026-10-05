# Reflorescer — orientação pro Claude Code

Projeto: Metroidvania 2D em Godot 4.7 + C# (assembly `Joguim`), tema ODS 15 (Vida Terrestre). Documentação de design completa (decisões, bestiário, sistemas) mora no **cofre do Obsidian**: a pasta-pai `Projetos/Reflorescer/` (índice `🗂️ Reflorescer.md`). O Notion foi abandonado em 18/09/2026. Este repositório fica dentro do cofre, então os `.md` daqui também são abertos no Obsidian.

## Fluxo de entrega deste projeto

Antes de qualquer mudança de código, ler nesta ordem:

1. **`docs/ROADMAP_QA_CODIGO.md`** — o que "funcionando" significa pra cada sistema de código (movimento, combate, inimigos, boss, save, etc). É a fonte de verdade pra saber se uma mudança quebrou algo.
2. **`docs/ROADMAP_DEV.md`** — o roadmap de implementação, organizado em fases, cada uma ligada aos casos de teste do arquivo acima. **Trabalhar seguindo a ordem das fases** — a Fase 0 (Input Map + unificar sistema de movimento) é pré-requisito de tudo.
3. **`docs/ROADMAP_QA_INTEGRACAO.md`** — testes que só fazem sentido depois que a arte do Higor entra (nomes de animação, pivô, contrato visual). Consultar ao integrar qualquer asset novo.
4. **`docs/ROADMAP_ARTE.md`** — o que o Higor está produzindo e em que ordem, pra saber o que já deve estar disponível pra integrar.

## Estrutura: Épico → Task → Subtask

Todos os 4 arquivos usam a mesma hierarquia, sem camada de História. `ROADMAP_DEV.md` e `ROADMAP_QA_CODIGO.md` compartilham os mesmos IDs de Épico/Task (ex: `EPIC-C03`); o mesmo vale entre `ROADMAP_ARTE.md` e `ROADMAP_QA_INTEGRACAO.md` (ex: `EPIC-A02`). Fluxo por Task: implementar as subtasks → rodar as subtasks de verificação da Task equivalente no roadmap de QA correspondente → marcar concluída → seguir pra próxima.

### Convenção de formatação (importante ao editar os `.md`)

Os 4 roadmaps são notas do Obsidian (Obsidian Flavored Markdown, ver a skill `obsidian-markdown`). Épicos e Tasks são **callouts recolhíveis aninhados**, não `<details>` (o Obsidian não renderiza Markdown dentro de HTML). No GitHub eles aparecem como blockquote simples, e isso é aceito. Ao editar qualquer roadmap, seguir exatamente este padrão:

- **Épico** = `> [!tipo]- EPIC-Cxx — Título — <ícone de status> <status> · <ícone de prioridade> <prioridade>`. Status no Dev/Arte: `⬜ Não iniciado`, `🔄 Em andamento`, `✅ Concluído`. Nos QA: `✅ Passa`, `🟡 Parcial`, `🔴 Falha`, `🔴 Bloqueado`.
- Logo abaixo, `> *Pronto quando*: <definição objetiva de "pronto" em uma frase>.`: é a Definition of Done do Épico. Prosa adicional (contexto, dependências) vem depois, dentro do mesmo callout.
- **Task** = callout aninhado dentro do Épico: `> > [!tipo]- Task Cxx-Ty — Título`, separado do bloco anterior por uma linha `>`. Todo o corpo da Task leva o prefixo `> > ` (linha em branco = `> >`). Não existe terceiro nível: detalhe técnico extra vai em bloco de código.
- **Tipo do callout = status** (a cor é o que o Obsidian mostra recolhido):

  | Tipo | Dev/Arte | QA |
  |---|---|---|
  | `todo` | não iniciado, 🟢 liberado | — |
  | `info` | implementado / 🔄 em andamento | — |
  | `warning` | 🟡 aguarda, parcial | 🟡 parcial |
  | `failure` | ⛔ bloqueado | 🔴 falha / bloqueado |
  | `success` | ✅ concluído (QA passou) | ✅ passa |

  Ao mudar a linha `Status:` de uma Task, ou o status de um Épico, trocar o tipo do callout junto.
- Corpo da Task: checklist `- [ ]`/`- [x]` das subtasks, depois bullets com rótulo em negrito pra specs/decisões/achados (`- **Achado da auditoria (DD/MM):** ...`, `- **Decisão do Gustavo:** ...`), terminando sempre com uma linha `Status: <resultado real, não "não iniciado" nos arquivos de QA — lá reflete o estado real da auditoria>.`
- **Notas de desenvolvimento** (bugfix, decisão, mudança de escopo) que surgirem depois de uma Task já existir viram um bullet novo dentro dela, no formato **Tipo (DD/MM[, ref]) — resumo curto**, com bullets de explicação/decisão logo abaixo (padrões: `**Decisão do Gustavo:**`, `**Perguntado ao Gustavo... confirmado:**`) e sua própria linha `Status:` atualizada ao final. Tipos usados: Bugfix, Bugfix cosmético, Bugfix CRÍTICO, Melhoria, UX, Revertida, Pendência resolvida.
- Task revertida ganha sufixo `-R` no ID e `(reversão)` no título.
- **Links:** referência a outro roadmap = wikilink (`[[ROADMAP_QA_CODIGO]]`). Link pra nota do GDD usa o caminho completo quando o nome se repete em outros projetos do cofre: `[[Projetos/Reflorescer/🗳️ Decisões e Configuração|🗳️ Decisões e Configuração]]` e `[[Projetos/Reflorescer/🗓️ Log de Atualizações|🗓️ Log de Atualizações]]`.
- **Frontmatter:** cada roadmap tem frontmatter lido pelo painel `📊 Painel de Desenvolvimento.base` (na pasta-pai). Sempre que mudar a linha `Status:` de alguma Task, recontar e atualizar `tasks_prontas` (Dev/Arte: status `implementado`/✅; QA: status ✅), `tasks_bloqueadas` (⛔ no Dev/Arte, `bloqueado` no QA), `tasks_liberadas` (🟢, só Dev/Arte), `epicos_concluidos` e `atualizado` (AAAA-MM-DD).
- Sem tabela-resumo de Épicos dentro dos roadmaps: isso fica em `🗺️ Roadmap.md` no cofre.

## Regras práticas

- Depois de qualquer mudança em `scripts/player/`, `scripts/enemies/` ou `scripts/bosses/`, rodar as subtasks de QA da Task equivalente em `ROADMAP_QA_CODIGO.md` antes de considerar a Task concluída.
- Ao concluir uma Task, marcar o checkbox correspondente no `ROADMAP_DEV.md` (ou `ROADMAP_ARTE.md`).
- **EventBus:** todo nó de cena que fizer `EventBus.Instance.X += Handler` precisa do `-=` correspondente no `_ExitTree()`. O `EventBus` é autoload e sobrevive à troca de cena, e o handler de um nó destruído lança `ObjectDisposedException`, interrompendo os demais inscritos (bug real de 28/09: a notificação do dash sumia). Só autoloads ficam dispensados.
- Toda decisão de design nova (nome, mecânica, escopo) é registrada no GDD do Obsidian: o quê/porquê em `🗳️ Decisões e Configuração.md` e uma linha datada em `🗓️ Log de Atualizações.md` (ambos na pasta-pai). Este repo só documenta o *como*, não o *o quê*/*por quê*.
- Escopo atual é o da **demo**: 4 áreas dentro da Floresta Tropical — Sopé da Mata → Dossel Vivo → Igarapé Sufocado → Covil de Korrag (boss, libera o dash) — ver `EPIC-C13` em `ROADMAP_DEV.md` (decidido em 29/09/2026). Deserto e Tundra saem do escopo da demo, ficam pro jogo completo. Não implementar conteúdo de outros biomas sem confirmar com o Gustavo primeiro — não é retrabalho perdido, só não é prioridade agora.
