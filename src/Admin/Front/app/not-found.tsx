import Link from "next/link";
import { Button } from "@/components/ui/button";

export default function NotFound() {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center gap-4 p-6">
      <h1 className="text-2xl font-bold">Página no encontrada</h1>
      <p className="text-muted-foreground">La ruta solicitada no existe.</p>
      <Link href="/dashboard">
        <Button>Ir al Dashboard</Button>
      </Link>
    </div>
  );
}
