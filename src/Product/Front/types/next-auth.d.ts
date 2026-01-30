import "next-auth";
import "next-auth/jwt";

declare module "next-auth" {
  interface Session {
    user: {
      id: string;
      cursorId: string;
      userId: string;
      username: string;
      firstName: string;
      lastName: string;
      email?: string;
      role: string; // "User" para usuarios regulares, "Admin" para administrativos
      companyId?: string; // Solo para usuarios regulares (multi-tenant)
      companyName?: string; // Solo para usuarios regulares (multi-tenant)
      permissions?: string[]; // Solo para usuarios regulares
    };
    accessToken?: string;
  }

  interface User {
    id: string;
    cursorId: string;
    userId: string;
    username: string;
    firstName: string;
    lastName: string;
    email?: string;
    role: string; // "User" para usuarios regulares, "Admin" para administrativos
    companyId?: string; // Solo para usuarios regulares (multi-tenant)
    companyName?: string; // Solo para usuarios regulares (multi-tenant)
    permissions?: string; // Solo para usuarios regulares (como string separado por comas)
    accessToken: string;
  }
}

declare module "next-auth/jwt" {
  interface JWT {
    cursorId?: string;
    userId?: string;
    username?: string;
    firstName?: string;
    lastName?: string;
    email?: string;
    role?: string; // "User" para usuarios regulares, "Admin" para administrativos
    companyId?: string; // Solo para usuarios regulares (multi-tenant)
    companyName?: string; // Solo para usuarios regulares (multi-tenant)
    permissions?: string; // Solo para usuarios regulares (como string separado por comas)
    accessToken?: string;
  }
}

