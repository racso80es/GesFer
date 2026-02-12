import { NextRequest, NextResponse } from "next/server";
import { Company } from "@/lib/types/api";
import { getProductApi } from "@/lib/api/product-api";

export async function GET(request: NextRequest) {
  try {
    const api = getProductApi();
    const company = await api.get<Company>("/mycompany");

    if (!company) {
      return NextResponse.json(
        { error: "Organización no encontrada" },
        { status: 404 }
      );
    }

    return NextResponse.json(company);
  } catch (error) {
    console.error("Error fetching my company:", error);
    return NextResponse.json(
      { error: "Error al obtener la organización" },
      { status: 500 }
    );
  }
}

export async function PUT(request: NextRequest) {
  try {
    const body = await request.json();
    const api = getProductApi();
    const company = await api.put<Company>("/mycompany", body);
    return NextResponse.json(company);
  } catch (error) {
    console.error("Error updating my company:", error);
    return NextResponse.json(
      { error: "Error al actualizar la organización" },
      { status: 500 }
    );
  }
}
