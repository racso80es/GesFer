import { defaultLocale, locales, type Locale } from '@/i18n';
import type { LoginResponse } from '@/lib/types/api';

/**
 * Obtiene el locale basado en la información del usuario
 * Prioridad: userLanguageId > companyLanguageId > countryLanguageId > effectiveLanguageId > defaultLocale
 */
export function getLocaleFromUser(user: LoginResponse | null): Locale {
  if (!user) {
    return defaultLocale;
  }

  // Mapeo de languageId a locale
  // Si los languageId son códigos como 'es', 'en', 'ca', los usamos directamente
  // Si son IDs numéricos, necesitarías un mapeo desde la API
  const languageIdToLocale: Record<string, Locale> = {
    'es': 'es',
    'en': 'en',
    'ca': 'ca',
    // Agregar más mapeos si los languageId son diferentes
  };

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
    
    // Si es un ID numérico u otro formato, usar el mapeo
    const locale = languageIdToLocale[languageId];
    if (locale && locales.includes(locale)) {
      return locale;
    }
  }

  // Si no hay idioma configurado, usar español por defecto
  return defaultLocale;
}





