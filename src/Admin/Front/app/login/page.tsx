"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { signIn } from "next-auth/react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@shared/components/ui/card";
import { Input } from "@shared/components/ui/input";
import { Label } from "@shared/components/ui/label";
import { Button } from "@shared/components/ui/button";
import { ErrorMessage } from "@shared/components/ui/error-message";
import { User, Lock, Loader2, Shield } from "lucide-react";

// Autofill local (dev): credenciales administrativas
const MOCK_ADMIN_CREDENTIALS = { username: "admin", password: "admin123" } as const;

export default function AdminLoginPage() {
  const router = useRouter();
  // Contexto ADMIN: Autocompletado para login administrativo
  const [formData, setFormData] = useState<{ username: string; password: string }>({ ...MOCK_ADMIN_CREDENTIALS });
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsLoading(true);

    try {
      // Usar el provider "admin" para autenticación administrativa
      const result = await signIn("admin", {
        username: formData.username,
        password: formData.password,
        redirect: false,
      });

      if (result?.error) {
        setError("Credenciales administrativas inválidas");
        setIsLoading(false);
        return;
      }

      if (result?.ok) {
        // Redirigir al dashboard administrativo
        router.push("/dashboard");
      }
    } catch (err) {
      console.error("Error en login administrativo:", err);
      setError("Error al iniciar sesión. Por favor, intenta de nuevo.");
      setIsLoading(false);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-gray-50 to-gray-100 dark:from-gray-900 dark:to-gray-800">
      <Card className="w-full max-w-md shadow-lg">
        <CardHeader className="space-y-1 text-center">
          <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-primary/10">
            <Shield className="h-8 w-8 text-primary" />
          </div>
          <CardTitle className="text-2xl font-bold">Acceso Administrativo</CardTitle>
          <CardDescription>
            Ingresa tus credenciales administrativas para acceder al panel de administración
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="username">Usuario Administrativo</Label>
              <div className="relative">
                <User className="absolute left-3 top-3 h-4 w-4 text-gray-400" />
                <Input
                  id="username"
                  type="text"
                  placeholder="admin"
                  value={formData.username}
                  onChange={(e: React.ChangeEvent<HTMLInputElement>) => setFormData({ ...formData, username: e.target.value })}
                  className="pl-10"
                  required
                  disabled={isLoading}
                />
              </div>
            </div>
            <div className="space-y-2">
              <Label htmlFor="password">Contraseña</Label>
              <div className="relative">
                <Lock className="absolute left-3 top-3 h-4 w-4 text-gray-400" />
                <Input
                  id="password"
                  type="password"
                  placeholder="••••••••"
                  value={formData.password}
                  onChange={(e: React.ChangeEvent<HTMLInputElement>) => setFormData({ ...formData, password: e.target.value })}
                  className="pl-10"
                  required
                  disabled={isLoading}
                />
              </div>
            </div>
            {error && <ErrorMessage message={error} />}
            <Button type="submit" className="w-full" disabled={isLoading}>
              {isLoading ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Iniciando sesión...
                </>
              ) : (
                <>
                  <Shield className="mr-2 h-4 w-4" />
                  Acceder al Panel Administrativo
                </>
              )}
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
