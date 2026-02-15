"use client";

import type { ControllerRenderProps } from "react-hook-form";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { Button } from "@shared/components/shared/Button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@shared/components/ui/dialog";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@shared/components/ui/form";
import { Input } from "@shared/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@shared/components/ui/select";
import { ArticleFamily } from "@/lib/api/article-families-api";
import { TaxType, taxTypesApi } from "@/lib/api/tax-types-api";
import { useTranslations } from "next-intl";
import { useEffect, useState } from "react";

const formSchema = z.object({
  code: z.string().min(1, "codeRequired").max(10),
  name: z.string().min(1, "nameRequired").max(50),
  description: z.string().max(255).optional(),
  taxTypeId: z.string().min(1, "taxTypeRequired"),
});

type FormValues = z.infer<typeof formSchema>;

interface ArticleFamilyFormProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onSubmit: (values: z.infer<typeof formSchema>) => void;
  initialData?: ArticleFamily;
  isLoading?: boolean;
}

export function ArticleFamilyForm({
  open,
  onOpenChange,
  onSubmit,
  initialData,
  isLoading,
}: ArticleFamilyFormProps) {
  const t = useTranslations("articleFamilies");
  const tCommon = useTranslations("common");
  const [taxTypes, setTaxTypes] = useState<TaxType[]>([]);

  const form = useForm<z.infer<typeof formSchema>>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      code: "",
      name: "",
      description: "",
      taxTypeId: "",
    },
  });

  useEffect(() => {
    if (open) {
      taxTypesApi.getAll().then(setTaxTypes).catch(() => setTaxTypes([]));
    }
  }, [open]);

  useEffect(() => {
    if (initialData) {
      form.reset({
        code: initialData.code,
        name: initialData.name,
        description: initialData.description || "",
        taxTypeId: initialData.taxTypeId,
      });
    } else {
      form.reset({
        code: "",
        name: "",
        description: "",
        taxTypeId: "",
      });
    }
  }, [initialData, form, open]);

  const handleSubmit = (values: z.infer<typeof formSchema>) => {
    onSubmit(values);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>
            {initialData ? t("editTitle") : t("createTitle")}
          </DialogTitle>
        </DialogHeader>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-4">
            <FormField
              control={form.control}
              name="code"
              render={({ field }: { field: ControllerRenderProps<FormValues, "code"> }) => (
                <FormItem>
                  <FormLabel>{t("code")}</FormLabel>
                  <FormControl>
                    <Input {...field} disabled={!!initialData} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="name"
              render={({ field }: { field: ControllerRenderProps<FormValues, "name"> }) => (
                <FormItem>
                  <FormLabel>{t("name")}</FormLabel>
                  <FormControl>
                    <Input {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="taxTypeId"
              render={({ field }: { field: ControllerRenderProps<FormValues, "taxTypeId"> }) => (
                <FormItem>
                  <FormLabel>{t("taxType")}</FormLabel>
                  <Select
                    onValueChange={field.onChange}
                    value={field.value}
                    disabled={!!initialData}
                  >
                    <FormControl>
                      <SelectTrigger>
                        <SelectValue placeholder={t("taxTypePlaceholder")} />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {taxTypes.map((tt) => (
                        <SelectItem key={tt.id} value={tt.id}>
                          {tt.code} - {tt.name} ({tt.value}%)
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />
            <FormField
              control={form.control}
              name="description"
              render={({ field }: { field: ControllerRenderProps<FormValues, "description"> }) => (
                <FormItem>
                  <FormLabel>{t("description")}</FormLabel>
                  <FormControl>
                    <Input {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />
            <DialogFooter>
              <Button type="submit" disabled={isLoading}>
                {tCommon("save")}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
