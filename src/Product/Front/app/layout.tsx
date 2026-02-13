import type { Metadata } from "next";
import { NextIntlClientProvider } from 'next-intl';
import { getMessages, getLocale } from 'next-intl/server';
import type { AbstractIntlMessages } from 'next-intl';
import "./globals.css";
import { Toaster } from "sonner";
import { QueryProvider } from "@/lib/providers/query-provider";
import { SessionProvider } from "@/lib/providers/session-provider";
import { AuthProvider } from "@/contexts/auth-context";
import { OverlayFix } from "@shared/components/ui/overlay-fix";

export const metadata: Metadata = {
  title: "GesFer - Gestión de Chatarra",
  description: "Sistema de gestión de compra/venta de chatarra",
};

/**
 * RootLayout - Layout minimalista de la raíz
 * Solo contiene fuentes y etiquetas básicas (html, body)
 * La lógica visual específica vive en los layouts de grupo (admin) y (client)
 */

export default async function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  // El locale se obtiene automáticamente desde i18n.ts basado en el usuario
  // Manejar errores para evitar que bloquee la carga
  let locale: string = 'es';
  let messages: AbstractIntlMessages = {};
  
  try {
    locale = await getLocale();
  } catch (error) {
    // Si hay error obteniendo locale, usar default
    console.warn('Error obteniendo locale, usando default:', error);
    locale = 'es';
  }
  
  try {
    messages = await getMessages();
  } catch (error) {
    // Si hay error obteniendo messages, intentar cargar directamente
    console.warn('Error obteniendo messages, intentando cargar directamente:', error);
    try {
      messages = (await import(`@/messages/${locale}.json`)).default;
    } catch {
      try {
        // Si falla, intentar con español
        messages = (await import(`@/messages/es.json`)).default;
      } catch {
        // Si incluso el fallback falla, usar objeto vacío
        messages = {};
      }
    }
  }

  return (
    <html lang={locale}>
      <body>
        <OverlayFix />
        <Toaster richColors position="top-center" />
        <NextIntlClientProvider locale={locale} messages={messages}>
          <SessionProvider>
            <QueryProvider>
              <AuthProvider>{children}</AuthProvider>
            </QueryProvider>
          </SessionProvider>
        </NextIntlClientProvider>
      </body>
    </html>
  );
}

