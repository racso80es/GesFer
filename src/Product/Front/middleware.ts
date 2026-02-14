import { NextRequest, NextResponse } from 'next/server';
import { auth } from './auth';
import { defaultLocale, locales, type Locale } from './i18n';

// Mapeo de languageId (Guids) a locale
// Estos son los IDs fijos de los idiomas según seed-data.sql
const languageIdToLocale: Record<string, Locale> = {
  '10000000-0000-0000-0000-000000000001': 'es', // Español
  '10000000-0000-0000-0000-000000000002': 'en', // English
  '10000000-0000-0000-0000-000000000003': 'ca', // Català
  // También soportar códigos directos por compatibilidad
  'es': 'es',
  'en': 'en',
  'ca': 'ca',
};

function getLocaleFromUser(request: NextRequest): Locale {
  // Intentar obtener el idioma del usuario desde cookies
  const userData = request.cookies.get('auth_user');
  
  if (userData?.value) {
    try {
      const user = JSON.parse(decodeURIComponent(userData.value));
      
      // Prioridad: userLanguageId > companyLanguageId > countryLanguageId > effectiveLanguageId
      const languageId = 
        user.userLanguageId || 
        user.companyLanguageId || 
        user.countryLanguageId || 
        user.effectiveLanguageId;
      
      if (languageId) {
        // Si el languageId es directamente un código de idioma válido
        if (locales.includes(languageId as Locale)) {
          return languageId as Locale;
        }
        
        // Si es un Guid u otro formato, usar el mapeo
        const locale = languageIdToLocale[languageId];
        if (locale && locales.includes(locale)) {
          return locale;
        }
      }
    } catch (error) {
      // Si hay error parseando, continuar con el default
      console.error('Error parsing user data:', error);
    }
  }
  
  return defaultLocale;
}

// Rutas públicas que no requieren autenticación
const publicRoutes = ['/login', '/api/auth'];

// Rutas que requieren autenticación
const protectedRoutes = ['/dashboard', '/usuarios', '/clientes', '/companies', '/maestros'];

/**
 * Normaliza la URL base corrigiendo localhost.com a localhost
 * Esto previene errores DNS cuando el navegador envía un hostname incorrecto
 */
function getNormalizedBaseUrl(request: NextRequest): string {
  const url = new URL(request.url);
  
  // Corregir localhost.com a localhost
  if (url.hostname === 'localhost.com' || url.hostname === 'www.localhost.com') {
    url.hostname = 'localhost';
  }
  
  return url.origin;
}

export default async function middleware(request: NextRequest) {
  const pathname = request.nextUrl.pathname;
  const baseUrl = getNormalizedBaseUrl(request);
  
  // Eliminar cualquier locale de la URL si existe (redirigir a la ruta sin locale)
  const pathnameParts = pathname.split('/').filter(Boolean);
  const firstPart = pathnameParts[0];
  
  // Si el primer segmento es un locale válido, redirigir a la ruta sin locale
  if (firstPart && locales.includes(firstPart as Locale)) {
    const pathWithoutLocale = '/' + pathnameParts.slice(1).join('/') || '/';
    return NextResponse.redirect(new URL(pathWithoutLocale, baseUrl));
  }
  
  // Verificar si la ruta es pública
  const isPublicRoute = publicRoutes.some(route => pathname.startsWith(route));
  
  // Verificar si la ruta está protegida
  const isProtectedRoute = protectedRoutes.some(route => pathname.startsWith(route));
  
  // Función helper para verificar autenticación (NextAuth o cookies)
  const isAuthenticated = async (): Promise<boolean> => {
    // Primero verificar NextAuth (SOLO dominio Cliente)
    const session = await auth();
    // Invariante de dominio:
    // - Una sesión Admin (role: "Admin") NO autentica rutas del dominio Cliente.
    // - Solo sesiones no-admin pueden autenticar el dominio Cliente.
    if (session?.user?.role && session.user.role !== "Admin") {
      return true;
    }
    if (session) {
      return false;
    }
    
    // Fallback: verificar cookie auth_user (para compatibilidad con AuthContext)
    const authUserCookie = request.cookies.get('auth_user');
    if (authUserCookie?.value) {
      try {
        const user = JSON.parse(decodeURIComponent(authUserCookie.value));
        return !!user && !!user.userId;
      } catch {
        return false;
      }
    }
    
    return false;
  };

  // Si es una ruta protegida, verificar autenticación
  if (isProtectedRoute) {
    const authenticated = await isAuthenticated();
    
    if (!authenticated) {
      // Redirigir a login si no está autenticado
      const loginUrl = new URL('/login', baseUrl);
      loginUrl.searchParams.set('callbackUrl', pathname);
      return NextResponse.redirect(loginUrl);
    }
  }
  
  // Si está autenticado y trata de acceder a login, redirigir a dashboard
  if (pathname === '/login') {
    const authenticated = await isAuthenticated();
    if (authenticated) {
      return NextResponse.redirect(new URL('/dashboard', baseUrl));
    }
  }

  // Si la ruta no es una API, reescribir internamente para incluir el locale
  // Esto permite que Next.js resuelva correctamente la ruta bajo la estructura app/[locale]/...
  if (!pathname.startsWith('/api/')) {
    const locale = getLocaleFromUser(request);
    // Usar rewrite en lugar de redirect para mantener la URL limpia en el navegador
    return NextResponse.rewrite(new URL(`/${locale}${pathname}${request.nextUrl.search}`, request.url));
  }
  
  // Continuar con la request normalmente
  return NextResponse.next();
}

export const config = {
  // Matcher para todas las rutas excepto archivos estáticos, API de NextAuth y archivos estáticos
  matcher: [
    /*
     * Match all request paths except for the ones starting with:
     * - api/auth (NextAuth routes)
     * - _next/static (static files)
     * - _next/image (image optimization files)
     * - favicon.ico (favicon file)
     * - public files (images, etc.)
     */
    '/((?!api/auth|_next/static|_next/image|favicon.ico|.*\\..*).*)',
  ],
};
