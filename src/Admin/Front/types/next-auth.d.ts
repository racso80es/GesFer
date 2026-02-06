import NextAuth, { DefaultSession } from "next-auth";
import { JWT } from "next-auth/jwt";

declare module "next-auth" {
  /**
   * Returned by `useSession`, `getSession` and received as a prop on the `SessionProvider` React Context
   */
  interface Session {
    user: {
      cursorId: string;
      userId: string;
      username: string;
      firstName: string;
      lastName: string;
      role: string;
      email?: string;
    } & DefaultSession["user"];
    accessToken?: string;
  }

  interface User {
    cursorId: string;
    userId: string;
    username: string;
    firstName: string;
    lastName: string;
    role: string;
    accessToken?: string;
  }
}

declare module "next-auth/jwt" {
  interface JWT {
    cursorId: string;
    userId: string;
    username: string;
    firstName: string;
    lastName: string;
    role: string;
    accessToken?: string;
  }
}
