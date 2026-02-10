import { NextRequest, NextResponse } from "next/server";
import { Company } from "@/lib/types/api";
import { getAdminApi } from "@/lib/api/admin-api";

interface Params {
  params: {
    id: string;
  };
}

export async function GET(request: NextRequest, { params }: Params) {
  try {
    const api = getAdminApi();
    const company = await api.get<Company>(`/company/${params.id}`);

    if (!company) {
      return NextResponse.json(
        { error: "Empresa no encontrada" },
        { status: 404 }
      );
    }

    return NextResponse.json(company);
  } catch (error) {
    console.error(`Error fetching company ${params.id}:`, error);
    return NextResponse.json(
      { error: "Error al obtener la empresa" },
      { status: 500 }
    );
  }
}

export async function PUT(request: NextRequest, { params }: Params) {
  try {
    const body = await request.json();
    const api = getAdminApi();
    const company = await api.put<Company>(`/company/${params.id}`, body);
    return NextResponse.json(company);
  } catch (error) {
    console.error(`Error updating company ${params.id}:`, error);
    return NextResponse.json(
      { error: "Error al actualizar la empresa" },
      { status: 500 }
    );
  }
}

export async function DELETE(request: NextRequest, { params }: Params) {
  try {
    const api = getAdminApi();
    await api.delete(`/company/${params.id}`);
    return new NextResponse(null, { status: 204 });
  } catch (error) {
    console.error(`Error deleting company ${params.id}:`, error);
    return NextResponse.json(
      { error: "Error al eliminar la empresa" },
      { status: 500 }
    );
  }
}
