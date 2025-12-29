"use client";

import { ProtectedRoute } from "@/components/auth/protected-route";
import { MainLayout } from "@/components/layout/main-layout";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Loading } from "@/components/ui/loading";
import { ErrorMessage } from "@/components/ui/error-message";
import { useQuery } from "@tanstack/react-query";
import { customersApi } from "@/lib/api/customers";
import { useAuth } from "@/contexts/auth-context";
import { Plus, Edit, Trash2, Building2 } from "lucide-react";
import { useState } from "react";
import { useTranslations } from 'next-intl';

export default function ClientesPage() {
  const { user } = useAuth();
  const t = useTranslations('customers');
  const [selectedCustomerId, setSelectedCustomerId] = useState<string | null>(null);

  const {
    data: clientes,
    isLoading,
    error,
    refetch,
  } = useQuery({
    queryKey: ["customers", user?.companyId],
    queryFn: () => customersApi.getAll(user?.companyId),
    enabled: !!user?.companyId,
  });

  const handleDelete = async (id: string) => {
    if (!confirm(t('deleteConfirm'))) {
      return;
    }

    try {
      await customersApi.delete(id);
      refetch();
    } catch (error) {
      alert(
        error instanceof Error
          ? error.message
          : t('deleteError')
      );
    }
  };

  return (
    <ProtectedRoute>
      <MainLayout>
        <div className="space-y-6">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-3xl font-bold">{t('title')}</h1>
              <p className="text-muted-foreground">
                {t('subtitle')}
              </p>
            </div>
            <Button>
              <Plus className="h-4 w-4 mr-2" />
              {t('newCustomer')}
            </Button>
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
                  : t('error')
              }
            />
          )}

          {clientes && clientes.length === 0 && (
            <Card>
              <CardContent className="flex flex-col items-center justify-center py-12">
                <Building2 className="h-12 w-12 text-muted-foreground mb-4" />
                <p className="text-muted-foreground">
                  {t('noCustomers')}
                </p>
              </CardContent>
            </Card>
          )}

          {clientes && clientes.length > 0 && (
            <Card>
              <CardHeader>
                <CardTitle>{t('listTitle')}</CardTitle>
                <CardDescription>
                  {t('listDescription', { count: clientes.length })}
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="overflow-x-auto">
                  <table className="w-full">
                    <thead>
                      <tr className="border-b">
                        <th className="text-left p-2">{t('table.name')}</th>
                        <th className="text-left p-2">{t('table.taxId')}</th>
                        <th className="text-left p-2">{t('table.email')}</th>
                        <th className="text-left p-2">{t('table.phone')}</th>
                        <th className="text-left p-2">{t('table.address')}</th>
                        <th className="text-left p-2">{t('table.status')}</th>
                        <th className="text-right p-2">{t('table.actions')}</th>
                      </tr>
                    </thead>
                    <tbody>
                      {clientes.map((cliente) => (
                        <tr key={cliente.id} className="border-b hover:bg-muted/50">
                          <td className="p-2 font-medium">{cliente.name}</td>
                          <td className="p-2">{cliente.taxId || "-"}</td>
                          <td className="p-2">{cliente.email || "-"}</td>
                          <td className="p-2">{cliente.phone || "-"}</td>
                          <td className="p-2">{cliente.address || "-"}</td>
                          <td className="p-2">
                            <span
                              className={`inline-flex items-center rounded-full px-2 py-1 text-xs font-medium ${
                                cliente.isActive
                                  ? "bg-green-100 text-green-800"
                                  : "bg-red-100 text-red-800"
                              }`}
                            >
                              {cliente.isActive ? t('table.active') : t('table.inactive')}
                            </span>
                          </td>
                          <td className="p-2">
                            <div className="flex justify-end gap-2">
                              <Button
                                variant="ghost"
                                size="icon"
                                onClick={() => setSelectedCustomerId(cliente.id)}
                              >
                                <Edit className="h-4 w-4" />
                              </Button>
                              <Button
                                variant="ghost"
                                size="icon"
                                onClick={() => handleDelete(cliente.id)}
                              >
                                <Trash2 className="h-4 w-4 text-destructive" />
                              </Button>
                            </div>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </CardContent>
            </Card>
          )}
        </div>
      </MainLayout>
    </ProtectedRoute>
  );
}

