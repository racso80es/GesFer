"use client";

import * as React from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
  DialogClose,
} from "../ui/dialog";
import { Button } from "./Button";
import { cn } from "../../lib/utils/cn";

export interface ModalBaseProps {
  /**
   * Controla si el modal está abierto
   */
  open: boolean;
  
  /**
   * Callback cuando cambia el estado de apertura
   */
  onOpenChange: (open: boolean) => void;
  
  /**
   * Título del modal
   */
  title: string;
  
  /**
   * Descripción del modal (opcional)
   */
  description?: string;
  
  /**
   * Contenido del modal
   */
  children: React.ReactNode;
  
  /**
   * Contenido del footer (opcional)
   * Si no se proporciona, se renderiza un footer por defecto con botones
   */
  footer?: React.ReactNode;
  
  /**
   * Texto del botón de confirmar (opcional)
   * Si se proporciona, se renderiza un footer por defecto
   */
  confirmText?: string;
  
  /**
   * Texto del botón de cancelar (opcional)
   * Si se proporciona, se renderiza un footer por defecto
   */
  cancelText?: string;
  
  /**
   * Callback cuando se confirma (opcional)
   * Si se proporciona, se renderiza un footer por defecto
   */
  onConfirm?: () => void;
  
  /**
   * Callback cuando se cancela (opcional)
   * Si se proporciona, se renderiza un footer por defecto
   */
  onCancel?: () => void;
  
  /**
   * Si el botón de confirmar está deshabilitado
   */
  confirmDisabled?: boolean;
  
  /**
   * Variante del botón de confirmar
   */
  confirmVariant?: "default" | "destructive" | "outline" | "secondary" | "ghost" | "link";
  
  /**
   * Si se muestra el botón de cerrar (X)
   */
  showClose?: boolean;
  
  /**
   * Clase CSS adicional para el contenido
   */
  className?: string;
  
  /**
   * Identificador para tests (data-testid)
   * Nomenclatura: shared-modal-[acción]
   * Ejemplo: shared-modal-confirm, shared-modal-delete
   */
  "data-testid"?: string;
}

/**
 * Componente ModalBase compartido
 * 
 * Componente puro e inmutable que envuelve el Dialog de shadcn/ui
 * con soporte para data-testid para tests automatizados.
 * 
 * Cualquier variación de comportamiento o estilo debe pasarse mediante props.
 */
export const ModalBase: React.FC<ModalBaseProps> = ({
  open,
  onOpenChange,
  title,
  description,
  children,
  footer,
  confirmText,
  cancelText,
  onConfirm,
  onCancel,
  confirmDisabled = false,
  confirmVariant = "default",
  showClose = true,
  className,
  "data-testid": dataTestId,
}) => {
  const testId = dataTestId || "shared-modal-base";

  const handleCancel = () => {
    if (onCancel) {
      onCancel();
    } else {
      onOpenChange(false);
    }
  };

  const handleConfirm = () => {
    if (onConfirm) {
      onConfirm();
    }
  };

  // Renderizar footer por defecto si se proporcionan confirmText o cancelText
  const renderFooter = () => {
    if (footer) {
      return footer;
    }

    if (confirmText || cancelText) {
      return (
        <DialogFooter>
          {cancelText && (
            <Button
              variant="outline"
              onClick={handleCancel}
              data-testid={`${testId}-cancel`}
            >
              {cancelText}
            </Button>
          )}
          {confirmText && (
            <Button
              variant={confirmVariant}
              onClick={handleConfirm}
              disabled={confirmDisabled}
              data-testid={`${testId}-confirm`}
            >
              {confirmText}
            </Button>
          )}
        </DialogFooter>
      );
    }

    return null;
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent
        className={cn(className)}
        data-testid={testId}
      >
        {showClose && <DialogClose onClose={handleCancel} />}
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          {description && <DialogDescription>{description}</DialogDescription>}
        </DialogHeader>
        <div className="space-y-4 py-4">{children}</div>
        {renderFooter()}
      </DialogContent>
    </Dialog>
  );
};
