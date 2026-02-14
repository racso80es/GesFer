import { redirect } from "next/navigation";
import { headers } from "next/headers";
import { auth } from "@/auth";

export interface LogEntry {
  id: number;
  level: string;
  message: string;
  exception?: string | null;
  timeStamp: string;
  source?: string | null;
  companyId?: string | null;
  userId?: string | null;
}

export interface LogsPagedResponse {
  logs: LogEntry[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export default async function LogsPage() {
  const session = await auth();
  if (!session?.user || session.user.role !== "Admin") {
    redirect("/login");
  }

  let data: LogsPagedResponse | null = null;
  let loadError: string | null = null;

  try {
    const baseUrl =
      process.env.NEXTAUTH_URL ??
      (process.env.VERCEL_URL ? `https://${process.env.VERCEL_URL}` : "http://localhost:3001");
    const cookie = (await headers()).get("cookie") ?? "";
    const res = await fetch(`${baseUrl}/api/admin/logs?pageNumber=1&pageSize=100`, {
      cache: "no-store",
      headers: { cookie },
    });
    if (!res.ok) {
      let errBody = await res.text();
      let detail = "";
      try {
        const j = JSON.parse(errBody);
        detail = j.detail ? ` — ${j.detail}` : "";
      } catch {
        if (errBody) detail = ` — ${errBody.slice(0, 200)}`;
      }
      console.error("GET /api/admin/logs failed:", res.status, errBody);
      loadError =
        res.status === 401
          ? "Sesión no válida o expirada. Cierra sesión e inicia de nuevo."
          : `Error al cargar logs (${res.status})${detail}. Comprueba que la API Admin esté en ejecución en el puerto 5010.`;
    } else {
      data = await res.json();
    }
  } catch (error) {
    console.error("Error fetching logs:", error);
    loadError =
      "No se pudo conectar con el servidor. Comprueba que la API Admin esté en ejecución (puerto 5010) y vuelve a iniciar sesión si es necesario.";
  }

  const logs = data?.logs ?? [];
  const totalCount = data?.totalCount ?? 0;

  return (
    <div className="container mx-auto py-10">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold">Logs del sistema</h1>
        <p className="text-sm text-muted-foreground">
          Total: {totalCount} registros
        </p>
      </div>

      {loadError && (
        <div className="mb-4 rounded-md bg-red-50 border border-red-200 p-4 text-red-800 text-sm">
          {loadError}
        </div>
      )}

      <div className="bg-white rounded-md border shadow overflow-x-auto">
        <table className="w-full min-w-[800px]">
          <thead>
            <tr className="border-b bg-gray-50">
              <th className="text-left py-3 px-4 font-medium text-gray-500 w-24">
                Fecha
              </th>
              <th className="text-left py-3 px-4 font-medium text-gray-500 w-20">
                Nivel
              </th>
              <th className="text-left py-3 px-4 font-medium text-gray-500">
                Mensaje
              </th>
              <th className="text-left py-3 px-4 font-medium text-gray-500 w-40">
                Origen
              </th>
            </tr>
          </thead>
          <tbody>
            {logs.length === 0 ? (
              <tr>
                <td
                  colSpan={4}
                  className="text-center py-8 text-gray-500"
                >
                  {loadError ? "No se pudieron cargar los logs." : "No hay logs registrados."}
                </td>
              </tr>
            ) : (
              logs.map((log) => (
                <tr
                  key={log.id}
                  className="border-b last:border-0 hover:bg-gray-50"
                >
                  <td className="py-3 px-4 text-sm text-gray-600 whitespace-nowrap">
                    {new Date(log.timeStamp).toLocaleString()}
                  </td>
                  <td className="py-3 px-4">
                    <span
                      className={`px-2 py-1 rounded text-xs font-medium ${
                        log.level === "Error"
                          ? "bg-red-100 text-red-800"
                          : log.level === "Warning"
                            ? "bg-amber-100 text-amber-800"
                            : "bg-gray-100 text-gray-800"
                      }`}
                    >
                      {log.level}
                    </span>
                  </td>
                  <td className="py-3 px-4 text-sm">
                    <span className="line-clamp-2" title={log.message}>
                      {log.message}
                    </span>
                    {log.exception && (
                      <details className="mt-1">
                        <summary className="text-xs text-red-600 cursor-pointer">
                          Excepción
                        </summary>
                        <pre className="text-xs mt-1 p-2 bg-gray-100 rounded overflow-x-auto max-w-2xl">
                          {log.exception}
                        </pre>
                      </details>
                    )}
                  </td>
                  <td className="py-3 px-4 text-sm text-gray-500">
                    {log.source || "-"}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
