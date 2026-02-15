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
import { ArticleFamily, articleFamiliesApi } from "@/lib/api/article-families-api";
import { ArticleFamilyForm } from "@/components/masters/ArticleFamilyForm";
import { toast } from "sonner";
import { useAuth } from "@/contexts/auth-context";
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

export default function ArticleFamiliesPage() {
  const t = useTranslations("articleFamilies");
  const tCommon = useTranslations("common");
  const { user } = useAuth();
  const [families, setFamilies] = useState<ArticleFamily[]>([]);
  const [loading, setLoading] = useState(true);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingFamily, setEditingFamily] = useState<ArticleFamily | undefined>();
  const [deleteId, setDeleteId] = useState<string | null>(null);

  const fetchFamilies = async () => {
    try {
      const data = await articleFamiliesApi.getAll();
      setFamilies(data);
    } catch (error) {
      toast.error("Error loading article families");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchFamilies();
  }, []);

  const handleCreate = () => {
    setEditingFamily(undefined);
    setIsFormOpen(true);
  };

  const handleEdit = (item: ArticleFamily) => {
    setEditingFamily(item);
    setIsFormOpen(true);
  };

  const handleDelete = async () => {
    if (!deleteId) return;
    try {
      await articleFamiliesApi.delete(deleteId);
      toast.success(t("deleteSuccess"));
      fetchFamilies();
    } catch (error) {
      toast.error("Error deleting article family");
    } finally {
      setDeleteId(null);
    }
  };

  const handleFormSubmit = async (values: { code: string; name: string; description?: string; taxTypeId: string }) => {
    if (!user?.companyId) {
      toast.error("Usuario sin organización");
      return;
    }
    try {
      if (editingFamily) {
        await articleFamiliesApi.update(editingFamily.id, {
          id: editingFamily.id,
          code: values.code,
          name: values.name,
          description: values.description,
          taxTypeId: values.taxTypeId,
        });
        toast.success(t("updateSuccess"));
      } else {
        await articleFamiliesApi.create({
          companyId: user.companyId,
          code: values.code,
          name: values.name,
          description: values.description,
          taxTypeId: values.taxTypeId,
        });
        toast.success(t("createSuccess"));
      }
      setIsFormOpen(false);
      fetchFamilies();
    } catch (error) {
      toast.error("Error saving article family");
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
                <TableHead>{t("taxType")}</TableHead>
                <TableHead>{t("description")}</TableHead>
                <TableHead className="w-[100px]">{t("actions")}</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {loading ? (
                <TableRow>
                  <TableCell colSpan={5} className="text-center py-10">
                    {tCommon("loading")}
                  </TableCell>
                </TableRow>
              ) : families.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={5} className="text-center py-10">
                    No records found
                  </TableCell>
                </TableRow>
              ) : (
                families.map((item) => (
                  <TableRow key={item.id}>
                    <TableCell className="font-medium">{item.code}</TableCell>
                    <TableCell>{item.name}</TableCell>
                    <TableCell>{item.taxTypeName ?? item.taxTypeId}</TableCell>
                    <TableCell>{item.description}</TableCell>
                    <TableCell>
                      <div className="flex items-center gap-2">
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => handleEdit(item)}
                        >
                          <Pencil className="h-4 w-4" />
                        </Button>
                        <Button
                          variant="ghost"
                          size="icon"
                          className="text-destructive"
                          onClick={() => setDeleteId(item.id)}
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

        <ArticleFamilyForm
          open={isFormOpen}
          onOpenChange={setIsFormOpen}
          onSubmit={handleFormSubmit}
          initialData={editingFamily}
        />

        <AlertDialog open={!!deleteId} onOpenChange={(open) => !open && setDeleteId(null)}>
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>{tCommon("confirm")}</AlertDialogTitle>
              <AlertDialogDescription>
                {t("deleteConfirm")}
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel>{tCommon("cancel")}</AlertDialogCancel>
              <AlertDialogAction onClick={handleDelete} className="bg-destructive text-destructive-foreground hover:bg-destructive/90">
                {tCommon("delete")}
              </AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>
      </div>
    </MainLayout>
  );
}
