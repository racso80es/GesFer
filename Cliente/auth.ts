import NextAuth, { NextAuthConfig } from "next-auth";
import CredentialsProvider from "next-auth/providers/credentials";
import type { LoginResponse } from "@/lib/types/api";

/**
 * Configuración de autenticación con Auth.js v5
 * Utiliza CredentialsProvider para autenticar contra la API de ASP.NET Core
 * Soporta sesión dual: usuarios regulares y usuarios administrativos
 */
export const authConfig: NextAuthConfig = {
  providers: [
    // Provider para usuarios regulares (multi-tenant)
    CredentialsProvider({
      id: "credentials",
      name: "Credentials",
      credentials: {
        empresa: { label: "Empresa", type: "text" },
        usuario: { label: "Usuario", type: "text" },
        contraseña: { label: "Contraseña", type: "password" },
      },
      async authorize(credentials) {
        if (!credentials?.empresa || !credentials?.usuario || !credentials?.contraseña) {
          return null;
        }

        try {
          const apiUrl = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";
          const response = await fetch(`${apiUrl}/api/auth/login`, {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
            },
            body: JSON.stringify({
              empresa: credentials.empresa,
              usuario: credentials.usuario,
              contraseña: credentials.contraseña,
            }),
          });

          if (!response.ok) {
            return null;
          }

          const data: LoginResponse = await response.json();

          // Retornar el usuario con el token y cursorId
          return {
            id: data.cursorId, // Usar cursorId como id
            cursorId: data.cursorId,
            userId: data.userId,
            username: data.username,
            firstName: data.firstName,
            lastName: data.lastName,
            companyId: data.companyId,
            companyName: data.companyName,
            role: "User", // Rol para usuarios regulares
            permissions: data.permissions.join(","), // Convertir array a string para almacenar
            accessToken: data.token, // Guardar el token JWT
          };
        } catch (error) {
          console.error("Error en authorize:", error);
          return null;
        }
      },
    }),
    // Provider para usuarios administrativos
    CredentialsProvider({
      id: "admin",
      name: "Admin",
      credentials: {
        usuario: { label: "Usuario", type: "text" },
        contraseña: { label: "Contraseña", type: "password" },
      },
      async authorize(credentials) {
        if (!credentials?.usuario || !credentials?.contraseña) {
          console.error("Admin authorize: Faltan credenciales (usuario o contraseña)");
          return null;
        }

        try {
          const apiUrl = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";
          const loginUrl = `${apiUrl}/api/admin/auth/login`;
          
          console.log("Admin authorize: Intentando login en", loginUrl);
          
          const response = await fetch(loginUrl, {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
            },
            body: JSON.stringify({
              usuario: credentials.usuario,
              contraseña: credentials.contraseña,
            }),
          });

          if (!response.ok) {
            const errorText = await response.text();
            console.error(`Admin authorize: Error en respuesta (${response.status}):`, errorText);
            return null;
          }

          const data = await response.json();
          console.log("Admin authorize: Login exitoso para", data.username);

          // Retornar el usuario administrativo con el token y cursorId
          return {
            id: data.cursorId, // Usar cursorId como id
            cursorId: data.cursorId,
            userId: data.userId,
            username: data.username,
            firstName: data.firstName,
            lastName: data.lastName,
            email: data.email,
            role: data.role || "Admin", // Rol administrativo
            accessToken: data.token, // Guardar el token JWT con claim role: Admin
          };
        } catch (error) {
          console.error("Error en authorize (admin):", error);
          if (error instanceof Error) {
            console.error("Mensaje de error:", error.message);
            console.error("Stack trace:", error.stack);
          }
          return null;
        }
      },
    }),
  ],
  callbacks: {
    /**
     * Callback JWT: Se ejecuta cuando se genera o actualiza el token JWT
     * Aquí persistimos el accessToken y cursorId en el token de NextAuth
     */
    async jwt({ token, user, trigger }) {
      // Cuando el usuario inicia sesión, guardar los datos del usuario
      if (user) {
        token.cursorId = user.cursorId as string;
        token.userId = user.userId as string;
        token.username = user.username as string;
        token.firstName = user.firstName as string;
        token.lastName = user.lastName as string;
        token.role = user.role as string;
        token.email = user.email as string;
        
        // Campos específicos para usuarios regulares (multi-tenant)
        if (user.companyId) {
          token.companyId = user.companyId as string;
          token.companyName = user.companyName as string;
          token.permissions = user.permissions as string;
        }
        
        token.accessToken = user.accessToken as string;
      }

      return token;
    },
    /**
     * Callback Session: Se ejecuta cuando se accede a la sesión
     * Aquí exponemos los datos que estarán disponibles en Server Components y Client Components
     */
    async session({ session, token }) {
      if (session.user) {
        session.user.cursorId = token.cursorId as string;
        session.user.userId = token.userId as string;
        session.user.username = token.username as string;
        session.user.firstName = token.firstName as string;
        session.user.lastName = token.lastName as string;
        session.user.email = token.email as string;
        (session.user as any).role = token.role as string;
        
        // Campos específicos para usuarios regulares (multi-tenant)
        if (token.companyId) {
          session.user.companyId = token.companyId as string;
          session.user.companyName = token.companyName as string;
          (session.user as any).permissions = (token.permissions as string)?.split(",") || [];
        }
        
        // El accessToken se expone en session para uso en Server Components
        (session as any).accessToken = token.accessToken as string;
      }
      return session;
    },
  },
  pages: {
    signIn: "/login",
  },
  session: {
    strategy: "jwt", // Usar JWT strategy para almacenar la sesión
    maxAge: 60 * 60, // 1 hora (debe coincidir con la expiración del JWT del backend)
  },
  secret: process.env.AUTH_SECRET || "your-secret-key-change-in-production",
};

export const { handlers, auth, signIn, signOut } = NextAuth(authConfig);

