import os
import datetime
import re

# Configuration
TARGET_DIRECTORIES = [
    "src/Shared/Front",
    "src/Product/Front",
    "src/Admin/Front"
]

FORBIDDEN_TERMS = ["empresa"]
ALLOWED_EXTENSIONS = [".ts", ".tsx", ".js", ".jsx", ".json", ".md", ".html", ".css", ".scss"]
EXCLUDED_DIRS = ["node_modules", ".git", "dist", "build", ".next", "coverage"]
EXCLUDED_FILES = [
    "src/Product/Front/lib/legacy-constants.ts"
]

REPORT_DIR = "docs/audits"
EVOLUTION_LOG = "docs/EVOLUTION_LOG.md"

def get_utc_date():
    return datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%d")

def scan_file(filepath):
    findings = {
        "forbidden_terms": [],
        "any_usage": 0,
        "ts_ignore": 0,
        "missing_alt": 0,
        "shared_leakage": [],
        "console_log": 0,
        "alert_confirm": 0,
        "alert_confirm_details": [],
        "any_details": []
    }

    try:
        # Check if file is excluded or has wrong extension
        rel_path = filepath.replace("\\", "/")
        if rel_path in EXCLUDED_FILES:
            return findings

        # Check allowed extensions
        if not any(filepath.endswith(ext) for ext in ALLOWED_EXTENSIONS):
            return findings

        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()
            lines = content.splitlines()

            # Check for forbidden terms
            for term in FORBIDDEN_TERMS:
                if term.lower() in content.lower():
                    # Find specific lines for context (simplified)
                    for i, line in enumerate(lines):
                        if term.lower() in line.lower():
                            findings["forbidden_terms"].append({
                                "term": term,
                                "line": i + 1,
                                "content": line.strip()
                            })

            # Check for technical debt (any, ts-ignore, console.log, alert/confirm)
            if filepath.endswith(('.ts', '.tsx', '.js', '.jsx')):
                # Use regex for better 'any' detection
                any_matches = re.finditer(r':\s*any\b|as\s+any\b', content)
                for match in any_matches:
                    findings["any_usage"] += 1
                    line_num = content[:match.start()].count('\n') + 1
                    findings["any_details"].append({
                        "line": line_num,
                        "content": lines[line_num-1].strip()
                    })

                findings["ts_ignore"] = content.count("@ts-ignore")
                findings["console_log"] = len(re.findall(r'console\.log\(', content))

                # alert/confirm detection
                alert_matches = re.finditer(r'\balert\(|\bconfirm\(', content)
                for match in alert_matches:
                    findings["alert_confirm"] += 1
                    line_num = content[:match.start()].count('\n') + 1
                    findings["alert_confirm_details"].append({
                        "line": line_num,
                        "content": lines[line_num-1].strip()
                    })

            # Check for accessibility (img without alt)
            if filepath.endswith(('.tsx', '.jsx', '.html')):
                # Simple regex for img tags
                img_tags = re.findall(r'<img[^>]*>', content)
                for tag in img_tags:
                    if 'alt=' not in tag:
                        findings["missing_alt"] += 1

            # Check for Shared Leakage (only in src/Shared/Front)
            if "src/Shared/Front" in filepath.replace("\\", "/"):
                # Regex for import paths
                patterns = [
                    r'from\s+[\'"]([^\'"]+)[\'"]',          # from "..." (covers import/export ... from)
                    r'import\s+[\'"]([^\'"]+)[\'"]',        # import "..." (side-effect)
                    r'require\s*\(\s*[\'"]([^\'"]+)[\'"]',  # require("...")
                    r'import\s*\(\s*[\'"]([^\'"]+)[\'"]'    # import("...")
                ]

                imports = []
                for pattern in patterns:
                    imports.extend(re.findall(pattern, content))

                for imp in imports:
                    if any(x in imp for x in ["/Product/", "/Admin/", "@product/", "@admin/"]) or \
                       imp.startswith("src/Product") or imp.startswith("src/Admin") or \
                       (imp.startswith("..") and ("Product" in imp or "Admin" in imp)):
                        findings["shared_leakage"].append(imp)


    except Exception as e:
        print(f"Error scanning {filepath}: {e}")

    return findings

def audit_directories():
    report_data = {
        "date": get_utc_date(),
        "forbidden_terms_count": 0,
        "forbidden_terms_details": [],
        "dependency_integrity": {},
        "tech_debt_any": 0,
        "tech_debt_any_details": [],
        "tech_debt_ts_ignore": 0,
        "accessibility_missing_alt": 0,
        "shared_leakage_count": 0,
        "shared_leakage_details": [],
        "console_log_count": 0,
        "alert_confirm_count": 0,
        "alert_confirm_details": [],
        "scanned_files": 0
    }

    # 1. Dependency Integrity Check
    for directory in ["src/Product/Front", "src/Admin/Front"]:
        lockfile = os.path.join(directory, "package-lock.json")
        if os.path.exists(lockfile):
            report_data["dependency_integrity"][directory] = "PRESENTE"
        else:
            report_data["dependency_integrity"][directory] = "AUSENTE"

    # 2. File Scan
    for root_dir in TARGET_DIRECTORIES:
        if not os.path.exists(root_dir):
            print(f"Directory not found: {root_dir}")
            continue

        for root, dirs, files in os.walk(root_dir):
            # Exclude directories
            dirs[:] = [d for d in dirs if d not in EXCLUDED_DIRS]

            for file in files:
                filepath = os.path.join(root, file)
                findings = scan_file(filepath)

                report_data["scanned_files"] += 1
                report_data["tech_debt_any"] += findings["any_usage"]
                report_data["tech_debt_ts_ignore"] += findings["ts_ignore"]
                report_data["accessibility_missing_alt"] += findings["missing_alt"]
                report_data["console_log_count"] += findings["console_log"]
                report_data["alert_confirm_count"] += findings["alert_confirm"]

                if findings["shared_leakage"]:
                    report_data["shared_leakage_count"] += len(findings["shared_leakage"])
                    for leak in findings["shared_leakage"]:
                            report_data["shared_leakage_details"].append({
                            "file": filepath,
                            "leak": leak
                        })

                if findings["forbidden_terms"]:
                    report_data["forbidden_terms_count"] += len(findings["forbidden_terms"])
                    for finding in findings["forbidden_terms"]:
                        report_data["forbidden_terms_details"].append({
                            "file": filepath,
                            "term": finding["term"],
                            "line": finding["line"],
                            "content": finding["content"]
                        })

                if findings["alert_confirm_details"]:
                    for finding in findings["alert_confirm_details"]:
                        report_data["alert_confirm_details"].append({
                            "file": filepath,
                            "line": finding["line"],
                            "content": finding["content"]
                        })

                if findings["any_details"]:
                     for finding in findings["any_details"]:
                        report_data["tech_debt_any_details"].append({
                            "file": filepath,
                            "line": finding["line"],
                            "content": finding["content"]
                        })


    return report_data

def generate_report(data):
    filename = f"AUDITORIA_FRONTEND_{data['date'].replace('-', '_')}.md"
    filepath = os.path.join(REPORT_DIR, filename)

    status_text = "✅ APROBADO (CON OBSERVACIONES)"
    status_state = "🟢 Óptimo"

    if data["forbidden_terms_count"] > 0 or data["shared_leakage_count"] > 0:
        status_text = "🔴 RECHAZADO (FALLAS CRÍTICAS)"
        status_state = "🔴 CRÍTICO"
    elif data["tech_debt_any"] > 20 or data["alert_confirm_count"] > 5:
         status_text = "🟡 ALERTA (DEUDA TÉCNICA ELEVADA)"
         status_state = "🟡 ALERTA"

    content = f"""# Auditoría Frontend Diaria

**Fecha:** {data['date']}
**Auditor:** FRONT-ARCHITECT
**Alcance:**
- `./src/Shared/Front`
- `./src/Product/Front`
- `./src/Admin/Front`

---

## 1. Resumen Ejecutivo

**Estado:** {status_text}

La auditoría del día {data['date']} muestra un estado {status_state.split(' ')[1].lower() if len(status_state.split(' ')) > 1 else 'general'}.
"""
    if data["forbidden_terms_count"] == 0 and data["shared_leakage_count"] == 0:
        content += "No se detectaron violaciones críticas de arquitectura (importaciones cruzadas prohibidas) ni violaciones de nomenclatura ('Empresa' vs 'Organización') en el código fuente productivo.\n"
    else:
        content += "Se han detectado **FALLAS CRÍTICAS** que requieren atención inmediata.\n"

    if data["tech_debt_any"] > 0 or data["alert_confirm_count"] > 0:
        content += "\nSin embargo, se han detectado deudas técnicas menores (uso de `any`, `alert`, `console.log`) que deben ser remediadas en el próximo ciclo de mejora.\n"

    content += f"""
---

## 2. Métricas Clave

| Categoría | Métrica | Resultado | Estado |
| :--- | :--- | :--- | :--- |
| **Arquitectura** | Violaciones de Capas (Cross-Boundary Imports) | {data['shared_leakage_count']} | {'🟢 Óptimo' if data['shared_leakage_count'] == 0 else '🔴 CRÍTICO'} |
| **Nomenclatura** | Uso de término 'Empresa' en UI/Lógica | {data['forbidden_terms_count']}* | {'🟢 Óptimo' if data['forbidden_terms_count'] == 0 else '🔴 CRÍTICO'} |
| **Accesibilidad** | Imágenes sin texto alternativo (`alt`) | {data['accessibility_missing_alt']} | {'🟢 Óptimo' if data['accessibility_missing_alt'] == 0 else '🔴 FALLA'} |
| **Calidad de Código** | `console.log` en código productivo | {data['console_log_count']} | {'🟢 Óptimo' if data['console_log_count'] == 0 else '🟡 Advertencia'} |
| **UX / Code Smell** | Uso de `alert()` o `confirm()` nativos | {data['alert_confirm_count']} | {'🟢 Óptimo' if data['alert_confirm_count'] == 0 else '🟡 Advertencia'} |
| **Type Safety** | Uso explícito de `any` | {data['tech_debt_any']} | {'🟢 Óptimo' if data['tech_debt_any'] == 0 else '🟡 Advertencia'} |

\\*Nota: Se excluyen archivos de configuración de entorno (.env.example).*

---

## 3. Hallazgos Detallados
"""

    # 3.1 UX / Code Smell
    if data["alert_confirm_details"]:
        content += "\n### 3.1. Experiencia de Usuario y Code Smells (`alert`)\n"
        content += "Se detectó el uso de `alert()` o `confirm()` nativo, lo cual bloquea el hilo principal.\n\n"
        files_map = {}
        for item in data["alert_confirm_details"]:
            if item['file'] not in files_map: files_map[item['file']] = []
            files_map[item['file']].append(item)

        for file, items in files_map.items():
            content += f"- **Archivo:** `{file}`\n"
            for item in items[:3]:
                content += f"  - Línea {item['line']}: `{item['content'][:80]}...`\n"
            if len(items) > 3:
                 content += f"  - ... y {len(items) - 3} más.\n"

    # 3.2 Type Safety
    if data["tech_debt_any_details"]:
        content += "\n### 3.2. Seguridad de Tipos (TypeScript `any`)\n"
        content += "Se detectó el uso de `any` explícito.\n\n"
        files_map = {}
        for item in data["tech_debt_any_details"]:
            if item['file'] not in files_map: files_map[item['file']] = []
            files_map[item['file']].append(item)

        count = 0
        for file, items in files_map.items():
            if count >= 5: # Limit detailed output
                content += f"- ... y otros archivos con `any`.\n"
                break
            content += f"- **Archivo:** `{file}`\n"
            for item in items[:2]:
                content += f"  - Línea {item['line']}: `{item['content'][:80]}...`\n"
            count += 1

    # 3.3 Architecture
    if data["shared_leakage_details"]:
        content += "\n### 3.3. Arquitectura (Shared Leakage)\n"
        content += "Violaciones de arquitectura detectadas en `src/Shared/Front`.\n\n"
        for item in data["shared_leakage_details"]:
            content += f"- **{item['file']}**: Importa `{item['leak']}`\n"

    # 3.4 Nomenclature
    if data["forbidden_terms_details"]:
        content += "\n### 3.4. Nomenclatura ('Empresa')\n"
        content += "Uso prohibido del término 'Empresa'.\n\n"
        files_map = {}
        for item in data["forbidden_terms_details"]:
             if item['file'] not in files_map: files_map[item['file']] = []
             files_map[item['file']].append(item)

        for file, items in files_map.items():
            content += f"- **Archivo:** `{file}`\n"
            for item in items[:3]:
                 content += f"  - Línea {item['line']}: `{item['content'][:80]}...`\n"

    content += """
---

## 4. Recomendaciones

1.  **Refactorizar Feedback de Usuario:** Reemplazar `alert()` por componentes de notificación (Toast).
2.  **Tipado Estricto:** Definir interfaces para eliminar `any`.
3.  **Mantener Vigilancia:** Continuar con la política de cero tolerancia a importaciones cruzadas.

---

*Fin del reporte.*
"""

    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)

    print(f"Report generated at: {filepath}")
    return data

def update_evolution_log(data):
    if data["forbidden_terms_count"] > 0 or data["shared_leakage_count"] > 0:
        log_entry_prefix = f"[{data['date']}] [Auditoría Frontend]"

        # Determine failure message
        failures = []
        if data["forbidden_terms_count"] > 0:
            failures.append(f"{data['forbidden_terms_count']} violaciones de 'empresa'")
        if data["shared_leakage_count"] > 0:
            failures.append(f"{data['shared_leakage_count']} violaciones de Shared Leakage")

        failure_msg = ", ".join(failures)
        log_entry = f"{log_entry_prefix} [FALLA CRÍTICA: {failure_msg} detectadas] [Requiere Acción]"

        # Check for duplicates
        if os.path.exists(EVOLUTION_LOG):
            with open(EVOLUTION_LOG, 'r', encoding='utf-8') as f:
                content = f.read()
                if log_entry in content:
                    print(f"Skipping duplicate log entry for {data['date']}")
                    return

        with open(EVOLUTION_LOG, 'a', encoding='utf-8') as f:
            f.write(f"\n{log_entry}")
        print(f"Evolution log updated: {EVOLUTION_LOG}")

if __name__ == "__main__":
    print("Starting Daily Frontend Audit...")
    data = audit_directories()
    generate_report(data)
    update_evolution_log(data)
    print("Auditoría Frontend diaria completada. Reporte generado en la carpeta de docs.")
