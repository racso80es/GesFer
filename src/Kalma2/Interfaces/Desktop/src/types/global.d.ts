export interface CalmaAPI {
  startSequence: (seqId: number) => Promise<void>;
  stopAll: () => Promise<void>;
  getSettings: () => Promise<any>; // Todo: strict type
  updateSettings: (key: string, value: unknown) => Promise<void>;
  runAudit: () => Promise<void>;
  clearCache: () => Promise<void>;
  syncSpec: () => Promise<void>;
  onStatusChange: (callback: (status: unknown) => void) => () => void;
}

declare global {
  interface Window {
    calmaAPI: CalmaAPI;
  }
}
