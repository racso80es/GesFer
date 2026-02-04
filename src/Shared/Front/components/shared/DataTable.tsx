"use client";

import * as React from "react";
import { cn } from "../../lib/utils/cn";
import { Button } from "./Button";

export interface DataTableColumn<T> {
  /**
   * Clave única de la columna
   */
  key: string;
  
  /**
   * Título de la columna
   */
  title: string;
  
  /**
   * Función para renderizar el contenido de la celda
   */
  render: (item: T, index: number) => React.ReactNode;
  
  /**
   * Clase CSS adicional para la columna
   */
  className?: string;
  
  /**
   * Si la columna es ordenable
   */
  sortable?: boolean;
}

export interface DataTableProps<T> {
  /**
   * Datos a mostrar en la tabla
   */
  data: T[];
  
  /**
   * Configuración de columnas
   */
  columns: DataTableColumn<T>[];
  
  /**
   * Función para obtener la clave única de cada fila
   */
  getRowKey: (item: T, index: number) => string | number;
  
  /**
   * Si la tabla está cargando
   */
  loading?: boolean;
  
  /**
   * Mensaje cuando no hay datos
   */
  emptyMessage?: string;
  
  /**
   * Clase CSS adicional para la tabla
   */
  className?: string;
  
  /**
   * Clase CSS adicional para el contenedor
   */
  containerClassName?: string;
  
  /**
   * Si se muestra el encabezado
   */
  showHeader?: boolean;
  
  /**
   * Configuración de paginación
   */
  pagination?: {
    currentPage: number;
    totalPages: number;
    hasPreviousPage: boolean;
    hasNextPage: boolean;
    onPageChange: (page: number) => void;
    previousText?: string;
    nextText?: string;
    pageText?: string;
  };
  
  /**
   * Función para renderizar filas expandibles (opcional)
   */
  renderExpandedRow?: (item: T) => React.ReactNode;
  
  /**
   * Función para obtener el estado de expansión de una fila
   */
  isRowExpanded?: (item: T) => boolean;
  
  /**
   * Identificador para tests (data-testid)
   * Nomenclatura: shared-datatable-[acción]
   * Ejemplo: shared-datatable-logs, shared-datatable-users
   */
  "data-testid"?: string;
}

/**
 * Componente DataTable compartido
 * 
 * Componente puro e inmutable para mostrar datos en formato tabla
 * con soporte para data-testid para tests automatizados.
 * 
 * Cualquier variación de comportamiento o estilo debe pasarse mediante props.
 */
export function DataTable<T>({
  data,
  columns,
  getRowKey,
  loading = false,
  emptyMessage = "No hay datos disponibles",
  className,
  containerClassName,
  showHeader = true,
  pagination,
  renderExpandedRow,
  isRowExpanded,
  "data-testid": dataTestId,
}: DataTableProps<T>) {
  const testId = dataTestId || "shared-datatable";

  if (loading) {
    return (
      <div
        className={cn("flex justify-center py-12", containerClassName)}
        data-testid={`${testId}-loading`}
      >
        <p className="text-muted-foreground">Cargando...</p>
      </div>
    );
  }

  if (data.length === 0) {
    return (
      <div
        className={cn("flex flex-col items-center justify-center py-12", containerClassName)}
        data-testid={`${testId}-empty`}
      >
        <p className="text-muted-foreground">{emptyMessage}</p>
      </div>
    );
  }

  return (
    <div
      className={cn("space-y-4", containerClassName)}
      data-testid={testId}
    >
      <div className="overflow-x-auto">
        <table className={cn("w-full", className)}>
          {showHeader && (
            <thead>
              <tr className="border-b">
                {columns.map((column) => (
                  <th
                    key={column.key}
                    className={cn("text-left p-2", column.className)}
                    data-testid={`${testId}-header-${column.key}`}
                  >
                    {column.title}
                  </th>
                ))}
              </tr>
            </thead>
          )}
          <tbody>
            {data.map((item, index) => {
              const rowKey = getRowKey(item, index);
              const isExpanded = isRowExpanded ? isRowExpanded(item) : false;
              
              return (
                <React.Fragment key={rowKey}>
                  <tr
                    className="border-b hover:bg-muted/50"
                    data-testid={`${testId}-row-${rowKey}`}
                  >
                    {columns.map((column) => (
                      <td
                        key={column.key}
                        className={cn("p-2", column.className)}
                        data-testid={`${testId}-cell-${rowKey}-${column.key}`}
                      >
                        {column.render(item, index)}
                      </td>
                    ))}
                  </tr>
                  {isExpanded && renderExpandedRow && (
                    <tr
                      className="border-b bg-muted/30"
                      data-testid={`${testId}-expanded-row-${rowKey}`}
                    >
                      <td colSpan={columns.length} className="p-4">
                        {renderExpandedRow(item)}
                      </td>
                    </tr>
                  )}
                </React.Fragment>
              );
            })}
          </tbody>
        </table>
      </div>
      
      {pagination && pagination.totalPages > 1 && (
        <div
          className="flex items-center justify-between mt-4"
          data-testid={`${testId}-pagination`}
        >
          <Button
            variant="outline"
            onClick={() => pagination.onPageChange(pagination.currentPage - 1)}
            disabled={!pagination.hasPreviousPage}
            data-testid={`${testId}-pagination-previous`}
          >
            {pagination.previousText || "Anterior"}
          </Button>
          <span className="text-sm text-muted-foreground">
            {pagination.pageText || "Página"} {pagination.currentPage} de {pagination.totalPages}
          </span>
          <Button
            variant="outline"
            onClick={() => pagination.onPageChange(pagination.currentPage + 1)}
            disabled={!pagination.hasNextPage}
            data-testid={`${testId}-pagination-next`}
          >
            {pagination.nextText || "Siguiente"}
          </Button>
        </div>
      )}
    </div>
  );
}
