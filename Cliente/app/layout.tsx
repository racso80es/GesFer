import type { Metadata } from "next";
import { Inter } from "next/font/google";
import { NextIntlClientProvider } from 'next-intl';
import { getMessages, getLocale } from 'next-intl/server';
import "./globals.css";
import { QueryProvider } from "@/lib/providers/query-provider";
import { SessionProvider } from "@/lib/providers/session-provider";
import { AuthProvider } from "@/contexts/auth-context";
import { OverlayFix } from "@/components/ui/overlay-fix";

const inter = Inter({ subsets: ["latin"] });

export const metadata: Metadata = {
  title: "GesFer - Gestión de Chatarra",
  description: "Sistema de gestión de compra/venta de chatarra",
};

export default async function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  // El locale se obtiene automáticamente desde i18n.ts basado en el usuario
  // Manejar errores para evitar que bloquee la carga
  let locale: string = 'es';
  let messages: any = {};
  
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
      <body className={inter.className}>
        <OverlayFix />
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

