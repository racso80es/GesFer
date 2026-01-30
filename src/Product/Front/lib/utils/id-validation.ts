/**
 * Utilidades para validar IDs (GUIDs)
 */

/**
 * Valida si un string es un GUID válido
 * @param id - El ID a validar
 * @returns true si es un GUID válido, false en caso contrario
 */
export function isValidGuid(id: string | null | undefined): boolean {
  if (!id || typeof id !== "string") {
    return false;
  }

  // Regex para validar formato GUID: 8-4-4-4-12 caracteres hexadecimales
  const guidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
  return guidRegex.test(id.trim());
}

/**
 * Sanitiza un ID eliminando caracteres inválidos y validando formato
 * @param id - El ID a sanitizar
 * @returns El ID sanitizado o null si no es válido
 */
export function sanitizeId(id: string | null | undefined): string | null {
  if (!id || typeof id !== "string") {
    return null;
  }

  // Eliminar espacios y caracteres especiales que no deberían estar en un GUID
  const cleaned = id.trim().replace(/[^0-9a-f-]/gi, "");

  // Verificar que después de limpiar sigue siendo un GUID válido
  if (isValidGuid(cleaned)) {
    return cleaned;
  }

  return null;
}

/**
 * Valida y sanitiza un ID, lanzando un error si no es válido
 * @param id - El ID a validar
 * @param entityName - Nombre de la entidad (para mensaje de error)
 * @returns El ID validado y sanitizado
 * @throws Error si el ID no es válido
 */
export function validateId(
  id: string | null | undefined,
  entityName: string = "entidad"
): string {
  if (!id) {
    throw new Error(`El ID de ${entityName} es requerido`);
  }

  const sanitized = sanitizeId(id);
  if (!sanitized) {
    throw new Error(
      `El ID de ${entityName} no es válido. Debe ser un GUID en formato: 00000000-0000-0000-0000-000000000000`
    );
  }

  return sanitized;
}

