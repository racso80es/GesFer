"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { signIn } from "next-auth/react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { ErrorMessage } from "@/components/ui/error-message";
import { User, Lock, Loader2, Shield } from "lucide-react";

export default function AdminLoginPage() {
  const router = useRouter();
  const [formData, setFormData] = useState({
    usuario: "admin",
    contraseña: "admin123",
  });
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsLoading(true);

    try {
      // Usar el provider "admin" para autenticación administrativa
      const result = await signIn("admin", {
        usuario: formData.usuario,
        contraseña: formData.contraseña,
        redirect: false,
      });

      if (result?.error) {
        setError("Credenciales administrativas inválidas");
        setIsLoading(false);
        return;
      }

      if (result?.ok) {
        // Redirigir al dashboard administrativo
        router.push("/admin/dashboard");
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
              <Label htmlFor="usuario">Usuario Administrativo</Label>
              <div className="relative">
                <User className="absolute left-3 top-3 h-4 w-4 text-gray-400" />
                <Input
                  id="usuario"
                  type="text"
                  placeholder="admin"
                  value={formData.usuario}
                  onChange={(e) => setFormData({ ...formData, usuario: e.target.value })}
                  className="pl-10"
                  required
                  disabled={isLoading}
                />
              </div>
            </div>
            <div className="space-y-2">
              <Label htmlFor="contraseña">Contraseña</Label>
              <div className="relative">
                <Lock className="absolute left-3 top-3 h-4 w-4 text-gray-400" />
                <Input
                  id="contraseña"
                  type="password"
                  placeholder="••••••••"
                  value={formData.contraseña}
                  onChange={(e) => setFormData({ ...formData, contraseña: e.target.value })}
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
