"use client";

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@shared/components/ui/card";
import { FileText } from "lucide-react";

export default function LogsPage() {
  return (
    <div className="container mx-auto py-10">
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <FileText className="h-5 w-5" />
            Logs
          </CardTitle>
          <CardDescription>
            Vista de registros del sistema. (En desarrollo)
          </CardDescription>
        </CardHeader>
        <CardContent>
          <p className="text-muted-foreground text-sm">
            Los logs administrativos se exponen aquí cuando la funcionalidad esté implementada.
          </p>
        </CardContent>
      </Card>
    </div>
  );
}
