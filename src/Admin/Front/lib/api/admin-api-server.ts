/**
 * Cliente API Admin para uso en Server Components.
 * Requiere el token JWT de la sesión para autorizar las peticiones.
 * En desarrollo, si la URL es HTTPS a localhost (ej. https://localhost:5011), se usa
 * HTTP en 5010 para evitar que Node rechace el certificado autofirmado (fetch failed).
 */
function normalizeAdminApiBaseUrl(url: string): string {
  const trimmed = url.replace(/\/+$/, "");
  return trimmed.endsWith("/api") ? trimmed : `${trimmed}/api`;
}

/** En servidor Node, HTTPS a localhost falla por certificado autofirmado; usar HTTP 5010. */
function serverSafeAdminApiUrl(url: string): string {
  try {
    const u = new URL(url);
    if (u.protocol === "https:" && (u.hostname === "localhost" || u.hostname === "127.0.0.1")) {
      return normalizeAdminApiBaseUrl(`http://${u.hostname}:5010`);
    }
  } catch {
    /* ignore */
  }
  return normalizeAdminApiBaseUrl(url);
}

export function getAdminApiWithToken(accessToken: string | undefined) {
  const configured =
    process.env.NEXT_PUBLIC_ADMIN_API_URL ||
    process.env.ADMIN_API_URL ||
    "http://localhost:5010";
  const baseUrl = serverSafeAdminApiUrl(configured);

  const headers: Record<string, string> = {
    "Content-Type": "application/json",
  };
  if (accessToken) {
    headers["Authorization"] = `Bearer ${accessToken}`;
  }

  const request = async <T>(
    path: string,
    options: RequestInit = {}
  ): Promise<T> => {
    const url = `${baseUrl}${path}`;
    let response: Response;
    try {
      response = await fetch(url, {
        ...options,
        headers: { ...headers, ...(options.headers as Record<string, string>) },
        // En servidor Next.js, evitar cache para siempre usar token actual
        cache: "no-store",
      });
    } catch (err) {
      const msg = err instanceof Error ? err.message : String(err);
      throw new Error(`No se pudo conectar con la API Admin (${baseUrl}): ${msg}`);
    }

    if (!response.ok) {
      const status = response.status;
      let body = "";
      try {
        body = await response.text();
      } catch {
        body = response.statusText;
      }
      throw new Error(
        `API Admin error ${status}: ${response.statusText}${body ? ` — ${body.slice(0, 200)}` : ""}`
      );
    }

    if (response.status === 204) {
      return {} as T;
    }

    return response.json();
  };

  return {
    get: <T>(path: string) => request<T>(path, { method: "GET" }),
    post: <T>(path: string, body: unknown) =>
      request<T>(path, { method: "POST", body: JSON.stringify(body) }),
    put: <T>(path: string, body: unknown) =>
      request<T>(path, { method: "PUT", body: JSON.stringify(body) }),
    delete: (path: string) => request(path, { method: "DELETE" }),
  };
}
