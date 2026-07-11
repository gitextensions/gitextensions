# continuous-integration — CI del fork en GitHub Actions

## ADDED Requirements

### Requirement: Verificación automática de avalonia/main
El repositorio SHALL contener un workflow de GitHub Actions (`.github/workflows/fork-ci.yml`)
que ejecute la verificación en un runner Windows en cada push a `avalonia/main`, en cada pull
request cuya rama destino sea `avalonia/main`, y bajo demanda mediante disparo manual
(`workflow_dispatch`).

#### Scenario: Push a la rama de trabajo
- **WHEN** se hace push de un commit a `avalonia/main`
- **THEN** GitHub Actions arranca un run del workflow y el commit queda marcado con el
  resultado (✓/✗) visible en el historial

#### Scenario: Disparo manual
- **WHEN** el usuario pulsa "Run workflow" en la pestaña Actions de GitHub
- **THEN** el workflow se ejecuta sobre la rama seleccionada sin necesidad de un push

### Requirement: CI como envoltorio fino del script local
El workflow SHALL limitarse a preparar la máquina (checkout con submódulos, instalación del
SDK .NET según `global.json`) e invocar `eng/Verify.ps1`, y SHALL NOT contener lógica propia
de compilación o selección de tests.

#### Scenario: Paridad local/CI
- **WHEN** `eng/Verify.ps1` pasa en una máquina local limpia con submódulos inicializados
- **THEN** el mismo commit pasa en CI, salvo diferencias de entorno del runner (que se
  consideran defectos a corregir en el script, no en el YAML)

### Requirement: Diagnóstico de fallos descargable
El workflow SHALL publicar como artifact los resultados `.trx` de los tests cuando la
verificación falle, con retención suficiente para su análisis posterior.

#### Scenario: Run fallido por tests
- **WHEN** un run falla porque uno o más unit tests no pasan
- **THEN** la página del run ofrece un artifact descargable con los `.trx` generados

### Requirement: Cancelación de runs superados
El workflow SHALL cancelar automáticamente los runs en curso de una rama cuando llega un
nuevo push a esa misma rama.

#### Scenario: Dos pushes consecutivos
- **WHEN** se hace push a `avalonia/main` mientras el run del push anterior sigue en curso
- **THEN** el run anterior se cancela y solo el nuevo llega a completarse

### Requirement: Workflows heredados del upstream retirados
El directorio `.github/workflows/` SHALL contener únicamente workflows aplicables al fork:
`fork-ci.yml` y `ge-build.yml` (conservado como referencia inerte del empaquetado upstream).
Los workflows de infraestructura del upstream (CLA, labelers, automatización de PRs/stale,
`git.yml`) SHALL quedar eliminados.

#### Scenario: Inventario de workflows
- **WHEN** se lista `.github/workflows/` tras implementar el change
- **THEN** solo existen `fork-ci.yml` y `ge-build.yml`

### Requirement: Visibilidad del estado en el README
El `README.md` SHALL mostrar un badge con el estado del último run del workflow del fork
sobre `avalonia/main`.

#### Scenario: Estado visible en la portada
- **WHEN** se visita la página principal del repositorio en GitHub
- **THEN** el README muestra el badge passing/failing enlazado a la pestaña Actions
