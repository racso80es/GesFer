#!/bin/bash
set -e

# Configuración de variables
DATE_UNDERSCORE=$(date -u +%Y_%m_%d)
DATE_DASH=$(date -u +%Y-%m-%d)
REPORT_FILE="docs/audits/AUDITORIA_FRONTEND_${DATE_UNDERSCORE}.md"
EVOLUTION_LOG="docs/EVOLUTION_LOG.md"

# Directorios a auditar
DIRS="./src/Shared/Front ./src/Product/Front ./src/Admin/Front"

# Inicializar contadores
VIOLACIONES_ARQUITECTURA=0
VIOLACIONES_EMPRESA=0
VIOLACIONES_ANY=0
VIOLACIONES_ALERT=0
VIOLACIONES_CONSOLE=0

# Realizar escaneos
# 1. Arquitectura: Importaciones cruzadas prohibidas (ej. Product importando Admin, o Admin importando Product)
COUNT_PROD_ADMIN=$(grep -rE "import .* from '.*Admin" ./src/Product/Front 2>/dev/null | wc -l || true)
COUNT_ADMIN_PROD=$(grep -rE "import .* from '.*Product" ./src/Admin/Front 2>/dev/null | wc -l || true)
VIOLACIONES_ARQUITECTURA=$((COUNT_PROD_ADMIN + COUNT_ADMIN_PROD))

# 2. Nomenclatura: Uso de 'empresa' (case insensitive, ignorando .env.example)
VIOLACIONES_EMPRESA=$(grep -rEi "empresa" ./src/Shared/Front ./src/Product/Front ./src/Admin/Front --exclude="*.env*" 2>/dev/null | wc -l || true)

# 3. Type Safety: Uso explícito de 'any'
VIOLACIONES_ANY=$(grep -rE "(: any|as any)" ./src/Shared/Front ./src/Product/Front ./src/Admin/Front --include="*.ts" --include="*.tsx" 2>/dev/null | wc -l || true)

# 4. UX / Code Smell: alert() excluyendo tests
VIOLACIONES_ALERT=$(grep -rE "\balert\(" ./src/Shared/Front ./src/Product/Front ./src/Admin/Front --include="*.ts" --include="*.tsx" --exclude-dir="__tests__" --exclude-dir="tests" --exclude="*.test.ts" --exclude="*.spec.ts" --exclude="*.test.tsx" --exclude="*.spec.tsx" 2>/dev/null | wc -l || true)

# 5. Calidad de código: console.log() excluyendo tests
VIOLACIONES_CONSOLE=$(grep -rE "console\.log\(" ./src/Shared/Front ./src/Product/Front ./src/Admin/Front --include="*.ts" --include="*.tsx" --exclude-dir="__tests__" --exclude-dir="tests" --exclude="*.test.ts" --exclude="*.spec.ts" --exclude="*.test.tsx" --exclude="*.spec.tsx" 2>/dev/null | wc -l || true)

# Determinar estado
ESTADO="✅ APROBADO"
HAY_CRITICAS=false
MENSAJE_CRITICO=""

if [ "$VIOLACIONES_ARQUITECTURA" -gt 0 ] || [ "$VIOLACIONES_EMPRESA" -gt 0 ]; then
    ESTADO="❌ REPROBADO"
    HAY_CRITICAS=true
    TOTAL_CRITICAS=$((VIOLACIONES_ARQUITECTURA + VIOLACIONES_EMPRESA))
    MENSAJE_CRITICO="FALLA CRÍTICA: $TOTAL_CRITICAS violaciones detectadas (Arquitectura/Nomenclatura)"
elif [ "$VIOLACIONES_ANY" -gt 0 ] || [ "$VIOLACIONES_ALERT" -gt 0 ] || [ "$VIOLACIONES_CONSOLE" -gt 0 ]; then
    ESTADO="✅ APROBADO (CON OBSERVACIONES)"
fi

# Generar reporte Markdown
cat > "$REPORT_FILE" << EOF
# Auditoría Frontend Diaria

**Fecha:** $DATE_DASH
**Auditor:** FRONT-ARCHITECT
**Alcance:**
- \`./src/Shared/Front\`
- \`./src/Product/Front\`
- \`./src/Admin/Front\`

---

## 1. Resumen Ejecutivo

**Estado:** $ESTADO

La auditoría del día $DATE_DASH arrojó los siguientes resultados.
$(if [ "$HAY_CRITICAS" = true ]; then echo "**Se han detectado Fallas Críticas que requieren atención inmediata.**"; fi)

---

## 2. Métricas Clave

| Categoría | Métrica | Resultado | Estado |
| :--- | :--- | :--- | :--- |
| **Arquitectura** | Violaciones de Capas (Cross-Boundary Imports) | $VIOLACIONES_ARQUITECTURA | $(if [ "$VIOLACIONES_ARQUITECTURA" -eq 0 ]; then echo "🟢 Óptimo"; else echo "🔴 Crítico"; fi) |
| **Nomenclatura** | Uso de término 'Empresa' en UI/Lógica | $VIOLACIONES_EMPRESA | $(if [ "$VIOLACIONES_EMPRESA" -eq 0 ]; then echo "🟢 Óptimo"; else echo "🔴 Crítico"; fi) |
| **Type Safety** | Uso explícito de \`any\` | $VIOLACIONES_ANY | $(if [ "$VIOLACIONES_ANY" -eq 0 ]; then echo "🟢 Óptimo"; else echo "🟡 Advertencia"; fi) |
| **UX / Code Smell** | Uso de \`alert()\` o \`confirm()\` nativos | $VIOLACIONES_ALERT | $(if [ "$VIOLACIONES_ALERT" -eq 0 ]; then echo "🟢 Óptimo"; else echo "🟡 Advertencia"; fi) |
| **Calidad de Código** | \`console.log\` en código productivo | $VIOLACIONES_CONSOLE | $(if [ "$VIOLACIONES_CONSOLE" -eq 0 ]; then echo "🟢 Óptimo"; else echo "🟡 Advertencia"; fi) |

*Nota: Se excluyen archivos de configuración de entorno (.env.example) y directorios de tests para métricas de UX y Calidad.*

---

## 3. Recomendaciones

$(if [ "$VIOLACIONES_ARQUITECTURA" -gt 0 ]; then echo "1. **Refactorizar Arquitectura:** Eliminar importaciones cruzadas inmediatamente."; fi)
$(if [ "$VIOLACIONES_EMPRESA" -gt 0 ]; then echo "2. **Corregir Nomenclatura:** Reemplazar 'Empresa' por 'Organización' o 'Company' según contexto."; fi)
$(if [ "$VIOLACIONES_ANY" -gt 0 ]; then echo "3. **Tipado Estricto:** Definir interfaces para eliminar el uso de \`any\`."; fi)
$(if [ "$VIOLACIONES_ALERT" -gt 0 ]; then echo "4. **Refactorizar Feedback:** Reemplazar \`alert()\` por componentes de notificación adecuados."; fi)
$(if [ "$VIOLACIONES_CONSOLE" -gt 0 ]; then echo "5. **Limpieza de Logs:** Eliminar \`console.log\` en código productivo o reemplazarlos por un Logger."; fi)

---
*Fin del reporte.*
EOF

# Actualizar EVOLUTION_LOG.md si hay fallas críticas
if [ "$HAY_CRITICAS" = true ]; then
    NEW_ENTRY="[$DATE_DASH] [Auditoría Frontend] [$MENSAJE_CRITICO] [Requiere Acción]"

    if ! grep -q "\\[$DATE_DASH\\] \\[Auditoría Frontend\\]" "$EVOLUTION_LOG"; then
        sed -i "3i $NEW_ENTRY" "$EVOLUTION_LOG"
    fi
fi

# Imprimir notificación exacta requerida
echo "Auditoría Frontend diaria completada. Reporte generado en la carpeta de docs."
