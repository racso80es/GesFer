import type { Metadata } from "next";
import { NextIntlClientProvider } from "next-intl";
import { getLocale, getMessages } from "next-intl/server";
import "./globals.css";
import { AdminAppWrapper } from "@/components/layout/admin-app-wrapper";

// Evita pre-render en build: auth y API no están disponibles; las rutas se generan en request.
export const dynamic = "force-dynamic";

export const metadata: Metadata = {
  title: "GesFer Admin",
  description: "Panel de administración GesFer",
};

export default async function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const locale = await getLocale();
  const messages = await getMessages();

  return (
    <html lang={locale}>
      <body>
        <NextIntlClientProvider locale={locale} messages={messages}>
          <AdminAppWrapper>{children}</AdminAppWrapper>
        </NextIntlClientProvider>
      </body>
    </html>
  );
}
