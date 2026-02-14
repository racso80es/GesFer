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
        "shared_leakage": []
    }

    try:
        with open(filepath, 'r', encoding='utf-8') as f:
            content = f.read()
            lines = content.splitlines()

            # Check for forbidden terms
            if filepath.replace("\\", "/") not in EXCLUDED_FILES:
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

            # Check for technical debt (any, ts-ignore)
            if filepath.endswith(('.ts', '.tsx', '.js', '.jsx')):
                # Use regex for better 'any' detection
                findings["any_usage"] = len(re.findall(r':\s*any', content)) + len(re.findall(r'as\s+any', content))
                findings["ts_ignore"] = content.count("@ts-ignore")

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
        "tech_debt_ts_ignore": 0,
        "accessibility_missing_alt": 0,
        "shared_leakage_count": 0,
        "shared_leakage_details": [],
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
                if any(file.endswith(ext) for ext in ALLOWED_EXTENSIONS):
                    filepath = os.path.join(root, file)
                    findings = scan_file(filepath)

                    report_data["scanned_files"] += 1
                    report_data["tech_debt_any"] += findings["any_usage"]
                    report_data["tech_debt_ts_ignore"] += findings["ts_ignore"]
                    report_data["accessibility_missing_alt"] += findings["missing_alt"]

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

    return report_data

def generate_report(data):
    filename = f"AUDITORIA_FRONTEND_{data['date'].replace('-', '_')}.md"
    filepath = os.path.join(REPORT_DIR, filename)

    status = "🟢 OK"
    if data["forbidden_terms_count"] > 0 or data["shared_leakage_count"] > 0:
        status = "🔴 FALLA CRÍTICA"
    elif data["tech_debt_any"] > 20:
        status = "🟡 ALERTA"

    content = f"""# AUDITORÍA FRONTEND — {data['date']}

**Auditor:** FRONT-ARCHITECT (Senior Frontend Quality Assurance & Accessibility Auditor)
**Fecha:** {data['date']} (UTC)
**Estado:** {status}

---

## 1. Resumen Ejecutivo

La auditoría diaria ha finalizado.
Estado Global: **{status}**
"""

    if data["forbidden_terms_count"] > 0:
        content += "\nSe han detectado **FALLAS CRÍTICAS** relacionadas con terminología prohibida ('empresa').\n"
    elif data["shared_leakage_count"] > 0:
        content += "\nSe han detectado **FALLAS CRÍTICAS** de arquitectura (Shared Leakage).\n"
    else:
        content += "\nNo se han detectado violaciones críticas.\n"

    content += f"""
## 2. Métricas Clave

| Métrica | Valor | Estado |
| :--- | :--- | :--- |
| **Violaciones de Terminología ("empresa")** | **{data['forbidden_terms_count']}** | {'🔴 CRÍTICO' if data['forbidden_terms_count'] > 0 else '🟢 PASA'} |
| **Integridad de Dependencias (Lockfiles)** | **{all(v == 'PRESENTE' for v in data['dependency_integrity'].values())}** | {'🟢 PASA' if all(v == 'PRESENTE' for v in data['dependency_integrity'].values()) else '🔴 FALLA'} |
| **Deuda Técnica (`any`)** | **{data['tech_debt_any']}** | {'🟡 ALERTA' if data['tech_debt_any'] > 10 else '🟢 PASA'} |
| **Deuda Técnica (`@ts-ignore`)** | **{data['tech_debt_ts_ignore']}** | {'🟡 ALERTA' if data['tech_debt_ts_ignore'] > 0 else '🟢 PASA'} |
| **Accesibilidad (Imágenes sin Alt)** | **{data['accessibility_missing_alt']}** | {'🔴 FALLA' if data['accessibility_missing_alt'] > 0 else '🟢 PASA'} |
| **Shared Leakage (Dependencias Circulares)** | **{data['shared_leakage_count']}** | {'🔴 CRÍTICO' if data['shared_leakage_count'] > 0 else '🟢 PASA'} |

## 3. Hallazgos Detallados

### 3.1 Terminología Prohibida ("empresa")
"""
    if data["forbidden_terms_details"]:
        content += "Se han encontrado las siguientes violaciones:\n\n"
        files_with_issues = {}
        for item in data["forbidden_terms_details"]:
            if item['file'] not in files_with_issues:
                files_with_issues[item['file']] = []
            files_with_issues[item['file']].append(item)

        for file, items in files_with_issues.items():
            content += f"- **{file}**\n"
            for item in items[:3]:
                content += f"  - Line {item['line']}: `{item['content'][:100]}...`\n"
            if len(items) > 3:
                content += f"  - ... y {len(items) - 3} más.\n"
    else:
        content += "Ninguna violación detectada.\n"

    content += """
### 3.2 Integridad de Dependencias
"""
    for dir, status in data["dependency_integrity"].items():
        content += f"- `{dir}/package-lock.json`: {status}\n"
    content += "- `src/Shared/Front`: N/A (Librería compartida)\n"

    content += f"""
### 3.3 Calidad de Código
- Se detectaron **{data['tech_debt_any']}** usos de `any`.
- Se detectaron **{data['tech_debt_ts_ignore']}** usos de `@ts-ignore`.
"""

    content += """
### 3.4 Shared Leakage
"""
    if data["shared_leakage_details"]:
        content += "Violaciones de arquitectura detectadas en `src/Shared/Front` (importando Product/Admin):\n"
        for item in data["shared_leakage_details"]:
            content += f"- **{item['file']}**: Importa `{item['leak']}`\n"
    else:
        content += "Ninguna violación detectada.\n"

    content += """
## 4. Conclusión

"""
    if data["forbidden_terms_count"] > 0 or data["shared_leakage_count"] > 0:
        content += "El estado actual es **CRÍTICO**. Se requiere intervención inmediata."
    elif data["tech_debt_any"] > 20:
        content += "El estado actual requiere atención para reducir la deuda técnica."
    else:
        content += "El estado actual es saludable."

    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)

    print(f"Report generated at: {filepath}")
    return status

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

        # Check for duplicates (simple check to avoid spamming same day if message is identical)
        if os.path.exists(EVOLUTION_LOG):
            with open(EVOLUTION_LOG, 'r', encoding='utf-8') as f:
                content = f.read()
                # If exact same entry exists, skip
                if log_entry in content:
                    print(f"Skipping duplicate log entry for {data['date']}")
                    return

        with open(EVOLUTION_LOG, 'a', encoding='utf-8') as f:
            f.write(f"\n{log_entry}")
        print(f"Evolution log updated: {EVOLUTION_LOG}")

if __name__ == "__main__":
    print("Starting Daily Frontend Audit...")
    data = audit_directories()
    status = generate_report(data)
    update_evolution_log(data)
    print("Auditoría Frontend diaria completada. Reporte generado en la carpeta de docs.")
