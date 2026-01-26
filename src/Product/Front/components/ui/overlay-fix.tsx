"use client";

import { useEffect } from "react";

/**
 * Componente de seguridad para prevenir overlays bloqueantes
 * Verifica y limpia cualquier overlay que pueda estar bloqueando la interacción
 */
export function OverlayFix() {
  useEffect(() => {
    // Verificar si hay overlays bloqueantes al montar y periódicamente
    const checkAndFixOverlays = () => {
      // Verificar si el body tiene overflow hidden (indicador de Dialog abierto)
      const bodyOverflow = document.body.style.overflow;
      if (bodyOverflow === "hidden") {
        // Verificar si realmente hay un Dialog abierto
        const openDialogs = document.querySelectorAll('[role="dialog"][aria-modal="true"]');
        const visibleDialogs = Array.from(openDialogs).filter((dialog) => {
          const style = window.getComputedStyle(dialog);
          return style.display !== "none" && style.visibility !== "hidden" && style.opacity !== "0";
        });

        // Si no hay Dialogs visibles pero el body está bloqueado, liberarlo
        if (visibleDialogs.length === 0) {
          console.warn("OverlayFix: Encontrado body.overflow='hidden' sin Dialog visible, restaurando...");
          document.body.style.overflow = "unset";
        }
      }

      // Verificar si hay overlays con bg-black/50 que puedan estar bloqueando sin Dialog padre
      // CRÍTICO: Buscar TODOS los overlays, incluso si no tienen las clases exactas
      const allOverlays = document.querySelectorAll('[class*="bg-black"], [class*="backdrop"], .fixed.inset-0');
      allOverlays.forEach((overlay) => {
        const style = window.getComputedStyle(overlay);
        const htmlOverlay = overlay as HTMLElement;
        
        // Verificar si tiene posición fixed, z-index alto y ocupa toda la pantalla
        const isFixed = style.position === 'fixed';
        const hasHighZIndex = parseInt(style.zIndex || '0') >= 50;
        const rect = htmlOverlay.getBoundingClientRect();
        const isFullScreen = rect.width >= window.innerWidth * 0.9 && rect.height >= window.innerHeight * 0.9;
        const isVisible = style.display !== "none" && style.visibility !== "hidden" && style.opacity !== "0";
        
        // Si es un overlay de pantalla completa visible
        if (isFixed && hasHighZIndex && isFullScreen && isVisible) {
          const parent = overlay.parentElement;
          
          // Verificar si tiene un Dialog padre o es parte del sidebar móvil
          const hasDialogParent = parent?.closest('[role="dialog"]') || overlay.closest('[role="dialog"]');
          const isSidebarOverlay = parent?.classList.contains('lg:hidden') || overlay.closest('.lg\\:hidden');
          const hasBgBlack = style.backgroundColor && (
            style.backgroundColor.includes('rgba(0, 0, 0') || 
            style.backgroundColor.includes('rgb(0, 0, 0)') ||
            overlay.className.includes('bg-black')
          );
          
          // Si es un overlay oscuro de pantalla completa sin Dialog ni sidebar padre, está bloqueando
          if (hasBgBlack && !hasDialogParent && !isSidebarOverlay) {
            console.error("OverlayFix: ⚠️ OVERLAY BLOQUEANTE DETECTADO - Eliminando overlay huérfano", overlay);
            // Ocultar el overlay huérfano inmediatamente
            htmlOverlay.style.display = "none";
            htmlOverlay.style.visibility = "hidden";
            htmlOverlay.style.opacity = "0";
            htmlOverlay.style.pointerEvents = "none";
          }
        }
      });

      // Verificar si hay elementos fixed con z-50 que puedan estar bloqueando sin propósito
      const fixedElements = Array.from(document.querySelectorAll('*')).filter((el) => {
        const style = window.getComputedStyle(el);
        return style.position === 'fixed' && 
               parseInt(style.zIndex || '0') >= 50 &&
               style.display !== 'none';
      });

      fixedElements.forEach((el) => {
        // Si es un overlay de Dialog o sidebar, está bien
        const isDialogOverlay = el.classList.contains('bg-black') || el.getAttribute('aria-hidden') === 'true';
        const isSidebarOverlay = el.closest('.lg\\:hidden') || el.closest('[class*="sidebar"]');
        
        if (!isDialogOverlay && !isSidebarOverlay && el instanceof HTMLElement) {
          // Verificar si realmente debería estar visible
          const rect = el.getBoundingClientRect();
          if (rect.width === window.innerWidth && rect.height === window.innerHeight) {
            // Es un overlay de pantalla completa que no debería estar ahí
            console.warn("OverlayFix: Overlay de pantalla completa bloqueante detectado, verificando...", el);
          }
        }
      });
    };

    // Ejecutar inmediatamente al montar
    checkAndFixOverlays();

    // Ejecutar periódicamente como medida de seguridad (cada 2 segundos)
    const intervalId = setInterval(checkAndFixOverlays, 2000);

    // Cleanup al desmontar
    return () => {
      clearInterval(intervalId);
      // Asegurar que el body no quede bloqueado al desmontar
      document.body.style.overflow = "unset";
    };
  }, []);

  return null; // Componente sin UI
}
