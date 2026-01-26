"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/contexts/auth-context";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@shared/components/ui/card";
import { Input } from "@shared/components/shared/Input";
import { Label } from "@shared/components/ui/label";
import { Button } from "@shared/components/shared/Button";
import { ErrorMessage } from "@shared/components/ui/error-message";
import { Building2, User, Lock, Loader2 } from "lucide-react";
import { useTranslations } from 'next-intl';

// Constante definitiva para autofill de credenciales de cliente
// GUID de Empresa Cliente: 33333333-3333-3333-3333-333333333333
const MOCK_CLIENT_CREDENTIALS = {
  empresa: "Empresa Cliente",
  usuario: "user_test",
  contraseña: "user123",
} as const;

export default function LoginPage() {
  const router = useRouter();
  const { login, isAuthenticated, isLoading: authLoading } = useAuth();
  const t = useTranslations('auth');
  // Contexto CLIENTE: Autocompletado para login de cliente
  const [formData, setFormData] = useState(MOCK_CLIENT_CREDENTIALS);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  // Redirigir si ya está autenticado al cargar la página o después del login
  useEffect(() => {
    // Solo redirigir si:
    // 1. Ya terminó de cargar el estado de autenticación inicial
    // 2. Está autenticado
    // 3. NO estamos en proceso de hacer login (isLoading es false)
    // 4. NO estamos ya en la página de login (evitar bucles)
    if (!authLoading && isAuthenticated && !isLoading) {
      const currentPath = typeof window !== 'undefined' ? window.location.pathname : '';
      // Solo redirigir si no estamos en login y no estamos ya en dashboard
      if (!currentPath.includes('dashboard') && !currentPath.includes('login')) {
        // Usar push en lugar de replace para asegurar la navegación
        router.push("/dashboard");
      }
    }
  }, [authLoading, isAuthenticated, isLoading, router]);

  // Mostrar loading mientras se verifica la autenticación
  if (authLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900 mx-auto"></div>
          <p className="mt-4 text-muted-foreground">Cargando...</p>
        </div>
      </div>
    );
  }

  // Si está autenticado y no estamos en proceso de login, redirigir
  // Pero solo si realmente estamos en la página de login (no en una redirección)
  if (isAuthenticated && !isLoading) {
    const currentPath = typeof window !== 'undefined' ? window.location.pathname : '';
    // Solo mostrar "Redirigiendo" si estamos realmente en /login
    if (currentPath.includes('login')) {
      // Redirigir inmediatamente sin mostrar mensaje
      router.push("/dashboard");
      return (
        <div className="flex min-h-screen items-center justify-center">
          <div className="text-center">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900 mx-auto"></div>
            <p className="mt-4 text-muted-foreground">Redirigiendo...</p>
          </div>
        </div>
      );
    }
  }

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError(null);
    setIsLoading(true);

    try {
      await login(formData);
      // El login actualiza el estado, lo que activará el useEffect para redirigir
      // No necesitamos redirigir manualmente aquí, el useEffect lo hará
      setIsLoading(false);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : t('loginError')
      );
      setIsLoading(false);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-blue-50 to-indigo-100 p-4">
      <Card className="w-full max-w-md">
        <CardHeader className="space-y-1">
          <CardTitle className="text-2xl font-bold text-center">
            GesFer
          </CardTitle>
          <CardDescription className="text-center">
            {t('login')}
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-4" data-testid="login-form">
            <div className="space-y-2">
              <Label htmlFor="empresa">{t('company')}</Label>
              <div className="relative">
                <Building2 className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                <Input
                  id="empresa"
                  type="text"
                  placeholder={t('company')}
                  value={formData.empresa}
                  onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                    setFormData({ ...formData, empresa: e.target.value })
                  }
                  className="pl-10"
                  required
                  data-testid="shared-input-text-empresa"
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="usuario">{t('username')}</Label>
              <div className="relative">
                <User className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                <Input
                  id="usuario"
                  type="text"
                  placeholder={t('username')}
                  value={formData.usuario}
                  onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                    setFormData({ ...formData, usuario: e.target.value })
                  }
                  className="pl-10"
                  required
                  data-testid="shared-input-text-usuario"
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="contraseña">{t('password')}</Label>
              <div className="relative">
                <Lock className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                <Input
                  id="contraseña"
                  type="password"
                  placeholder={t('password')}
                  value={formData.contraseña}
                  onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                    setFormData({ ...formData, contraseña: e.target.value })
                  }
                  className="pl-10"
                  required
                  data-testid="shared-input-password-contraseña"
                />
              </div>
            </div>

            {error && <ErrorMessage message={error} data-testid="login-error-message" />}

            <Button
              type="submit"
              className="w-full"
              disabled={isLoading}
              data-testid="shared-button-login-submit"
            >
              {isLoading ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  {t('login')}...
                </>
              ) : (
                t('login')
              )}
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}

