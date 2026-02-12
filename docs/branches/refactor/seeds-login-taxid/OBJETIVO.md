# Rama: refactor/seeds-login-taxid

## Objetivo

Unificar en master los cambios de: seeds como única vía de carga masiva, configuración de login por defecto en fronts y corrección de TaxIds en seeds.

## Contenido

- **Backend**: Companies en demo-data sembradas por JsonDataSeeder; DbInitializer solo repara admin (no crea Company/User); README seeds actualizado.
- **Front**: Variables de entorno para valores por defecto de login (Product y Admin) alineados con seeds; tests actualizados.
- **Seeds**: TaxIds corregidos en demo-data.json y Admin companies.json (CIF con dígito de control válido).

## Certificación

Compilación y script Unificar-Rama.ps1. Merge a master tras certificación.
