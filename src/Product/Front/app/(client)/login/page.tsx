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

// Credenciales por defecto: desde env (coinciden con seeds demo-data.json) o fallback para desarrollo
function getDefaultLoginCredentials(): { company: string; username: string; password: string } {
  return {
    company: process.env.NEXT_PUBLIC_DEFAULT_LOGIN_COMPANY ?? "Organización Cliente",
    username: process.env.NEXT_PUBLIC_DEFAULT_LOGIN_USER ?? "user_test",
    password: process.env.NEXT_PUBLIC_DEFAULT_LOGIN_PASSWORD ?? "admin123",
  };
}

export default function LoginPage() {
  const router = useRouter();
  const { login, isAuthenticated, isLoading: authLoading } = useAuth();
  const t = useTranslations('auth');
  const [formData, setFormData] = useState<{ company: string; username: string; password: string }>(() => getDefaultLoginCredentials());
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  // Redirigir si ya está autenticado al cargar la página o después del login
  useEffect(() => {
    if (!authLoading && isAuthenticated && !isLoading) {
      const currentPath = typeof window !== 'undefined' ? window.location.pathname : '';
      // Si estamos en /login y ya estamos autenticados, redirigir a dashboard o callbackUrl
      if (currentPath.includes('login')) {
        const params = typeof window !== 'undefined' ? new URLSearchParams(window.location.search) : null;
        const callbackUrl = params?.get('callbackUrl');
        const target = callbackUrl && callbackUrl.startsWith('/') && !callbackUrl.startsWith('//') ? callbackUrl : '/dashboard';
        router.replace(target);
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

  // Si está autenticado y estamos en /login, redirigir a dashboard o callbackUrl
  if (isAuthenticated && !isLoading) {
    const currentPath = typeof window !== 'undefined' ? window.location.pathname : '';
    if (currentPath.includes('login')) {
      const params = typeof window !== 'undefined' ? new URLSearchParams(window.location.search) : null;
      const callbackUrl = params?.get('callbackUrl');
      const target = callbackUrl && callbackUrl.startsWith('/') && !callbackUrl.startsWith('//') ? callbackUrl : '/dashboard';
      router.replace(target);
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
              <Label htmlFor="company">{t('company')}</Label>
              <div className="relative">
                <Building2 className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                <Input
                  id="company"
                  type="text"
                  placeholder={t('company')}
                  value={formData.company}
                  onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                    setFormData({ ...formData, company: e.target.value })
                  }
                  className="pl-10"
                  required
                  data-testid="shared-input-text-company"
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="username">{t('username')}</Label>
              <div className="relative">
                <User className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                <Input
                  id="username"
                  type="text"
                  placeholder={t('username')}
                  value={formData.username}
                  onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                    setFormData({ ...formData, username: e.target.value })
                  }
                  className="pl-10"
                  required
                  data-testid="shared-input-text-username"
                />
              </div>
            </div>

            <div className="space-y-2">
              <Label htmlFor="password">{t('password')}</Label>
              <div className="relative">
                <Lock className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                <Input
                  id="password"
                  type="password"
                  placeholder={t('password')}
                  value={formData.password}
                  onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
                    setFormData({ ...formData, password: e.target.value })
                  }
                  className="pl-10"
                  required
                  data-testid="shared-input-password-password"
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

