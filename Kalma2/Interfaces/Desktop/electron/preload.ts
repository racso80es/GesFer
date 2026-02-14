import { ipcRenderer, contextBridge } from 'electron';

contextBridge.exposeInMainWorld('calmaAPI', {
  startSequence: (seqId: number) => ipcRenderer.invoke('start-sequence', seqId),
  stopAll: () => ipcRenderer.invoke('stop-all'),
  getSettings: () => ipcRenderer.invoke('get-settings'),
  updateSettings: (key: string, value: any) => ipcRenderer.invoke('update-settings', key, value),

  // Quick Actions
  runAudit: (payload: any) => ipcRenderer.invoke('run-audit', payload),
  clearCache: () => ipcRenderer.invoke('clear-cache'),
  syncSpec: () => ipcRenderer.invoke('sync-spec'),
  checkStatus: (url: string) => ipcRenderer.invoke('check-status', url),

  // Events
  onStatusChange: (callback: (status: any) => void) => {
    const subscription = (_: any, value: any) => callback(value);
    ipcRenderer.on('status-change', subscription);
    return () => ipcRenderer.removeListener('status-change', subscription);
  },
});
