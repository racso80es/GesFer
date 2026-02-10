/// <reference types="vite/client" />

interface CalmaAPI {
  startSequence: (seqId: number) => Promise<any>;
  stopAll: () => Promise<any>;
  getSettings: () => Promise<any>;
  updateSettings: (key: string, value: any) => Promise<any>;
  runAudit: () => Promise<any>;
  clearCache: () => Promise<any>;
  syncSpec: () => Promise<any>;
  onStatusChange: (callback: (status: any) => void) => () => void;
}

interface Window {
  calmaAPI: CalmaAPI;
}
