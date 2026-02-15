"use client";

import { MainLayout } from "@/components/layout/main-layout";
import { Button } from "@shared/components/shared/Button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@shared/components/ui/table";
import { Plus, Pencil, Trash2 } from "lucide-react";
import { useTranslations } from "next-intl";
import { useState, useEffect } from "react";
import { TaxType, CreateTaxTypeDto, taxTypesApi } from "@/lib/api/tax-types-api";
import { TaxTypeForm } from "@/components/masters/TaxTypeForm";
import { toast } from "sonner";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@shared/components/ui/alert-dialog";

export default function TaxTypesPage() {
  const t = useTranslations("taxTypes");
  const tCommon = useTranslations("common");
  const [taxTypes, setTaxTypes] = useState<TaxType[]>([]);
  const [loading, setLoading] = useState(true);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingTaxType, setEditingTaxType] = useState<TaxType | undefined>();
  const [deleteId, setDeleteId] = useState<string | null>(null);

  const fetchTaxTypes = async () => {
    try {
      const data = await taxTypesApi.getAll();
      setTaxTypes(data);
    } catch (error) {
      toast.error("Error loading tax types");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchTaxTypes();
  }, []);

  const handleCreate = () => {
    setEditingTaxType(undefined);
    setIsFormOpen(true);
  };

  const handleEdit = (taxType: TaxType) => {
    setEditingTaxType(taxType);
    setIsFormOpen(true);
  };

  const handleDelete = async () => {
    if (!deleteId) return;
    try {
      await taxTypesApi.delete(deleteId);
      toast.success(t("deleteSuccess"));
      fetchTaxTypes();
    } catch (error) {
      toast.error("Error deleting tax type");
    } finally {
      setDeleteId(null);
    }
  };

  const handleFormSubmit = async (values: CreateTaxTypeDto) => {
    try {
      if (editingTaxType) {
        await taxTypesApi.update(editingTaxType.id, {
          ...values,
          id: editingTaxType.id,
        });
        toast.success(t("updateSuccess"));
      } else {
        await taxTypesApi.create(values);
        toast.success(t("createSuccess"));
      }
      setIsFormOpen(false);
      fetchTaxTypes();
    } catch (error) {
      toast.error("Error saving tax type");
    }
  };

  return (
    <MainLayout>
      <div className="space-y-6">
        <div className="flex justify-between items-center">
          <h1 className="text-3xl font-bold tracking-tight">{t("title")}</h1>
          <Button onClick={handleCreate}>
            <Plus className="mr-2 h-4 w-4" />
            {t("createTitle")}
          </Button>
        </div>

        <div className="border rounded-md">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>{t("code")}</TableHead>
                <TableHead>{t("name")}</TableHead>
                <TableHead>{t("value")}</TableHead>
                <TableHead>{t("description")}</TableHead>
                <TableHead className="w-[100px]">{t("actions")}</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {loading ? (
                <TableRow>
                  <TableCell colSpan={5} className="text-center py-10">
                    Loading...
                  </TableCell>
                </TableRow>
              ) : taxTypes.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={5} className="text-center py-10">
                    No records found
                  </TableCell>
                </TableRow>
              ) : (
                taxTypes.map((item) => (
                  <TableRow key={item.id}>
                    <TableCell className="font-medium">{item.code}</TableCell>
                    <TableCell>{item.name}</TableCell>
                    <TableCell>{item.value}%</TableCell>
                    <TableCell>{item.description}</TableCell>
                    <TableCell>
                      <div className="flex items-center gap-2">
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => handleEdit(item)}
                          aria-label={tCommon("edit")}
                        >
                          <Pencil className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="icon"
                          className="text-destructive"
                          onClick={() => setDeleteId(item.id)}
                          aria-label={tCommon("delete")}
                        >
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </div>

        <TaxTypeForm
          open={isFormOpen}
          onOpenChange={setIsFormOpen}
          onSubmit={handleFormSubmit}
          initialData={editingTaxType}
        />

        <AlertDialog open={!!deleteId} onOpenChange={(open) => !open && setDeleteId(null)}>
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>Confirm Deletion</AlertDialogTitle>
              <AlertDialogDescription>
                {t("deleteConfirm")}
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel>Cancel</AlertDialogCancel>
              <AlertDialogAction onClick={handleDelete} className="bg-destructive text-destructive-foreground hover:bg-destructive/90">
                Delete
              </AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>
      </div>
    </MainLayout>
  );
}
