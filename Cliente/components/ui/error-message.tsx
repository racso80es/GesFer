import { AlertCircle } from "lucide-react";
import { cn } from "@/lib/utils/cn";

interface ErrorMessageProps {
  message: string;
  className?: string;
  'data-testid'?: string;
}

export function ErrorMessage({ message, className, 'data-testid': testId }: ErrorMessageProps) {
  return (
    <div
      className={cn(
        "flex items-center gap-2 rounded-md bg-destructive/10 p-3 text-sm text-destructive",
        className
      )}
      data-testid={testId}
    >
      <AlertCircle className="h-4 w-4" />
      <span>{message}</span>
    </div>
  );
}

