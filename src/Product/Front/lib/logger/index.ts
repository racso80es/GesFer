import pino from 'pino';

// Interface para propiedades del log para evitar el uso de any
interface LogProperties {
  [key: string]: unknown;
  err?: { stack?: string };
  error?: { stack?: string };
  source?: string;
}

// Determinar el nivel mínimo según el entorno
const getMinLevel = (): pino.Level => {
  const env = process.env.NODE_ENV || 'development';
  
  // En producción solo info y superiores, en desarrollo debug
  return env === 'production' ? 'info' : 'debug';
};

// Transportador personalizado para enviar logs al backend
class TelemetryTransport {
  private apiUrl: string;
  private enabled: boolean;
  private queue: Array<{ level: number; message: string; exception?: string; properties?: LogProperties; source?: string; timestamp?: Date; clientInfo?: Record<string, unknown> }> = [];
  private flushInterval: NodeJS.Timeout | null = null;
  private readonly FLUSH_INTERVAL_MS = 5000; // Flush cada 5 segundos
  private readonly MAX_QUEUE_SIZE = 100;

  constructor() {
    // Obtener la URL de la API desde la configuración
    if (typeof window !== 'undefined') {
      this.apiUrl = process.env.NEXT_PUBLIC_API_URL || 'https://localhost:5001';
    } else {
      this.apiUrl = process.env.API_URL || 'https://localhost:5001';
    }
    
    // Solo habilitar en producción o si está explícitamente configurado
    this.enabled = process.env.NODE_ENV === 'production' || process.env.NEXT_PUBLIC_ENABLE_TELEMETRY === 'true';
    
    if (this.enabled) {
      this.startFlushInterval();
    }
  }

  private startFlushInterval() {
    if (this.flushInterval) {
      clearInterval(this.flushInterval);
    }
    
    this.flushInterval = setInterval(() => {
      this.flush();
    }, this.FLUSH_INTERVAL_MS);
  }

  async send(log: {
    level: number;
    message: string;
    exception?: string;
    properties?: LogProperties;
    source?: string;
    timestamp?: Date;
    clientInfo?: Record<string, unknown>;
  }) {
    if (!this.enabled) {
      return;
    }

    // Solo enviar errores y warnings
    if (log.level < 40) { // 40 = warn, 50 = error, 60 = fatal
      return;
    }

    // Agregar a la cola
    this.queue.push(log);

    // Si la cola está llena, hacer flush inmediato
    if (this.queue.length >= this.MAX_QUEUE_SIZE) {
      await this.flush();
    }
  }

  private async flush() {
    if (this.queue.length === 0) {
      return;
    }

    const logsToSend = [...this.queue];
    this.queue = [];

    // Enviar logs uno por uno (se puede optimizar para batch en el futuro)
    for (const log of logsToSend) {
      try {
        await fetch(`${this.apiUrl}/api/telemetry/logs`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify(log),
          // No bloquear si falla
          signal: AbortSignal.timeout(5000), // Timeout de 5 segundos
        });
      } catch (error) {
        // Si falla, reintentar más tarde (no bloquear la aplicación)
        // Solo loguear en desarrollo
        if (process.env.NODE_ENV === 'development') {
          console.error('Error al enviar log de telemetría:', error);
        }
        // Re-agregar a la cola para reintentar (solo si no es un error de timeout)
        if (!(error instanceof Error && error.name === 'AbortError')) {
          this.queue.push(log);
        }
      }
    }
  }

  destroy() {
    if (this.flushInterval) {
      clearInterval(this.flushInterval);
      this.flushInterval = null;
    }
    // Flush final
    this.flush();
  }
}

// Instancia del transportador
const telemetryTransport = new TelemetryTransport();

// Hook personalizado para interceptar logs
const telemetryHook = {
  logMethod(inputArgs: unknown[], method: (...args: unknown[]) => void, level: number) {
    const [obj, msg, ...args] = inputArgs;
    
    // Extraer información del log
    const message = typeof msg === 'string' ? msg : JSON.stringify(msg);
    const properties = typeof obj === 'object' && obj !== null ? (obj as LogProperties) : {};
    const exception = properties.err?.stack || properties.error?.stack || undefined;
    
    // Obtener información del cliente
    const clientInfo: Record<string, unknown> = {
      userAgent: typeof window !== 'undefined' ? window.navigator.userAgent : undefined,
      url: typeof window !== 'undefined' ? window.location.href : undefined,
      timestamp: new Date().toISOString(),
    };

    // Enviar al transportador (no bloqueante)
    telemetryTransport.send({
      level,
      message,
      exception,
      properties,
      source: properties.source || 'GesFer-Client',
      timestamp: new Date(),
      clientInfo,
    }).catch(() => {
      // Ignorar errores silenciosamente
    });

    // Continuar con el log normal
    return method.apply(this, inputArgs);
  },
};

// Configuración base del logger
const loggerConfig: pino.LoggerOptions = {
  level: getMinLevel(),
  base: {
    env: process.env.NODE_ENV || 'development',
    app: 'GesFer-Client',
  },
  timestamp: pino.stdTimeFunctions.isoTime,
  formatters: {
    level: (label) => {
      return { level: label };
    },
  },
  hooks: {
    logMethod: telemetryHook.logMethod as unknown as NonNullable<pino.LoggerOptions['hooks']>['logMethod'],
  },
};

// Crear el logger base
const logger = pino(loggerConfig);

// Exportar el logger
export default logger;

// Exportar función para limpiar recursos
export const destroyLogger = () => {
  telemetryTransport.destroy();
};
