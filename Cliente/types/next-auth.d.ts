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
      companyId: string;
      companyName: string;
      permissions: string[];
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
    companyId: string;
    companyName: string;
    permissions: string;
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
    companyId?: string;
    companyName?: string;
    permissions?: string;
    accessToken?: string;
  }
}

