# Tasks — Change 0.1: CI propia del fork

## 1. Verificación local (`eng/Verify.ps1`)

- [x] 1.1 Crear `eng/Verify.ps1` con parámetro `-Configuration` (Release por defecto):
      build de `GitExtensions.slnx` y, si compila, `dotnet test --no-build` sobre los
      `*.csproj` de `tests/app/UnitTests/` y `tests/plugins/UnitTests/`, con logger `.trx`
      hacia `artifacts/`, resumen final OK/FAIL por proyecto y exit code agregado
- [x] 1.2 Ejecutar `.\eng\Verify.ps1` en local hasta que pase completo; si aflora algún test
      inestable, documentarlo en el change y excluirlo individualmente con justificación
      → pasó a la primera: 15/15 proyectos OK en 4:59 (Release, desde cero), sin inestables
- [x] 1.3 Verificar los escenarios de fallo: provocar un error de compilación y un test en
      rojo, y comprobar exit code ≠ 0 y resumen correcto en ambos casos
      → build rota: exit 1 sin ejecutar tests; test en rojo: 15/15 ejecutados, 1 FAIL, exit 1

## 2. Limpieza de workflows heredados

- [x] 2.1 Eliminar `cla-check.yml`, `label-lifecycle.yml`, `labeler-cache-retention.yml`,
      `labeler-predict-issues.yml`, `labeler-predict-pulls.yml`, `labeler-promote.yml`,
      `labeler-train.yml`, `pr-automation.yml`, `pr-check-stale.yml` y `git.yml` de
      `.github/workflows/` (commit propio, revertible); `ge-build.yml` se conserva

## 3. Workflow de CI (`fork-ci.yml`)

- [x] 3.1 Crear `.github/workflows/fork-ci.yml` comentado línea a línea (didáctico):
      triggers `push`/`pull_request` sobre `avalonia/main` + `workflow_dispatch`,
      `concurrency` con cancelación, job único en `windows-latest` con steps:
      checkout con submódulos → `actions/setup-dotnet` con `global.json` →
      `.\eng\Verify.ps1` → upload de `.trx` como artifact solo si falla
- [x] 3.2 Añadir el badge de estado del workflow al `README.md`
- [ ] 3.3 Push a `avalonia/main` y seguir el primer run en la pestaña Actions; anotar la
      duración total (dato para decidir si se añade caché NuGet más adelante)
- [ ] 3.4 Si el run falla por diferencias de entorno del runner, corregir en `Verify.ps1`
      (no en el YAML) y repetir hasta run verde

## 4. Verificación final y documentación

- [ ] 4.1 Comprobar los escenarios de las specs: run automático en push, disparo manual,
      cancelación por push consecutivo, artifact de `.trx` en un fallo forzado (opcional),
      e inventario de `.github/workflows/` (solo `fork-ci.yml` y `ge-build.yml`)
- [ ] 4.2 Actualizar la hoja de ruta (marcar 0.1 como completado) y registrar en el registro
      de decisiones interno cualquier decisión tomada durante la implementación (p. ej.
      tests excluidos y por qué)
