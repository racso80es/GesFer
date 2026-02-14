/**
 * Cliente API Admin para uso en Server Components.
 * Requiere el token JWT de la sesión para autorizar las peticiones.
 */
export function getAdminApiWithToken(accessToken: string | undefined) {
  const baseUrl =
    process.env.NEXT_PUBLIC_ADMIN_API_URL ||
    process.env.ADMIN_API_URL ||
    "http://localhost:5010/api";

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
    const response = await fetch(url, {
      ...options,
      headers: { ...headers, ...(options.headers as Record<string, string>) },
    });

    if (!response.ok) {
      throw new Error(`API error: ${response.statusText}`);
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
