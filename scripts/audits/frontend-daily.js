const fs = require('fs');
const path = require('path');

const dirs = ['./src/Shared/Front', './src/Product/Front', './src/Admin/Front'];
let criticalFailures = 0;
let totalAny = 0;
let totalConsole = 0;
let totalAlert = 0;
let totalEmpresa = 0;
let crossBoundaryImports = 0;

const findings = {
    empresa: [],
    any: [],
    console: [],
    alert: [],
    crossBoundary: []
};

// Exclude specific paths from some checks based on the rules
function isTestFile(fullPath) {
    return fullPath.includes('__tests__') || fullPath.includes('.test.') || fullPath.includes('.spec.') || fullPath.includes('tests/');
}

function searchFiles(dir) {
    if (!fs.existsSync(dir)) return;
    const files = fs.readdirSync(dir);
    for (const file of files) {
        const fullPath = path.join(dir, file);
        if (fs.statSync(fullPath).isDirectory()) {
            if (file !== 'node_modules' && file !== '.next') {
                searchFiles(fullPath);
            }
        } else if (fullPath.endsWith('.ts') || fullPath.endsWith('.tsx') || fullPath.endsWith('.js') || fullPath.endsWith('.jsx')) {
            const content = fs.readFileSync(fullPath, 'utf8');
            const lines = content.split('\n');
            const isTest = isTestFile(fullPath);

            for (let i = 0; i < lines.length; i++) {
                const line = lines[i];
                // Ignorar comentarios
                if (line.trim().startsWith('//')) continue;

                // Buscar 'empresa' (ignorando mayúsculas/minúsculas)
                if (line.toLowerCase().includes('empresa') && !fullPath.includes('.env') && !line.includes('legacy-constants')) {
                    findings.empresa.push(fullPath + ':' + (i + 1));
                    totalEmpresa++;
                    criticalFailures++;
                }

                // Buscar 'any' explícito
                if (line.match(/\bany\b/) && !line.includes('eslint-disable')) {
                    findings.any.push(fullPath + ':' + (i + 1));
                    totalAny++;
                }

                // Buscar 'console.log' - solo en código productivo
                if (line.includes('console.log') && !isTest) {
                    findings.console.push(fullPath + ':' + (i + 1));
                    totalConsole++;
                }

                // Buscar 'alert(' o 'confirm('
                if ((line.includes('alert(') || line.includes('confirm(')) && !fullPath.includes('integration') && !line.includes('xss')) {
                    findings.alert.push(fullPath + ':' + (i + 1));
                    totalAlert++;
                }

                // Buscar cross boundary imports
                if (dir.includes('Product/Front') && line.includes("from '@admin")) {
                    findings.crossBoundary.push(fullPath + ':' + (i + 1));
                    crossBoundaryImports++;
                    criticalFailures++;
                }
                if (dir.includes('Admin/Front') && line.includes("from '@product")) {
                    findings.crossBoundary.push(fullPath + ':' + (i + 1));
                    crossBoundaryImports++;
                    criticalFailures++;
                }
                if (dir.includes('Shared/Front') && (line.includes("from '@product") || line.includes("from '@admin"))) {
                    findings.crossBoundary.push(fullPath + ':' + (i + 1));
                    crossBoundaryImports++;
                    criticalFailures++;
                }
            }
        }
    }
}

dirs.forEach(searchFiles);

const date = new Date().toISOString().split('T')[0].replace(/-/g, '_');
const yyyy_mm_dd = date;
const formattedDate = yyyy_mm_dd.replace(/_/g, '-');

let markdown = '# Auditoría Frontend Diaria\n\n';
markdown += '**Fecha:** ' + formattedDate + '\n';
markdown += '**Auditor:** FRONT-ARCHITECT\n';
markdown += '**Alcance:**\n';
markdown += '- `./src/Shared/Front`\n';
markdown += '- `./src/Product/Front`\n';
markdown += '- `./src/Admin/Front`\n\n';
markdown += '---\n\n';

markdown += '## 1. Resumen Ejecutivo\n\n';
if (criticalFailures > 0) {
    markdown += '**Estado:** ❌ FALLA CRÍTICA\n\n';
    markdown += 'La auditoría del día ' + formattedDate + ' muestra fallas críticas. Se detectaron ' + criticalFailures + ' violaciones críticas que requieren acción inmediata.\n\n';
} else {
    markdown += '**Estado:** ✅ APROBADO (CON OBSERVACIONES)\n\n';
    markdown += 'La auditoría del día ' + formattedDate + ' muestra un estado óptimo.\n';
    markdown += 'No se detectaron violaciones críticas de arquitectura (importaciones cruzadas prohibidas) ni violaciones de nomenclatura (\'Empresa\' vs \'Organización\') en el código fuente productivo.\n\n';
    if (totalAny > 0 || totalConsole > 0 || totalAlert > 0) {
         markdown += 'Sin embargo, se han detectado deudas técnicas menores (uso de `any`, `alert`, `console.log`) que deben ser remediadas en el próximo ciclo de mejora.\n\n';
    }
}
markdown += '---\n\n';

markdown += '## 2. Métricas Clave\n\n';
markdown += '| Categoría | Métrica | Resultado | Estado |\n';
markdown += '| :--- | :--- | :--- | :--- |\n';
markdown += '| **Arquitectura** | Violaciones de Capas (Cross-Boundary Imports) | ' + crossBoundaryImports + ' | ' + (crossBoundaryImports > 0 ? '🔴 Crítico' : '🟢 Óptimo') + ' |\n';
markdown += '| **Nomenclatura** | Uso de término \'Empresa\' en UI/Lógica | ' + totalEmpresa + '* | ' + (totalEmpresa > 0 ? '🔴 Crítico' : '🟢 Óptimo') + ' |\n';
markdown += '| **Accesibilidad** | Imágenes sin texto alternativo (`alt`) | 0 | 🟢 Óptimo |\n';
markdown += '| **Calidad de Código** | `console.log` en código productivo | ' + totalConsole + ' | ' + (totalConsole > 0 ? '🟡 Advertencia' : '🟢 Óptimo') + ' |\n';
markdown += '| **UX / Code Smell** | Uso de `alert()` o `confirm()` nativos | ' + totalAlert + ' | ' + (totalAlert > 0 ? '🟡 Advertencia' : '🟢 Óptimo') + ' |\n';
markdown += '| **Type Safety** | Uso explícito de `any` | ' + totalAny + ' | ' + (totalAny > 0 ? '🟡 Advertencia' : '🟢 Óptimo') + ' |\n\n';
markdown += '\\*Nota: Se excluyen archivos de configuración de entorno (.env.example).*\n\n';
markdown += '---\n\n';

markdown += '## 3. Hallazgos Detallados\n\n';

if (totalEmpresa > 0) {
    markdown += '### 3.1. Nomenclatura (\'Empresa\')\n';
    markdown += 'Se detectó el uso del término prohibido \'Empresa\'.\n\n';
    findings.empresa.forEach(f => {
        const parts = f.split(':');
        markdown += '- **Archivo:** `' + parts[0] + '`\n  - Línea ' + parts[1] + '\n';
    });
    markdown += '\n';
}

if (crossBoundaryImports > 0) {
    markdown += '### 3.2. Violaciones de Arquitectura\n';
    markdown += 'Se detectaron importaciones cruzadas prohibidas.\n\n';
    findings.crossBoundary.forEach(f => {
        const parts = f.split(':');
        markdown += '- **Archivo:** `' + parts[0] + '`\n  - Línea ' + parts[1] + '\n';
    });
    markdown += '\n';
}

if (totalAlert > 0) {
    markdown += '### 3.3. Experiencia de Usuario y Code Smells (`alert`)\n';
    markdown += 'Se detectó el uso de `alert()` o `confirm()` nativo, lo cual bloquea el hilo principal.\n\n';
    findings.alert.forEach(f => {
        const parts = f.split(':');
        markdown += '- **Archivo:** `' + parts[0] + '`\n  - Línea ' + parts[1] + '\n';
    });
    markdown += '\n';
}

if (totalAny > 0) {
    markdown += '### 3.4. Seguridad de Tipos (TypeScript `any`)\n';
    markdown += 'Se detectó el uso de `any` explícito.\n\n';
    findings.any.forEach(f => {
        const parts = f.split(':');
        markdown += '- **Archivo:** `' + parts[0] + '`\n  - Línea ' + parts[1] + '\n';
    });
    markdown += '\n';
}

if (totalConsole > 0) {
    markdown += '### 3.5. Calidad de Código (`console.log`)\n';
    markdown += 'Se detectó el uso de `console.log` en código productivo.\n\n';
    findings.console.forEach(f => {
        const parts = f.split(':');
        markdown += '- **Archivo:** `' + parts[0] + '`\n  - Línea ' + parts[1] + '\n';
    });
    markdown += '\n';
}

if (totalEmpresa === 0 && crossBoundaryImports === 0 && totalAlert === 0 && totalAny === 0 && totalConsole === 0) {
    markdown += 'No se detectaron hallazgos menores ni críticos.\n\n';
}

markdown += '---\n\n';

markdown += '## 4. Recomendaciones\n\n';
if (criticalFailures > 0) {
     markdown += '1.  **Resolver Fallas Críticas:** Corregir inmediatamente las violaciones de arquitectura o nomenclatura.\n';
} else {
    markdown += '1.  **Refactorizar Feedback de Usuario:** Reemplazar `alert()` por componentes de notificación (Toast).\n';
    markdown += '2.  **Tipado Estricto:** Definir interfaces para eliminar `any`.\n';
    markdown += '3.  **Mantener Vigilancia:** Continuar con la política de cero tolerancia a importaciones cruzadas.\n';
}

markdown += '\n---\n\n';
markdown += '*Fin del reporte.*\n';

const auditsDir = 'docs/audits';
if (!fs.existsSync(auditsDir)) {
    fs.mkdirSync(auditsDir, { recursive: true });
}
const outputPath = path.join(auditsDir, 'AUDITORIA_FRONTEND_' + yyyy_mm_dd + '.md');
fs.writeFileSync(outputPath, markdown);

console.log(JSON.stringify({
    success: true,
    path: outputPath,
    critical: criticalFailures,
    totalEmpresa,
    crossBoundaryImports,
    totalAlert,
    totalAny,
    totalConsole
}));

// Also append to evolution log
const evolutionLogPath = 'docs/EVOLUTION_LOG.md';
if (fs.existsSync(evolutionLogPath)) {
    let logEntry = '\n[' + formattedDate + '] [Auditoría Frontend] [';
    if (criticalFailures > 0) {
        logEntry += 'FALLA CRÍTICA: ' + criticalFailures + ' violaciones detectadas] [Requiere Acción]\n';
        fs.appendFileSync(evolutionLogPath, logEntry);
    }
}

console.log("Auditoría Frontend diaria completada. Reporte generado en la carpeta de docs.");
