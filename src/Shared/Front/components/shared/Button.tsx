"use client";

import * as React from "react";
import { Button as ShadcnButton, type ButtonProps as ShadcnButtonProps } from "../ui/button";
import { cn } from "../../lib/utils/cn";

export interface ButtonProps extends ShadcnButtonProps {
  /**
   * Identificador para tests (data-testid)
   * Nomenclatura: shared-button-[acción]
   * Ejemplo: shared-button-confirm, shared-button-cancel
   */
  "data-testid"?: string;
}

/**
 * Componente Button compartido
 * 
 * Componente puro e inmutable que envuelve el Button de shadcn/ui
 * con soporte para data-testid para tests automatizados.
 * 
 * Cualquier variación de comportamiento o estilo debe pasarse mediante props.
 */
const Button = React.forwardRef<HTMLButtonElement, ButtonProps>(
  ({ className, "data-testid": dataTestId, ...props }, ref) => {
    // Generar data-testid automático si no se proporciona
    // Basado en variant y children si es posible
    const testId = dataTestId || `shared-button-${props.variant || "default"}`;

    return (
      <ShadcnButton
        ref={ref}
        className={cn(className)}
        data-testid={testId}
        {...props}
      />
    );
  }
);
Button.displayName = "SharedButton";

export { Button };
