"use client";

import { CompanyForm } from "../../components/companies/company-form";
import { CreateCompany, UpdateCompany, Company } from "@/lib/types/api";
import { useEffect, useState } from "react";
import { useTranslations } from 'next-intl';
import { toast } from "sonner";

export default function MyCompanyPage() {
  const [company, setCompany] = useState<Company | null>(null);
  const [loading, setLoading] = useState(true);
  const t = useTranslations('myCompany');

  useEffect(() => {
    const fetchCompany = async () => {
      try {
        const response = await fetch("/api/my-company");
        if (!response.ok) throw new Error("Error al cargar la organización");
        const data = await response.json();
        setCompany(data);
      } catch (error) {
        console.error(error);
      } finally {
        setLoading(false);
      }
    };
    fetchCompany();
  }, []);

  const handleSubmit = async (data: CreateCompany | UpdateCompany) => {
    try {
      const response = await fetch("/api/my-company", {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(data),
      });

      if (!response.ok) throw new Error("Error al actualizar la organización");

      const updatedData = await response.json();
      setCompany(updatedData);
      toast.success(t('updatedSuccessfully'));
    } catch (error) {
      console.error(error);
      toast.error(t('updateError'));
    }
  };

  if (loading) return <div>{t('loading')}</div>;
  if (!company) return <div>{t('notFound')}</div>;

  return (
    <div className="container mx-auto py-10">
      <h1 className="text-3xl font-bold mb-6">{t('title')}</h1>
      <div className="bg-white p-6 rounded-lg shadow max-w-2xl">
        <CompanyForm
          company={company}
          onSubmit={handleSubmit}
          onCancel={() => {}} // No cancel needed
          isLoading={loading}
        />
      </div>
    </div>
  );
}
