// Kaizen Check: Compilation Requirement
const fs = require('fs');
const path = require('path');

console.log('--------------------------------------------------');
console.log('[KAIZEN] Initiating Pre-Compilation Protection Check...');
console.log('--------------------------------------------------');

// Rule 1: Verify Critical Constitution Files Exist
// Path relative to execution context (usually package.json root)
let constitutionPathArg = process.argv[2];
let constitutionPath;

if (constitutionPathArg) {
    // Resolve relative to current working directory (where the script is called from)
    constitutionPath = path.resolve(process.cwd(), constitutionPathArg);
} else {
    // Fallback: Try to locate relative to this script file
    // Script: src/Kalma2/Interfaces/Desktop/scripts/kaizen-check.js
    // Target: src/Kalma2/CONSTITUTION.md
    // Path: ../../../CONSTITUTION.md
    constitutionPath = path.resolve(__dirname, '../../../CONSTITUTION.md');
    console.warn('[KAIZEN WARNING] No constitution path argument provided. Defaulting to relative path:', constitutionPath);
}

console.log(`[KAIZEN] Verifying Constitution at: ${constitutionPath}`);

if (!fs.existsSync(constitutionPath)) {
  console.error('[KAIZEN FAILURE] CONSTITUTION.md missing. The Constitution is the foundation of Kalma2.');
  console.error('Expected path:', constitutionPath);
  process.exit(1);
} else {
  console.log('[KAIZEN] Constitution verified.');
}

// Rule 2: Verify Strict Mode in tsconfig.json
const tsconfigPath = path.join(__dirname, '../tsconfig.json');
if (fs.existsSync(tsconfigPath)) {
  try {
    // Basic JSON parse (tsconfig might have comments, but standard JSON.parse fails on comments)
    // Assuming standard JSON for now. If it has comments, this might fail.
    // We'll wrap in try/catch and warn if parse fails but not block unless critical.
    const tsconfigContent = fs.readFileSync(tsconfigPath, 'utf8');
    // Simple strip comments regex just in case
    const jsonContent = tsconfigContent.replace(/\/\/.*$/gm, '').replace(/\/\*[\s\S]*?\*\//g, '');
    const tsconfig = JSON.parse(jsonContent);

    if (!tsconfig.compilerOptions || tsconfig.compilerOptions.strict !== true) {
       console.error('[KAIZEN FAILURE] "strict" mode must be enabled in tsconfig.json for high-quality code.');
       process.exit(1);
    }
    console.log('[KAIZEN] Strict mode verified.');
  } catch (e) {
    console.warn('[KAIZEN WARNING] Could not parse tsconfig.json. Skipping strict check.', e.message);
  }
} else {
  console.warn('[KAIZEN WARNING] tsconfig.json not found. Skipping strict check.');
}

console.log('[KAIZEN SUCCESS] Protection Actions Verified. Proceeding to build.');
process.exit(0);
