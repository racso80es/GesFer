# Observaciones: Ejecución Manual Consola / Opción 1

**Fecha:** 2026-02-14  
**Contexto:** Inicialización completa vía Consola GesFer, opción 1 (o Acción 3.4)

---

## 1. Tabla Logs no encontrada en BD

### Síntoma
La tabla `Logs` no existe en la base de datos al consultar (p. ej. página Admin `/logs`).

### Causa identificada
La tabla `Logs` pertenece al **dominio Admin** y se crea mediante **migraciones de Admin** (`AdminDbContext`), no de Product. La **Opción 7** (Aplicar migraciones) solo ejecuta migraciones de Product, por lo que Logs no se crea.

| Flujo | Migraciones aplicadas | Tabla Logs |
|-------|------------------------|------------|
| **Opción 1** (Inicialización completa) | Product + Admin | ✅ Creada |
| **Opción 3.4** (Inicialización Completa BD) | Product + Admin | ✅ Creada |
| **Opción 7** (Aplicar migraciones) | Solo Product | ❌ No creada |

### Migraciones relevantes
- `AdminDbContext`: `20260214110000_CreateLogsTableIfNotExists`, `20260214120000_AddMissingColumnsToLogs`

### Acción recomendada
1. Ejecutar **Opción 1** (Inicialización completa) o **Acción 3.4** (Inicialización Completa BD) para aplicar migraciones de Admin.
2. O bien, aplicar manualmente las migraciones de Admin:
   ```powershell
   cd src/Admin/Back/Api
   dotnet ef database update --project ../Infrastructure/GesFer.Admin.Infrastructure.csproj --startup-project .
   ```

---

## 2. Product Front devuelve 404 al consultar empresa

### Síntoma
La ruta `/my-company` (Mi Organización) devuelve 404 o "Organización no encontrada".

### Causa identificada
`getProductApi()` en `src/Product/Front/lib/api/product-api.ts` usaba por defecto `http://localhost:5002/api`, pero la **Product API** escucha en **5000** (HTTP) y **5001** (HTTPS).

### Corrección aplicada
- Cambio de `5002` → `5000` en el fallback de `NEXT_PUBLIC_PRODUCT_API_URL`.
- Archivo: `src/Product/Front/lib/api/product-api.ts`

### Nota
Para Docker, la variable `NEXT_PUBLIC_PRODUCT_API_URL` se define en el `docker-compose` y apunta a la API correcta. El fallback corregido afecta solo a ejecución local con `ejecutar-servicios.bat`.

---

## 3. Familias de artículos y tasas no se visualizan

### Síntoma
Las páginas de Familias de artículos y Tipos de tasa (TaxTypes) no muestran datos.

### Causas posibles

1. **404 Empresa (causa raíz)**: Si `/api/my-company` falla (véase punto 2), el token puede no tener `company_id` correctamente propagado, o el usuario no tiene empresa asociada. Los endpoints de ArticleFamilies y TaxTypes filtran por `CompanyId` del token; sin empresa válida, la lista queda vacía.

2. **API URL del cliente**: El `apiClient` (usado por ArticleFamilies, TaxTypes, etc.) utiliza `API_URL` de `lib/config.ts`:
   - Desarrollo: `https://localhost:5001` (HTTPS)
   - Si la Product API corre solo en HTTP (5000), las peticiones fallarían por protocolo o CORS.

3. **Seeds no ejecutados**: Si los datos demo (ArticleFamilies, TaxTypes, Companies) no se cargaron, las listas estarán vacías.

### Verificaciones
- Confirmar que la Product API está en marcha en `http://localhost:5000` y/o `https://localhost:5001`.
- Verificar que `NEXT_PUBLIC_API_URL` en Product Front apunte a la Product API (p. ej. `http://localhost:5000` o `https://localhost:5001`).
- Revisar que la Opción 1/3.4 completó correctamente los seeds (demo-data, ArticleFamilies, TaxTypes).

---

## Resumen de acciones

| Problema | Estado | Acción |
|----------|--------|--------|
| Tabla Logs | Documentado | Ejecutar Opción 1 o 3.4 (dominio Admin) |
| 404 Empresa | Corregido | `product-api.ts` usa puerto 5000 por defecto |
| Familias/Tasas vacías | Investigación | Verificar API URL, token company_id y seeds |
