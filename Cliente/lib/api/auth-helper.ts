import { auth } from "@/auth";

/**
 * Obtiene el access token de la sesión actual (Server Components)
 * @returns El access token JWT o null si no hay sesión
 */
export async function getAccessToken(): Promise<string | null> {
  const session = await auth();
  return (session as any)?.accessToken || null;
}

/**
 * Obtiene el cursor ID de la sesión actual (Server Components)
 * @returns El cursor ID o null si no hay sesión
 */
export async function getCursorId(): Promise<string | null> {
  const session = await auth();
  return session?.user?.cursorId || null;
}

