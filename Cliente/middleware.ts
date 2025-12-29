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

export default function middleware(request: NextRequest) {
  const pathname = request.nextUrl.pathname;
  
  // Eliminar cualquier locale de la URL si existe (redirigir a la ruta sin locale)
  const pathnameParts = pathname.split('/').filter(Boolean);
  const firstPart = pathnameParts[0];
  
  // Si el primer segmento es un locale válido, redirigir a la ruta sin locale
  if (firstPart && locales.includes(firstPart as Locale)) {
    const pathWithoutLocale = '/' + pathnameParts.slice(1).join('/') || '/';
    return NextResponse.redirect(new URL(pathWithoutLocale, request.url));
  }
  
  // Continuar con la request normalmente (sin redirecciones de locale)
  return NextResponse.next();
}

export const config = {
  // Matcher para todas las rutas excepto archivos estáticos y API
  matcher: ['/((?!api|_next|_vercel|.*\\..*).*)']
};
