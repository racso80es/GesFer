import { getRequestConfig } from 'next-intl/server';
import { cookies } from 'next/headers';

// Idiomas soportados
export const locales = ['es', 'en', 'ca'] as const;
export type Locale = (typeof locales)[number];

// Idioma por defecto
export const defaultLocale: Locale = 'es';

// Mapeo de languageId (Guids) a locale
const languageIdToLocale: Record<string, Locale> = {
  '10000000-0000-0000-0000-000000000001': 'es', // Español
  '10000000-0000-0000-0000-000000000002': 'en', // English
  '10000000-0000-0000-0000-000000000003': 'ca', // Català
  // También soportar códigos directos por compatibilidad
  'es': 'es',
  'en': 'en',
  'ca': 'ca',
};

function getLocaleFromUser(): Locale {
  try {
    // Intentar obtener cookies de forma segura
    let cookieStore;
    try {
      cookieStore = cookies();
    } catch (cookieError) {
      // Si no se pueden obtener cookies (por ejemplo, en build time), usar default
      return defaultLocale;
    }
    
    if (!cookieStore) {
      return defaultLocale;
    }
    
    const userData = cookieStore.get('auth_user');
    
    if (userData?.value) {
      try {
        const decoded = decodeURIComponent(userData.value);
        const user = JSON.parse(decoded);
        
        // Prioridad: userLanguageId > companyLanguageId > countryLanguageId > effectiveLanguageId
        const languageId = 
          user?.userLanguageId || 
          user?.companyLanguageId || 
          user?.countryLanguageId || 
          user?.effectiveLanguageId;
        
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
      } catch (parseError) {
        // Si hay error parseando el JSON, continuar con el default
        // No loguear en producción para evitar spam
        if (process.env.NODE_ENV === 'development') {
          console.warn('Error parsing user data from cookie:', parseError);
        }
      }
    }
  } catch (error) {
    // Si hay cualquier error, usar el default
    // No loguear en producción para evitar spam
    if (process.env.NODE_ENV === 'development') {
      console.warn('Error accessing cookies for locale:', error);
    }
  }
  
  return defaultLocale;
}

export default getRequestConfig(async () => {
  // Obtener el locale del usuario desde cookies
  let locale: Locale = defaultLocale;
  
  try {
    locale = getLocaleFromUser();
  } catch (error) {
    // Si hay cualquier error, usar el locale por defecto
    console.warn('Error obteniendo locale, usando default:', error);
    locale = defaultLocale;
  }

  // Cargar mensajes de forma segura
  let messages: any = {};
  try {
    messages = (await import(`./messages/${locale}.json`)).default;
  } catch (error) {
    // Si falla cargar mensajes del locale, intentar con el default
    console.warn(`Error cargando mensajes para ${locale}, usando default:`, error);
    try {
      messages = (await import(`./messages/${defaultLocale}.json`)).default;
    } catch {
      // Si incluso el default falla, usar objeto vacío
      messages = {};
    }
  }

  return {
    locale: locale as Locale,
    messages
  };
});
