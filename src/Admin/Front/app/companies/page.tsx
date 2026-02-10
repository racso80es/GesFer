import Link from "next/link";
import { Button } from "../../components/ui/button";
import { Plus, Pencil, Trash } from "lucide-react";
import { getAdminApi } from "@/lib/api/admin-api";
import { Company } from "@/lib/types/api";

export default async function CompaniesPage() {
  const api = getAdminApi();
  let companies: Company[] = [];

  try {
    companies = await api.get<Company[]>("/company");
  } catch (error) {
    console.error("Error fetching companies:", error);
  }

  return (
    <div className="container mx-auto py-10">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold">Empresas</h1>
        <Link href="/companies/new">
          <Button>
            <Plus className="mr-2 h-4 w-4" /> Nueva Empresa
          </Button>
        </Link>
      </div>

      <div className="bg-white rounded-md border shadow">
        <table className="w-full">
          <thead>
            <tr className="border-b bg-gray-50">
              <th className="text-left py-3 px-4 font-medium text-gray-500">Nombre</th>
              <th className="text-left py-3 px-4 font-medium text-gray-500">CIF/NIF</th>
              <th className="text-left py-3 px-4 font-medium text-gray-500">Email</th>
              <th className="text-left py-3 px-4 font-medium text-gray-500">Estado</th>
              <th className="text-right py-3 px-4 font-medium text-gray-500">Acciones</th>
            </tr>
          </thead>
          <tbody>
            {companies.length === 0 ? (
              <tr>
                <td colSpan={5} className="text-center py-8 text-gray-500">
                  No hay empresas registradas
                </td>
              </tr>
            ) : (
              companies.map((company) => (
                <tr key={company.id} className="border-b last:border-0 hover:bg-gray-50">
                  <td className="py-3 px-4">{company.name}</td>
                  <td className="py-3 px-4">{company.taxId || "-"}</td>
                  <td className="py-3 px-4">{company.email || "-"}</td>
                  <td className="py-3 px-4">
                    <span
                      className={`px-2 py-1 rounded text-xs font-medium ${
                        company.isActive
                          ? "bg-green-100 text-green-800"
                          : "bg-red-100 text-red-800"
                      }`}
                    >
                      {company.isActive ? "Activo" : "Inactivo"}
                    </span>
                  </td>
                  <td className="py-3 px-4 text-right">
                    <Link href={`/companies/${company.id}/edit`}>
                      <Button variant="ghost" size="sm">
                        <Pencil className="h-4 w-4" />
                      </Button>
                    </Link>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
