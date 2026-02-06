"use client";

import { useSession as useNextAuthSession } from "next-auth/react";

/**
 * Hook personalizado para obtener la sesión en Client Components
 * Extiende la sesión de NextAuth con el accessToken
 */
export function useSession() {
  const { data: session, status, update } = useNextAuthSession();
  
  return {
    data: session,
    status,
    update,
    accessToken: session?.accessToken,
    cursorId: session?.user?.cursorId,
  };
}

