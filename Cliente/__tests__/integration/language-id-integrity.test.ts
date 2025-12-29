/**
 * Tests de integridad para el manejo de languageId en formularios
 * 
 * NOTA: Este test ha sido deshabilitado porque el sistema de rutas por idioma se ha eliminado.
 * El sistema ahora usa un middleware que redirige las rutas con locale a rutas sin locale.
 * 
 * Estos tests verifican que:
 * 1. Cuando se selecciona un idioma específico, se envía el Guid correcto
 * 2. Cuando se selecciona "Por defecto", se envía undefined/null
 * 3. El backend acepta y persiste correctamente ambos casos
 * 
 * Requiere:
 * - API ejecutándose en http://localhost:5000
 * - Credenciales de prueba: empresa "Empresa Demo", usuario "admin", contraseña "admin123"
 */

// DESHABILITADO: El sistema de rutas por idioma se ha eliminado
describe.skip("Integridad: Manejo de languageId en formularios", () => {
  it("Tests deshabilitados - sistema de rutas por idioma eliminado", () => {
    expect(true).toBe(true);
  });
});
