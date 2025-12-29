"use client";

import { useMemo } from "react";
import { useAuth } from "@/contexts/auth-context";
import { locales, defaultLocale, type Locale } from "@/i18n";

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

/**
 * Hook para obtener el locale del usuario
 * Prioridad: userLanguageId > companyLanguageId > countryLanguageId > effectiveLanguageId > defaultLocale
 */
export function useLocale(): Locale {
  const { user } = useAuth();

  return useMemo(() => {
    if (!user) {
      return defaultLocale;
    }

    // Prioridad: userLanguageId > companyLanguageId > countryLanguageId > effectiveLanguageId
    const languageId = 
      user.userLanguageId || 
      user.companyLanguageId || 
      user.countryLanguageId || 
      user.effectiveLanguageId;

    if (!languageId) {
      return defaultLocale;
    }

    // Si el languageId es directamente un código de idioma válido
    if (locales.includes(languageId as Locale)) {
      return languageId as Locale;
    }

    // Si es un Guid u otro formato, usar el mapeo
    const locale = languageIdToLocale[languageId];
    if (locale && locales.includes(locale)) {
      return locale;
    }

    return defaultLocale;
  }, [user]);
}

