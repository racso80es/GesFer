# Objetivo de la Rama: fix-benchmark-compilation

**Fecha:** 2026-02-14
**Rama:** kaizen/2026-02-14-fix-benchmark-compilation
**Autor:** Agente Kaizen

## Descripción
Esta rama soluciona el error de compilación crítico en el proyecto `GesFer.Performance.Benchmarks`.

## Problema
El proyecto de benchmarks utilizaba una estructura de entidad `Article` obsoleta, intentando asignar una propiedad `Family` que ha sido reemplazada por `ArticleFamily`.

## Solución Implementada
*   Se actualizó `StockBenchmark.cs` para utilizar `ArticleFamily` y `TaxType`.
*   Se inicializan las entidades dependientes (`TaxType`, `ArticleFamily`) dentro del setup del benchmark.

## Verificación
*   `dotnet build src/Performance/GesFer.Performance.Benchmarks/GesFer.Performance.Benchmarks.csproj` -> ÉXITO
*   `dotnet build GesFer.sln` -> ÉXITO
*   `dotnet test GesFer.sln` -> ÉXITO (Sin regresiones)
