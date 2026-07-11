# Change 0.1 — CI propia del fork (local-first)

## Why

El fork es independiente del upstream (no se contribuye con PRs) pero no tiene ninguna verificación
automática propia: los workflows heredados de `.github/workflows/` solo disparan sobre
`master`/`release/*` (nunca sobre `avalonia/main`), dependen de infraestructura del upstream
(SignPath, labelers con modelos entrenados, CLA) y además tienen **los tests desactivados**
(`run-x64-tests: false`). Todos los changes de la Fase 0 (limpieza de `Extensibility`,
extracción de interops, canary multiplataforma) necesitan una red de seguridad que confirme en
cada push que la solución compila y los unit tests pasan — y el canary (change 0.4) necesitará
una pata Linux que solo tiene sentido sobre una CI ya funcionando.

## What Changes

- **Nuevo script local de verificación** `eng/Verify.ps1`: build de la solución + unit tests,
  con salida clara. Es el mecanismo primario de verificación del día a día (enfoque
  *local-first*: rápido, sin depender de red).
- **Nuevo workflow de GitHub Actions** `.github/workflows/fork-ci.yml` que dispara en push/PR
  sobre `avalonia/main` y actúa como **envoltorio fino**: checkout + submódulos + SDK .NET y
  llamada al mismo `eng/Verify.ps1`. Sin lógica de build propia en el YAML — si pasa en local,
  pasa en CI.
- **Neutralización de los workflows heredados** del upstream que no aplican al fork
  (labelers, CLA, pr-automation, stale, SignPath): se eliminan. `ge-build.yml` se conserva de
  momento como referencia (no dispara en `avalonia/main`) y se decidirá su futuro al absorber
  tags upstream.
- **Documentación didáctica**: el `design.md` de este change explica los conceptos de GitHub
  Actions desde cero (workflow, job, step, runner, trigger, artifact) y cada línea del YAML va
  comentada.
- Alcance de tests en CI: **solo unit tests** (`tests/app/UnitTests`, `tests/plugins`). Los
  tests de integración de UI (`UI.IntegrationTests`, instancian Forms reales) quedan fuera en
  este change por fragilidad conocida en runners; se reevaluará más adelante.

## Capabilities

### New Capabilities

- `local-verification`: scripts locales reproducibles para verificar el estado del repo
  (compilación completa + unit tests) con un solo comando, sin depender de GitHub.
- `continuous-integration`: verificación automática en GitHub Actions de cada push/PR a
  `avalonia/main`, como espejo fino de la verificación local; base sobre la que el change 0.4
  añadirá la pata Linux multiplataforma.

### Modified Capabilities

<!-- Ninguna: no existen capabilities previas en openspec/specs/. -->

## Impact

- **Ficheros nuevos**: `eng/Verify.ps1`, `.github/workflows/fork-ci.yml`.
- **Ficheros eliminados**: workflows heredados no aplicables (`cla-check.yml`,
  `label-lifecycle.yml`, `labeler-*.yml` ×4, `pr-automation.yml`, `pr-check-stale.yml`,
  `git.yml`). `ge-build.yml` se mantiene.
- **Sin impacto en código de producto**: no se toca ningún proyecto C# ni el sistema de build
  MSBuild; solo tooling.
- **Coste**: GitHub Actions es gratuito en repos públicos (runners estándar). Un run de
  build+tests en Windows tardará ~10-15 min (la build local son ~1 min, pero el runner parte
  de máquina limpia: restore NuGet completo + compilación sin caché).
- **Riesgo conocido**: puede aflorar algún unit test inestable (el upstream desactivó sus
  tests en CI citando `GetOriginalLineInPreviousCommit`). Si ocurre, se documenta y se decide
  excluirlo o arreglarlo — no se desactiva la suite entera.
