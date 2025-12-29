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
    const cookieStore = cookies();
    const userData = cookieStore.get('auth_user');
    
    if (userData?.value) {
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
    }
  } catch (error) {
    // Si hay error parseando, continuar con el default
    console.error('Error parsing user data:', error);
  }
  
  return defaultLocale;
}

export default getRequestConfig(async () => {
  // Obtener el locale del usuario desde cookies
  const locale = getLocaleFromUser();

  return {
    locale: locale as Locale,
    messages: (await import(`./messages/${locale}.json`)).default
  };
});
