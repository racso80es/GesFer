import { NextRequest, NextResponse } from "next/server";
import { Company } from "@/lib/types/api";
import { getAdminApiWithToken } from "@/lib/api/admin-api-server";
import { auth } from "@/auth";

interface Params {
  params: { id: string };
}

export async function GET(_request: NextRequest, { params }: Params) {
  try {
    const session = await auth();
    if (!session?.accessToken) {
      return NextResponse.json({ error: "No autorizado" }, { status: 401 });
    }
    const api = getAdminApiWithToken(session.accessToken);
    const company = await api.get<Company>(`/company/${params.id}`);

    if (!company) {
      return NextResponse.json(
        { error: "Organización no encontrada" },
        { status: 404 }
      );
    }

    return NextResponse.json(company);
  } catch (error) {
    console.error(`Error fetching company ${params.id}:`, error);
    return NextResponse.json(
      { error: "Error al obtener la organización" },
      { status: 500 }
    );
  }
}

export async function PUT(request: NextRequest, { params }: Params) {
  try {
    const session = await auth();
    if (!session?.accessToken) {
      return NextResponse.json({ error: "No autorizado" }, { status: 401 });
    }
    const body = await request.json();
    const api = getAdminApiWithToken(session.accessToken);
    const company = await api.put<Company>(`/company/${params.id}`, body);
    return NextResponse.json(company);
  } catch (error) {
    console.error(`Error updating company ${params.id}:`, error);
    return NextResponse.json(
      { error: "Error al actualizar la organización" },
      { status: 500 }
    );
  }
}

export async function DELETE(_request: NextRequest, { params }: Params) {
  try {
    const session = await auth();
    if (!session?.accessToken) {
      return NextResponse.json({ error: "No autorizado" }, { status: 401 });
    }
    const api = getAdminApiWithToken(session.accessToken);
    await api.delete(`/company/${params.id}`);
    return new NextResponse(null, { status: 204 });
  } catch (error) {
    console.error(`Error deleting company ${params.id}:`, error);
    return NextResponse.json(
      { error: "Error al eliminar la organización" },
      { status: 500 }
    );
  }
}
