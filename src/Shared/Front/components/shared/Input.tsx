"use client";

import * as React from "react";
import { Input as ShadcnInput, type InputProps as ShadcnInputProps } from "../ui/input";
import { cn } from "../../lib/utils/cn";

export interface InputProps extends ShadcnInputProps {
  /**
   * Identificador para tests (data-testid)
   * Nomenclatura: shared-input-[tipo]-[acción]
   * Ejemplo: shared-input-text-search, shared-input-datetime-from
   */
  "data-testid"?: string;
}

/**
 * Componente Input compartido
 * 
 * Componente puro e inmutable que envuelve el Input de shadcn/ui
 * con soporte para data-testid para tests automatizados.
 * 
 * Cualquier variación de comportamiento o estilo debe pasarse mediante props.
 */
const Input = React.forwardRef<HTMLInputElement, InputProps>(
  ({ className, type, "data-testid": dataTestId, ...props }, ref) => {
    // Generar data-testid automático si no se proporciona
    // Basado en type y name/id si es posible
    const testId = dataTestId || 
      `shared-input-${type || "text"}-${props.name || props.id || "default"}`;

    return (
      <ShadcnInput
        ref={ref}
        type={type}
        className={cn(className)}
        data-testid={testId}
        {...props}
      />
    );
  }
);
Input.displayName = "SharedInput";

export { Input };
