"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/contexts/auth-context";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { ErrorMessage } from "@/components/ui/error-message";
import { Building2, User, Lock, Loader2 } from "lucide-react";
import { useTranslations } from 'next-intl';

export default function LoginPage() {
  const router = useRouter();
  const { login, isAuthenticated, isLoading: authLoading } = useAuth();
  const t = useTranslations('auth');
  const [formData, setFormData] = useState({
    empresa: "Empresa Demo",
    usuario: "admin",
    contraseña: "admin123",
  });
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  // Redirigir si ya está autenticado al cargar la página (solo una vez, no durante el login)
  useEffect(() => {
    // Solo redirigir si:
    // 1. Ya terminó de cargar el estado de autenticación inicial
    // 2. Está autenticado
    // 3. NO estamos en proceso de hacer login (isLoading es false)
    // 4. NO estamos ya en dashboard (para evitar bucles)
    if (!authLoading && isAuthenticated && !isLoading) {
      const currentPath = typeof window !== 'undefined' ? window.location.pathname : '';
      if (!currentPath.includes('dashboard')) {
        // Verificar también localStorage para asegurarnos
        const storedUser = typeof window !== 'undefined' ? localStorage.getItem('auth_user') : null;
        if (storedUser) {
          router.replace("/dashboard");
        }
      }
    }
  }, [authLoading]); // Solo ejecutar cuando cambie authLoading (carga inicial)

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

  // No mostrar nada si está autenticado (se redirigirá)
  if (isAuthenticated) {
    return null;
  }

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError(null);
    setIsLoading(true);

    try {
      await login(formData);
      // Esperar un momento para que el estado se actualice y localStorage se guarde
      await new Promise(resolve => setTimeout(resolve, 100));
      // Redirigir después del login exitoso
      router.replace("/dashboard");
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
                  data-testid="login-empresa-input"
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
                  data-testid="login-usuario-input"
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
                  data-testid="login-password-input"
                />
              </div>
            </div>

            {error && <ErrorMessage message={error} data-testid="login-error-message" />}

            <Button
              type="submit"
              className="w-full"
              disabled={isLoading}
              data-testid="login-submit-button"
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


