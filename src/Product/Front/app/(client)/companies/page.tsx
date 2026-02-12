"use client";

import { ProtectedRoute } from "@/components/auth/protected-route";
import { MainLayout } from "@/components/layout/main-layout";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@shared/components/ui/card";
import { Loading } from "@shared/components/ui/loading";
import { ErrorMessage } from "@shared/components/ui/error-message";
import { CompanyForm } from "@/components/companies/company-form";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { myCompanyApi } from "@/lib/api/my-company";
import { useTranslations } from 'next-intl';
import type { UpdateCompany, CreateCompany } from "@/lib/types/api";
import { useState } from "react";
import { Building2 } from "lucide-react";

export default function MyCompanyPage() {
  const queryClient = useQueryClient();
  const t = useTranslations('myCompany');
  const tCompanies = useTranslations('companies');
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const {
    data: company,
    isLoading,
    error,
  } = useQuery({
    queryKey: ["myCompany"],
    queryFn: () => myCompanyApi.get(),
  });

  const updateMutation = useMutation({
    mutationFn: (data: UpdateCompany) => myCompanyApi.update(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["myCompany"] });
      setSuccessMessage(t('updatedSuccessfully'));
      setTimeout(() => setSuccessMessage(null), 3000);
    },
  });

  const handleUpdate = async (data: CreateCompany | UpdateCompany) => {
    // Only pass relevant fields for update
    await updateMutation.mutateAsync(data as UpdateCompany);
  };

  return (
    <ProtectedRoute>
      <MainLayout>
        <div className="space-y-6">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-3xl font-bold">{t('title')}</h1>
            </div>
          </div>

          {isLoading && (
            <div className="flex justify-center py-12">
              <Loading size="lg" text={t('loading')} />
            </div>
          )}

          {error && (
            <ErrorMessage
              message={
                error instanceof Error
                  ? error.message
                  : t('notFound')
              }
            />
          )}

          {successMessage && (
            <div className="bg-green-100 border border-green-400 text-green-700 px-4 py-3 rounded relative mb-4" role="alert">
              <span className="block sm:inline">{successMessage}</span>
            </div>
          )}

          {company && (
            <Card>
              <CardHeader>
                <div className="flex items-center gap-2">
                    <Building2 className="h-5 w-5 text-muted-foreground" />
                    <CardTitle>{company.name}</CardTitle>
                </div>
                <CardDescription>
                  {tCompanies('editDescription')}
                </CardDescription>
              </CardHeader>
              <CardContent>
                <CompanyForm
                  company={company}
                  onSubmit={handleUpdate}
                  onCancel={() => {}} // No cancel action needed on single page
                  isLoading={updateMutation.isPending}
                />
              </CardContent>
            </Card>
          )}
        </div>
      </MainLayout>
    </ProtectedRoute>
  );
}
