interface ErrorMessageProps {
  message: string;
}

export function ErrorMessage({ message }: ErrorMessageProps) {
  return (
    <div className="rounded-md bg-destructive/15 p-3 text-sm text-destructive">
      <p>{message}</p>
    </div>
  );
}
