/**
 * Fallback build que invoca el CLI de Next sin depender del binario next (dist/bin/next).
 * Evita MODULE_NOT_FOUND cuando node_modules/next está incompleto o en entornos problemáticos.
 * Ejecutar desde la raíz del proyecto (Product/Front o Admin/Front).
 */
const path = require('path');
const nextBuildPath = path.join(__dirname, '..', 'node_modules', 'next', 'dist', 'cli', 'next-build.js');
let nextBuild;
try {
  nextBuild = require(nextBuildPath).nextBuild;
} catch (e) {
  console.error('No se pudo cargar next/dist/cli/next-build. Ejecuta npm install en este directorio.');
  process.exit(1);
}
const projectDir = path.resolve(process.cwd());
const p = nextBuild(
  { lint: true, mangling: true },
  projectDir
);
if (p && typeof p.then === 'function') {
  p.then(() => process.exit(0)).catch((err) => {
    console.error(err);
    process.exit(1);
  });
}
