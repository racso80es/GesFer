"use client";

import { useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "../ui/dialog";
import { Button } from "./Button";
import { Input } from "./Input";
import { Label } from "../ui/label";
import { AlertTriangle, Loader2 } from "lucide-react";

interface DestructiveActionConfirmProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: () => Promise<void> | void;
  title?: string;
  description?: string;
  confirmationKeyword?: string;
  confirmButtonText?: string;
  cancelButtonText?: string;
  isLoading?: boolean;
}

/**
 * Componente Vision Zero para confirmación de acciones destructivas.
 * Requiere que el usuario escriba una palabra clave (por defecto "ELIMINAR") 
 * antes de habilitar el botón de ejecución.
 */
export function DestructiveActionConfirm({
  open,
  onOpenChange,
  onConfirm,
  title = "Confirmar acción destructiva",
  description = "Esta acción no se puede deshacer. Por favor, escribe la palabra clave para confirmar.",
  confirmationKeyword = "ELIMINAR",
  confirmButtonText = "Ejecutar",
  cancelButtonText = "Cancelar",
  isLoading = false,
}: DestructiveActionConfirmProps) {
  const [inputValue, setInputValue] = useState("");
  const [isExecuting, setIsExecuting] = useState(false);

  const isKeywordCorrect = inputValue.trim().toUpperCase() === confirmationKeyword.toUpperCase();
  const isButtonEnabled = isKeywordCorrect && !isExecuting && !isLoading;

  const handleConfirm = async () => {
    if (!isButtonEnabled) return;

    try {
      setIsExecuting(true);
      await onConfirm();
      // Cerrar el diálogo después de ejecutar la acción
      setInputValue("");
      onOpenChange(false);
    } catch (error) {
      // El error debe ser manejado por el componente padre
      console.error("Error al ejecutar acción destructiva:", error);
    } finally {
      setIsExecuting(false);
    }
  };

  const handleCancel = () => {
    setInputValue("");
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <div className="flex items-center gap-2">
            <AlertTriangle className="h-5 w-5 text-destructive" />
            <DialogTitle>{title}</DialogTitle>
          </div>
          <DialogDescription className="pt-2">
            {description}
          </DialogDescription>
        </DialogHeader>

        <div className="py-4">
          <Label htmlFor="confirmation-input" className="text-sm font-medium">
            Escribe <strong className="font-bold text-destructive">{confirmationKeyword}</strong> para confirmar:
          </Label>
          <Input
            id="confirmation-input"
            type="text"
            value={inputValue}
            onChange={(e) => setInputValue(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === "Enter" && isButtonEnabled) {
                handleConfirm();
              }
            }}
            placeholder={confirmationKeyword}
            className="mt-2"
            disabled={isExecuting || isLoading}
            autoFocus
            data-testid="shared-input-text-destructive-confirmation"
          />
          {inputValue && !isKeywordCorrect && (
            <p className="mt-2 text-sm text-muted-foreground">
              La palabra clave no coincide. Debes escribir exactamente: <strong>{confirmationKeyword}</strong>
            </p>
          )}
        </div>

        <DialogFooter>
          <Button
            variant="outline"
            onClick={handleCancel}
            disabled={isExecuting || isLoading}
            data-testid="shared-button-cancel"
          >
            {cancelButtonText}
          </Button>
          <Button
            variant="destructive"
            onClick={handleConfirm}
            disabled={!isButtonEnabled}
            data-testid="shared-button-confirm"
          >
            {isExecuting || isLoading ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Ejecutando...
              </>
            ) : (
              confirmButtonText
            )}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
