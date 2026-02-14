import NextAuth, { NextAuthConfig } from "next-auth";
import CredentialsProvider from "next-auth/providers/credentials";
import { serverPostJson } from "@/lib/api/server-fetch";

/**
 * Configuración de autenticación para GesFer Admin
 * Utiliza CredentialsProvider para autenticar contra la API Admin de ASP.NET Core.
 * En desarrollo usa serverPostJson para aceptar el certificado HTTPS autofirmado de la API.
 */
export const authConfig: NextAuthConfig = {
  providers: [
    // Provider para usuarios administrativos
    CredentialsProvider({
      id: "admin",
      name: "Admin",
      credentials: {
        username: { label: "Usuario", type: "text" },
        password: { label: "Contraseña", type: "password" },
      },
      async authorize(credentials) {
        if (!credentials?.username || !credentials?.password) {
          return null;
        }

        try {
          const apiUrl = (process.env.ADMIN_API_URL || "http://localhost:5010").replace(/\/+$/, "");
          const loginUrl = apiUrl.endsWith("/api") ? `${apiUrl}/admin/auth/login` : `${apiUrl}/api/admin/auth/login`;

          const { ok, status, data, errorText } = await serverPostJson<{
            userId?: string;
            cursorId?: string;
            username?: string;
            firstName?: string;
            lastName?: string;
            email?: string | null;
            role?: string;
            token?: string;
          }>(loginUrl, {
            usuario: credentials.username,
            contraseña: credentials.password,
          });

          if (!ok || !data) {
            console.error("Login failed:", status, errorText);
            return null;
          }

          return {
            id: data.cursorId ?? "",
            cursorId: data.cursorId ?? "",
            userId: data.userId ?? "",
            username: data.username ?? "",
            firstName: data.firstName ?? "",
            lastName: data.lastName ?? "",
            email: data.email ?? undefined,
            role: data.role ?? "Admin",
            accessToken: data.token ?? "",
          };
        } catch (error) {
          console.error("Error en authorize (admin):", error);
          return null;
        }
      },
    }),
  ],
  callbacks: {
    async jwt({ token, user }) {
      if (user) {
        token.cursorId = user.cursorId as string;
        token.userId = user.userId as string;
        token.username = user.username as string;
        token.firstName = user.firstName as string;
        token.lastName = user.lastName as string;
        token.role = user.role as string;
        token.email = user.email as string;
        token.accessToken = user.accessToken as string;
      }
      return token;
    },
    async session({ session, token }) {
      if (session.user) {
        session.user.cursorId = token.cursorId as string;
        session.user.userId = token.userId as string;
        session.user.username = token.username as string;
        session.user.firstName = token.firstName as string;
        session.user.lastName = token.lastName as string;
        session.user.email = token.email as string;
        session.user.role = token.role as string;
        session.accessToken = token.accessToken as string;
      }
      return session;
    },
  },
  pages: {
    signIn: "/login",
  },
  session: {
    strategy: "jwt",
    maxAge: 60 * 60,
  },
  secret: process.env.AUTH_SECRET || "your-secret-key-change-in-production",
};

export const { handlers, auth, signIn, signOut } = NextAuth(authConfig);
