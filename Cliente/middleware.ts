import createMiddleware from 'next-intl/middleware';
import { NextRequest, NextResponse } from 'next/server';
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

function getLocaleFromUser(request: NextRequest): Locale | null {
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
      // Si hay error parseando, continuar con el flujo normal
      console.error('Error parsing user data:', error);
    }
  }
  
  return null;
}

const intlMiddleware = createMiddleware({
  locales,
  defaultLocale,
  localePrefix: 'as-needed', // No mostrar /es en la URL si es el idioma por defecto
  localeDetection: true, // Detectar idioma del navegador
});

export default function middleware(request: NextRequest) {
  const pathname = request.nextUrl.pathname;
  
  // Evitar procesar rutas de API y recursos estáticos
  if (pathname.startsWith('/api/') || pathname.startsWith('/_next/')) {
    return intlMiddleware(request);
  }
  
  // Excluir rutas críticas de las redirecciones de locale para evitar bucles
  // Estas rutas se manejarán por el middleware de next-intl sin redirecciones adicionales
  const criticalRoutes = ['/login', '/dashboard', '/usuarios', '/clientes', '/empresas'];
  if (criticalRoutes.some(route => pathname.includes(route))) {
    return intlMiddleware(request);
  }
  
  // Intentar obtener el locale del usuario
  const userLocale = getLocaleFromUser(request);
  
  // Si tenemos un locale del usuario, usarlo como preferencia
  if (userLocale) {
    const pathnameHasLocale = locales.some(
      (locale) => pathname.startsWith(`/${locale}/`) || pathname === `/${locale}`
    );
    
    // Si el pathname no tiene locale, redirigir al locale del usuario
    if (!pathnameHasLocale) {
      const newPath = userLocale === defaultLocale 
        ? pathname 
        : `/${userLocale}${pathname === '/' ? '' : pathname}`;
      
      // Si el path ya es correcto, continuar con el middleware de next-intl
      if (newPath === pathname) {
        return intlMiddleware(request);
      }
      
      // Solo redirigir si la nueva ruta es diferente a la actual
      const newUrl = new URL(newPath, request.url);
      if (newUrl.pathname !== pathname && newUrl.pathname !== request.nextUrl.pathname) {
        return NextResponse.redirect(newUrl);
      }
    } else {
      // Si el pathname tiene un locale diferente al del usuario, cambiarlo
      const currentLocale = pathname.split('/')[1];
      if (locales.includes(currentLocale as Locale) && currentLocale !== userLocale) {
        const pathWithoutLocale = pathname.replace(`/${currentLocale}`, '') || '/';
        const newPath = userLocale === defaultLocale 
          ? pathWithoutLocale 
          : `/${userLocale}${pathWithoutLocale}`;
        
        // Solo redirigir si la nueva ruta es diferente a la actual
        const newUrl = new URL(newPath, request.url);
        if (newUrl.pathname !== pathname && newUrl.pathname !== request.nextUrl.pathname) {
          return NextResponse.redirect(newUrl);
        }
      }
    }
  }
  
  // Usar el middleware de next-intl que maneja automáticamente la redirección
  return intlMiddleware(request);
}

export const config = {
  // Matcher para rutas que necesitan internacionalización
  matcher: ['/((?!api|_next|_vercel|.*\\..*).*)']
};
