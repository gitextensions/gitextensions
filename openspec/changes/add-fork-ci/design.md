# Design — Change 0.1: CI propia del fork (local-first)

## Context

El fork parte del tag `v7.2.0` con la rama de trabajo `avalonia/main`. La verificación hoy es
manual (`dotnet build` / `dotnet test` a mano). Los workflows heredados en `.github/workflows/`
pertenecen a la infraestructura del upstream y no cubren `avalonia/main`.

Estado verificado del repo (2026-07-11):

- `dotnet build GitExtensions.slnx` compila limpio en ~55 s (0 errores, 0 avisos) con SDK
  .NET 10.0.301.
- Los componentes nativos C++ (`src/native`) **no** son necesarios para build ni tests.
- El upstream tiene los tests **desactivados** en su CI (`run-x64-tests: false` en
  `ge-build.yml`, citando un test inestable: `GetOriginalLineInPreviousCommit`).
- Proyectos de test: unit tests en `tests/app/UnitTests/*` y `tests/plugins/UnitTests/*`;
  integración en `tests/app/IntegrationTests/*` (los de UI instancian Forms WinForms reales).

Este documento incluye una introducción didáctica (§ "GitHub Actions desde cero") y
el YAML resultante irá comentado línea a línea.

## Goals / Non-Goals

**Goals:**

- Un único punto de verdad para "el repo está verificado": `eng/Verify.ps1` (build + unit
  tests), ejecutable en local en cualquier momento.
- CI en GitHub Actions que ejecute **ese mismo script** en cada push/PR a `avalonia/main`,
  sobre Windows, partiendo de máquina limpia.
- Dejar `.github/workflows/` sin ruido: solo lo que aplica al fork.
- Que el usuario entienda cada pieza (didáctico).

**Non-Goals:**

- Pata Linux/macOS (llega en el change 0.4, cuando el core sea multiplataforma).
- Tests de integración/UI en CI (frágiles en runners; se reevaluará).
- Publicación de artefactos, firmado, instalador, releases (Fase 4).
- Optimización agresiva de tiempos (caché NuGet, etc.) — solo si resulta trivial.

## GitHub Actions desde cero (contexto didáctico)

Conceptos que aparecen en este change, de mayor a menor:

| Concepto | Qué es | En nuestro caso |
|---|---|---|
| **Workflow** | Un fichero YAML en `.github/workflows/` que describe un proceso automatizado. GitHub lo ejecuta cuando ocurre su *trigger* | `fork-ci.yml` |
| **Trigger (`on:`)** | El evento que arranca el workflow: push, PR, botón manual, cron… | `push` y `pull_request` sobre `avalonia/main`, más `workflow_dispatch` (botón "Run workflow" en la web, útil para probar) |
| **Job** | Grupo de pasos que corre en una máquina virtual. Un workflow puede tener varios jobs en paralelo | Un único job `verify` |
| **Runner** | La máquina virtual (limpia, efímera) donde corre un job. GitHub las provee gratis en repos públicos | `windows-latest` (Windows Server con PowerShell, git y .NET preinstalados) |
| **Step** | Cada paso de un job: o ejecuta un comando (`run:`) o usa una **action** (`uses:`) | checkout → submódulos → SDK → `eng/Verify.ps1` |
| **Action** | Paso empaquetado y reutilizable publicado en el Marketplace | `actions/checkout` (clona el repo), `actions/setup-dotnet` (instala el SDK que pida `global.json`) |
| **Artifact** | Ficheros que un job sube para poder descargarlos/inspeccionarlos después | Logs de tests (`.trx`) cuando algo falla |

Dónde se ve todo: pestaña **Actions** del repo en GitHub → lista de runs → cada run muestra
sus jobs y el log de cada step en vivo. Un run fallido marca el commit con ❌ y GitHub envía
email de aviso al autor del push.

Cómo se factura: en repos **públicos**, los runners estándar (Linux, Windows, macOS) son
**gratuitos y sin límite de minutos**. No hay nada que configurar ni riesgo de coste.

## Decisions

### D1 — Local-first: el YAML no sabe compilar

`eng/Verify.ps1` contiene toda la lógica (qué se compila, qué tests corren, con qué
configuración). El workflow solo prepara la máquina (checkout, submódulos, SDK) e invoca el
script. **Por qué**: una sola definición de "verificado"; lo que pasa en local pasa en CI; se
puede depurar el 100% de la lógica sin subir commits "probando CI"; y si un día se abandona
GitHub Actions, no se pierde nada.
*Alternativa descartada*: lógica en el YAML (steps `dotnet build`/`dotnet test` sueltos) — es
lo habitual en ejemplos, pero duplica la definición y solo es depurable empujando commits.

### D2 — `Verify.ps1`: build de la solución completa + unit tests, configuración parametrizable

- `dotnet build GitExtensions.slnx` en `Release` por defecto (parámetro `-Configuration`;
  en local se puede pasar `Debug` para reutilizar la caché de desarrollo).
- Tests: se descubren los `*.csproj` bajo `tests/app/UnitTests` y `tests/plugins/UnitTests` y
  se ejecutan con `dotnet test --no-build` (la build ya se hizo; evita recompilar por
  proyecto). Los `IntegrationTests` quedan explícitamente fuera (ver Non-Goals).
- Salida: resumen final claro (OK/FAIL por proyecto) y exit code ≠ 0 si algo falla — el exit
  code es lo que hace que el step de CI se marque en rojo.
- Logs `.trx` por proyecto en `artifacts/` para poder subirlos como artifact en caso de fallo.

*Alternativa descartada*: `dotnet test GitExtensions.slnx` a secas — arrastraría también los
tests de integración/UI, que instancian Forms reales y son la causa por la que el propio
upstream apagó los tests en CI.

### D3 — Trigger acotado a `avalonia/main` + disparo manual

`push` y `pull_request` sobre `avalonia/main` únicamente, más `workflow_dispatch`. `master`
queda fuera a propósito: es un espejo del upstream, no se desarrolla ahí. Se añade
`concurrency` con cancelación: si haces dos pushes seguidos, el run del primero se cancela
(no tiene sentido gastar 15 min verificando un commit ya superado).

### D4 — Workflows heredados: borrar los de infraestructura upstream, conservar `ge-build.yml`

Se eliminan: `cla-check.yml` (CLA del proyecto upstream), `labeler-*.yml` ×4 y
`label-lifecycle.yml` (etiquetado automático con modelos entrenados suyos),
`pr-automation.yml`, `pr-check-stale.yml`, `git.yml`. Ninguno aporta al fork y varios
fallarían por secretos/permisos inexistentes si algún día disparasen.
`ge-build.yml` **se conserva sin cambios**: no dispara en `avalonia/main` (es inerte) y sirve
de referencia de cómo empaqueta el upstream (WiX, versionado, arm64) de cara a la Fase 4.
*Alternativa descartada*: desactivarlos desde la UI de GitHub — deja el repo engañoso (el
fichero existe pero no hace nada) y la decisión no queda versionada.

### D5 — Versionado de actions por tag mayor (`@v5`), no por SHA

El upstream ancla las actions por SHA de commit (más seguro frente a supply-chain: el tag `v5`
podría moverse a código malicioso, un SHA no). Para este change se usan tags mayores por
legibilidad didáctica, con comentario en el YAML explicando el trade-off. Cuando el flujo sea
estable se puede endurecer a SHA (hay bots como Dependabot que lo mantienen).

### D6 — Badge de estado en README

Se añade el badge de estado del workflow al `README.md` (imagen que muestra
passing/failing en la portada del repo). Coste cero y da visibilidad inmediata del estado.

## Risks / Trade-offs

- **[Tests inestables al activarlos en CI]** El upstream los tenía apagados; es posible que
  algún unit test falle intermitentemente en runners (timing, cultura del SO, rutas). →
  Mitigación: los `.trx` se suben como artifact en cada fallo; el test problemático se
  documenta y se excluye individualmente (`--filter`) o se arregla — nunca apagar la suite.
- **[Primera ejecución lenta, ~10-15 min]** El runner parte de cero: restore NuGet completo +
  build Release + tests. → Aceptado en este change; si molesta, añadir `actions/cache` para
  NuGet es un cambio de ~10 líneas que puede venir después con datos reales de duración.
- **[Diferencias runner vs PC local]** `windows-latest` (Windows Server, en-US) no es idéntico
  a Windows 11 es-ES; algún test puede depender de cultura o de git config global. → Es
  justamente el valor de la CI: aflora suposiciones ocultas. Se tratan caso a caso.
- **[El YAML solo se prueba en GitHub]** No hay forma oficial de ejecutar el workflow en local
  (existe `act`, pero es imperfecto). → Mitigado por D1: el YAML queda tan fino (~40 líneas,
  la mitad comentarios) que apenas hay nada que pueda fallar salvo la preparación de la
  máquina, y eso se ve en el log del run.

## Migration Plan

1. Crear `eng/Verify.ps1` y validarlo **en local** hasta que pase entero (esto no toca CI).
2. Borrar los workflows heredados listados en D4 (commit separado, revertible).
3. Añadir `fork-ci.yml` + badge y hacer push a `avalonia/main` → observar el primer run en la
   pestaña Actions (momento didáctico: seguir el log step a step).
4. Si el primer run falla por entorno (no por código), iterar sobre el YAML; si falla por
   tests inestables, aplicar la política de D2/Risks.

Rollback: borrar `fork-ci.yml` (la verificación local sigue intacta); los workflows heredados
se recuperan de git si hicieran falta.

## Open Questions

- ¿Los unit tests requieren `git.exe` con alguna config global concreta en el runner?
  (`CommonTestUtils` usa LibGit2Sharp y repos de prueba). Se sabrá en el primer run; el runner
  trae git preinstalado, así que se espera que no haga falta nada.
- Duración real del run completo — medir en el primer run y decidir si compensa `actions/cache`.
