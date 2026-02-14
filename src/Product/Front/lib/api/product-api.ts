export const getProductApi = () => {
  const baseUrl = process.env.NEXT_PUBLIC_PRODUCT_API_URL || "http://localhost:5000/api";

  const request = async <T>(path: string, options: RequestInit = {}): Promise<T> => {
    const url = `${baseUrl}${path}`;
    const headers = {
      "Content-Type": "application/json",
      ...options.headers,
    };

    const response = await fetch(url, { ...options, headers });

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
    post: <T>(path: string, body: unknown) => request<T>(path, { method: "POST", body: JSON.stringify(body) }),
    put: <T>(path: string, body: unknown) => request<T>(path, { method: "PUT", body: JSON.stringify(body) }),
    delete: (path: string) => request(path, { method: "DELETE" }),
  };
};
