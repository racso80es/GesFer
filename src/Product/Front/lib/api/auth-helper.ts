import { auth } from "@/auth";

/**
 * Obtiene el access token de la sesión actual (Server Components)
 * @returns El access token JWT o null si no hay sesión
 */
export async function getAccessToken(): Promise<string | null> {
  const session = await auth();
  // Invariante de dominio: el token Admin no debe “autenticar” dominio Cliente por accidente.
  if (session?.user?.role === "Admin") return null;
  return session?.accessToken || null;
}

/**
 * Obtiene el cursor ID de la sesión actual (Server Components)
 * @returns El cursor ID o null si no hay sesión
 */
export async function getCursorId(): Promise<string | null> {
  const session = await auth();
  if (session?.user?.role === "Admin") return null;
  return session?.user?.cursorId || null;
}

