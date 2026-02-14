import { NextRequest, NextResponse } from "next/server";
import { Company } from "@/lib/types/api";
import { getAdminApiWithToken } from "@/lib/api/admin-api-server";
import { auth } from "@/auth";

export async function GET() {
  try {
    const session = await auth();
    const accessToken = session?.accessToken ?? (session as { accessToken?: string } | null)?.accessToken;
    if (!accessToken) {
      return NextResponse.json({ error: "No autorizado", detail: "Falta token en la sesión" }, { status: 401 });
    }
    const api = getAdminApiWithToken(accessToken);
    const companies = await api.get<Company[]>("/company");
    return NextResponse.json(companies);
  } catch (error) {
    const message = error instanceof Error ? error.message : String(error);
    console.error("Error fetching companies:", message, error);
    return NextResponse.json(
      { error: "Error al obtener las organizaciones", detail: message },
      { status: 500 }
    );
  }
}

export async function POST(request: NextRequest) {
  try {
    const session = await auth();
    if (!session?.accessToken) {
      return NextResponse.json({ error: "No autorizado" }, { status: 401 });
    }
    const body = await request.json();
    const api = getAdminApiWithToken(session.accessToken);
    const company = await api.post<Company>("/company", body);
    return NextResponse.json(company, { status: 201 });
  } catch (error) {
    console.error("Error creating company:", error);
    return NextResponse.json(
      { error: "Error al crear la organización" },
      { status: 500 }
    );
  }
}
