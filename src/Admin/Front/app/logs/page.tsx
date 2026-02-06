"use client";

import { useEffect, useState } from "react";
import { useSession } from "next-auth/react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@shared/components/ui/card";
import { Loading } from "@shared/components/ui/loading";
import { ErrorMessage } from "@shared/components/ui/error-message";
import { Button } from "@shared/components/ui/button";
import { FileText, Trash2 } from "lucide-react";

interface Log {
  id: number;
  level: string;
  message: string;
  exception?: string;
  timeStamp: string;
  source?: string;
  companyId?: string;
  userId?: string;
}

interface LogsResponse {
  logs: Log[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export default function LogsPage() {
  const { data: session } = useSession();
  const [logs, setLogs] = useState<Log[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);

  useEffect(() => {
    const fetchLogs = async () => {
      try {
        setIsLoading(true);
        setError(null);

        const apiUrl = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5001";
        const token = session?.accessToken;

        if (!token) {
          setError("Token de autenticación no disponible");
          setIsLoading(false);
          return;
        }

        const response = await fetch(
          `${apiUrl}/api/admin/logs?pageNumber=${pageNumber}&pageSize=50`,
          {
            method: "GET",
            headers: {
              "Authorization": `Bearer ${token}`,
              "Content-Type": "application/json",
            },
          }
        );

        if (!response.ok) {
          if (response.status === 401 || response.status === 403) {
            setError("No tienes permisos para acceder a los logs");
          } else {
            setError(`Error al cargar los logs: ${response.statusText}`);
          }
          setIsLoading(false);
          return;
        }

        const data: LogsResponse = await response.json();
        setLogs(data.logs);
        setTotalPages(data.totalPages);
      } catch (err) {
        console.error("Error al cargar los logs:", err);
        setError("Error al conectar con el servidor");
      } finally {
        setIsLoading(false);
      }
    };

    if (session?.user && session.user.role === "Admin") {
      fetchLogs();
    }
  }, [session, pageNumber]);

  const getLevelColor = (level: string) => {
    switch (level.toLowerCase()) {
      case "error":
      case "fatal":
        return "text-red-600 dark:text-red-400";
      case "warning":
        return "text-yellow-600 dark:text-yellow-400";
      case "information":
      case "info":
        return "text-blue-600 dark:text-blue-400";
      case "debug":
        return "text-gray-600 dark:text-gray-400";
      default:
        return "text-gray-600 dark:text-gray-400";
    }
  };

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <Loading />
      </div>
    );
  }

  return (
    <div className="container mx-auto p-6">
      <div className="mb-8 flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Logs del Sistema</h1>
          <p className="text-muted-foreground">
            Visualización y gestión de logs del sistema
          </p>
        </div>
      </div>

      {error && (
        <div className="mb-6">
          <ErrorMessage message={error} />
        </div>
      )}

      <Card>
        <CardHeader>
          <CardTitle>Registros de Logs</CardTitle>
          <CardDescription>
            Página {pageNumber} de {totalPages} ({logs.length} registros mostrados)
          </CardDescription>
        </CardHeader>
        <CardContent>
          {logs.length === 0 ? (
            <div className="text-center py-8 text-muted-foreground">
              <FileText className="mx-auto h-12 w-12 mb-4 opacity-50" />
              <p>No hay logs disponibles</p>
            </div>
          ) : (
            <div className="space-y-4">
              {logs.map((log) => (
                <div
                  key={log.id}
                  className="border rounded-lg p-4 hover:bg-accent/50 transition-colors"
                >
                  <div className="flex items-start justify-between">
                    <div className="flex-1">
                      <div className="flex items-center gap-2 mb-2">
                        <span className={`font-semibold ${getLevelColor(log.level)}`}>
                          {log.level}
                        </span>
                        <span className="text-xs text-muted-foreground">
                          {new Date(log.timeStamp).toLocaleString("es-ES")}
                        </span>
                      </div>
                      <p className="text-sm mb-1">{log.message}</p>
                      {log.source && (
                        <p className="text-xs text-muted-foreground">Fuente: {log.source}</p>
                      )}
                      {log.exception && (
                        <pre className="mt-2 text-xs bg-destructive/10 p-2 rounded overflow-auto">
                          {log.exception}
                        </pre>
                      )}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}

          {totalPages > 1 && (
            <div className="mt-6 flex items-center justify-between">
              <Button
                variant="outline"
                onClick={() => setPageNumber((p) => Math.max(1, p - 1))}
                disabled={pageNumber === 1}
              >
                Anterior
              </Button>
              <span className="text-sm text-muted-foreground">
                Página {pageNumber} de {totalPages}
              </span>
              <Button
                variant="outline"
                onClick={() => setPageNumber((p) => Math.min(totalPages, p + 1))}
                disabled={pageNumber === totalPages}
              >
                Siguiente
              </Button>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
