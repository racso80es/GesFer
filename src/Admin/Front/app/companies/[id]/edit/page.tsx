"use client";

import { CompanyForm } from "../../../../components/companies/company-form";
import { useRouter } from "next/navigation";
import { CreateCompany, UpdateCompany, Company } from "@/lib/types/api";
import { useEffect, useState } from "react";

interface EditCompanyPageProps {
  params: {
    id: string;
  };
}

export default function EditCompanyPage({ params }: EditCompanyPageProps) {
  const router = useRouter();
  const [company, setCompany] = useState<Company | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const fetchCompany = async () => {
      try {
        const response = await fetch(`/api/companies/${params.id}`);
        if (!response.ok) throw new Error("Failed to fetch company");
        const data = await response.json();
        setCompany(data);
      } catch (error) {
        console.error(error);
      } finally {
        setIsLoading(false);
      }
    };

    fetchCompany();
  }, [params.id]);

  const handleSubmit = async (data: CreateCompany | UpdateCompany) => {
    try {
      const response = await fetch(`/api/companies/${params.id}`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        throw new Error("Failed to update company");
      }

      router.push("/companies");
      router.refresh();
    } catch (error) {
      console.error(error);
    }
  };

  if (isLoading) return <div>Cargando...</div>;
  if (!company) return <div>Organización no encontrada</div>;

  return (
    <div className="container mx-auto py-10">
      <h1 className="text-2xl font-bold mb-6">Editar Organización</h1>
      <div className="max-w-2xl bg-white p-6 rounded-lg shadow">
        <CompanyForm
          company={company}
          onSubmit={handleSubmit}
          onCancel={() => router.back()}
        />
      </div>
    </div>
  );
}
