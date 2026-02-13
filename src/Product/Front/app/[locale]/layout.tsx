import type { Metadata } from "next";
import { NextIntlClientProvider } from 'next-intl';
import { getMessages } from 'next-intl/server';
import { notFound } from 'next/navigation';
import "../globals.css";
import { Toaster } from "sonner";
import { QueryProvider } from "@/lib/providers/query-provider";
import { AuthProvider } from "@/contexts/auth-context";
import { OverlayFix } from "@shared/components/ui/overlay-fix";
import { locales } from "@/i18n";

export const metadata: Metadata = {
  title: "GesFer - Gestión de Chatarra",
  description: "Sistema de gestión de compra/venta de chatarra",
};

export default async function LocaleLayout({
  children,
  params
}: {
  children: React.ReactNode;
  params: Promise<{ locale: string }>;
}) {
  const { locale } = await params;
  
  // Validar que el locale sea válido
  if (!locales.includes(locale as (typeof locales)[number])) {
    notFound();
  }

  // Cargar mensajes para el locale específico
  const messages = await getMessages({ locale });

  return (
    <html lang={locale}>
      <body>
        <OverlayFix />
        <Toaster richColors position="top-center" />
        <NextIntlClientProvider locale={locale} messages={messages}>
          <QueryProvider>
            <AuthProvider>{children}</AuthProvider>
          </QueryProvider>
        </NextIntlClientProvider>
      </body>
    </html>
  );
}

