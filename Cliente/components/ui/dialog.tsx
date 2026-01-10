import * as React from "react";
import { X } from "lucide-react";
import { cn } from "@/lib/utils/cn";
import { Button } from "./button";

interface DialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  children: React.ReactNode;
}

interface DialogContentProps extends React.HTMLAttributes<HTMLDivElement> {
  children: React.ReactNode;
}

interface DialogHeaderProps extends React.HTMLAttributes<HTMLDivElement> {
  children: React.ReactNode;
}

interface DialogTitleProps extends React.HTMLAttributes<HTMLHeadingElement> {
  children: React.ReactNode;
}

interface DialogDescriptionProps
  extends React.HTMLAttributes<HTMLParagraphElement> {
  children: React.ReactNode;
}

interface DialogFooterProps extends React.HTMLAttributes<HTMLDivElement> {
  children: React.ReactNode;
}

const Dialog = ({ open, onOpenChange, children }: DialogProps) => {
  // Efecto para manejar el overflow del body
  React.useEffect(() => {
    if (open) {
      const originalOverflow = document.body.style.overflow;
      document.body.style.overflow = "hidden";
      
      return () => {
        // Siempre restaurar el overflow al desmontar o cerrar
        document.body.style.overflow = originalOverflow || "unset";
      };
    } else {
      // Si se cierra, restaurar inmediatamente
      document.body.style.overflow = "unset";
    }
  }, [open]);

  // Protección adicional: timeout de seguridad para cerrar automáticamente si está abierto demasiado tiempo
  // Esto previene overlays bloqueantes que no se cierran correctamente
  React.useEffect(() => {
    if (open) {
      const safetyTimeout = setTimeout(() => {
        console.warn("Dialog: Timeout de seguridad activado, cerrando Dialog automáticamente");
        onOpenChange(false);
        // Asegurar que el body se restaure incluso si onOpenChange falla
        document.body.style.overflow = "unset";
      }, 300000); // 5 minutos como máximo (muy generoso, pero seguro)

      return () => {
        clearTimeout(safetyTimeout);
      };
    } else {
      // Si se cierra, asegurar que el body siempre se restaure
      document.body.style.overflow = "unset";
    }
  }, [open, onOpenChange]);

  // Efecto adicional: asegurar que cuando open es false, el body siempre esté liberado
  React.useEffect(() => {
    if (!open && document.body.style.overflow === "hidden") {
      // Si el Dialog está cerrado pero el body sigue bloqueado, restaurarlo
      console.warn("Dialog: Dialog cerrado pero body.overflow='hidden' detectado, restaurando...");
      document.body.style.overflow = "unset";
    }
  }, [open]);

  // Si no está abierto, no renderizar nada
  if (!open) return null;

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center"
      onClick={() => {
        onOpenChange(false);
        // Asegurar que el body se restaure al cerrar
        document.body.style.overflow = "unset";
      }}
      role="dialog"
      aria-modal="true"
    >
      <div
        className="fixed inset-0 bg-black/50"
        aria-hidden="true"
        style={{ pointerEvents: 'auto' }}
      />
      <div onClick={(e) => e.stopPropagation()} style={{ pointerEvents: 'auto' }}>
        {children}
      </div>
    </div>
  );
};

const DialogContent = React.forwardRef<HTMLDivElement, DialogContentProps>(
  ({ className, children, ...props }, ref) => {
    return (
      <div
        ref={ref}
        className={cn(
          "relative z-50 w-full max-w-lg max-h-[90vh] overflow-y-auto bg-card p-6 shadow-lg rounded-lg border",
          className
        )}
        {...props}
      >
        {children}
      </div>
    );
  }
);
DialogContent.displayName = "DialogContent";

const DialogHeader = React.forwardRef<HTMLDivElement, DialogHeaderProps>(
  ({ className, ...props }, ref) => (
    <div
      ref={ref}
      className={cn(
        "flex flex-col space-y-1.5 text-center sm:text-left mb-4",
        className
      )}
      {...props}
    />
  )
);
DialogHeader.displayName = "DialogHeader";

const DialogTitle = React.forwardRef<HTMLHeadingElement, DialogTitleProps>(
  ({ className, ...props }, ref) => (
    <h2
      ref={ref}
      className={cn("text-2xl font-semibold leading-none tracking-tight", className)}
      {...props}
    />
  )
);
DialogTitle.displayName = "DialogTitle";

const DialogDescription = React.forwardRef<
  HTMLParagraphElement,
  DialogDescriptionProps
>(({ className, ...props }, ref) => (
  <p
    ref={ref}
    className={cn("text-sm text-muted-foreground", className)}
    {...props}
  />
));
DialogDescription.displayName = "DialogDescription";

const DialogFooter = React.forwardRef<HTMLDivElement, DialogFooterProps>(
  ({ className, ...props }, ref) => (
    <div
      ref={ref}
      className={cn(
        "flex flex-col-reverse sm:flex-row sm:justify-end sm:space-x-2 mt-6",
        className
      )}
      {...props}
    />
  )
);
DialogFooter.displayName = "DialogFooter";

const DialogClose = React.forwardRef<
  HTMLButtonElement,
  React.ButtonHTMLAttributes<HTMLButtonElement> & {
    onClose: () => void;
  }
>(({ className, onClose, ...props }, ref) => (
  <Button
    ref={ref}
    variant="ghost"
    size="icon"
    className={cn("absolute right-4 top-4", className)}
    onClick={onClose}
    {...props}
  >
    <X className="h-4 w-4" />
    <span className="sr-only">Cerrar</span>
  </Button>
));
DialogClose.displayName = "DialogClose";

export {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
  DialogClose,
};

