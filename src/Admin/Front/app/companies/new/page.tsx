"use client";

import { CompanyForm } from "../../../components/companies/company-form";
import { useRouter } from "next/navigation";
import { CreateCompany } from "@/lib/types/api";

export default function NewCompanyPage() {
  const router = useRouter();

  const handleSubmit = async (data: CreateCompany) => {
    try {
      const response = await fetch("/api/companies", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        throw new Error("Failed to create company");
      }

      router.push("/companies");
      router.refresh();
    } catch (error) {
      console.error(error);
      // Handle error (e.g., show toast)
    }
  };

  return (
    <div className="container mx-auto py-10">
      <h1 className="text-2xl font-bold mb-6">Nueva Organización</h1>
      <div className="max-w-2xl bg-white p-6 rounded-lg shadow">
        <CompanyForm
          onSubmit={handleSubmit}
          onCancel={() => router.back()}
        />
      </div>
    </div>
  );
}
