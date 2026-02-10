import { NextRequest, NextResponse } from "next/server";
import { Company } from "@/lib/types/api";
import { getAdminApi } from "@/lib/api/admin-api";

export async function GET(request: NextRequest) {
  try {
    const api = getAdminApi();
    const companies = await api.get<Company[]>("/company");
    return NextResponse.json(companies);
  } catch (error) {
    console.error("Error fetching companies:", error);
    return NextResponse.json(
      { error: "Error al obtener las empresas" },
      { status: 500 }
    );
  }
}

export async function POST(request: NextRequest) {
  try {
    const body = await request.json();
    const api = getAdminApi();
    const company = await api.post<Company>("/company", body);
    return NextResponse.json(company, { status: 201 });
  } catch (error) {
    console.error("Error creating company:", error);
    return NextResponse.json(
      { error: "Error al crear la empresa" },
      { status: 500 }
    );
  }
}
