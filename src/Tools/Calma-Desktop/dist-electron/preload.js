"use strict";
const electron = require("electron");
electron.contextBridge.exposeInMainWorld("calmaAPI", {
  startSequence: (seqId) => electron.ipcRenderer.invoke("start-sequence", seqId),
  stopAll: () => electron.ipcRenderer.invoke("stop-all"),
  getSettings: () => electron.ipcRenderer.invoke("get-settings"),
  updateSettings: (key, value) => electron.ipcRenderer.invoke("update-settings", key, value),
  // Quick Actions
  runAudit: () => electron.ipcRenderer.invoke("run-audit"),
  clearCache: () => electron.ipcRenderer.invoke("clear-cache"),
  syncSpec: () => electron.ipcRenderer.invoke("sync-spec"),
  // Events
  onStatusChange: (callback) => {
    const subscription = (_, value) => callback(value);
    electron.ipcRenderer.on("status-change", subscription);
    return () => electron.ipcRenderer.removeListener("status-change", subscription);
  }
});
