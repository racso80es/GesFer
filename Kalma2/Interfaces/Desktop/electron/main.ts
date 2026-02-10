import { app, BrowserWindow, Tray, Menu, ipcMain, nativeImage } from 'electron';
import path from 'node:path';
import Store from 'electron-store';

process.env.DIST = path.join(__dirname, '../dist');
process.env.VITE_PUBLIC = app.isPackaged ? process.env.DIST : path.join(process.env.DIST, '../public');

let win: BrowserWindow | null;
let tray: Tray | null;
let isQuitting = false;

// Store Schema
const schema = {
  lastSequence: { type: 'number', default: 0 } as const,
  autoStart: { type: 'boolean', default: false } as const,
  servicePaths: { type: 'object', default: {} } as const
};

const store = new Store({ schema });

// Icon Paths
function getIconPath(state: 'grey' | 'orange' | 'green') {
  const iconName = `icon-${state}.png`;
  return path.join(process.env.VITE_PUBLIC as string, 'tray', iconName);
}

function createWindow() {
  win = new BrowserWindow({
    width: 900,
    height: 600,
    icon: getIconPath('grey'), // Default icon for window
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      nodeIntegration: false,
      contextIsolation: true,
    },
  });

  // Test active push message to Renderer-process.
  win.webContents.on('did-finish-load', () => {
    win?.webContents.send('main-process-message', (new Date).toLocaleString());
  });

  if (process.env.VITE_DEV_SERVER_URL) {
    win.loadURL(process.env.VITE_DEV_SERVER_URL);
  } else {
    // win.loadFile('dist/index.html')
    win.loadFile(path.join(process.env.DIST as string, 'index.html'));
  }

  // Rule S+: Prevent closing, minimize to tray
  win.on('close', (event) => {
    if (!isQuitting) {
      event.preventDefault();
      win?.hide();
      return false;
    }
  });
}

function createTray() {
  const icon = nativeImage.createFromPath(getIconPath('grey'));
  tray = new Tray(icon);
  tray.setToolTip('Calma Desktop - Stopped');

  const contextMenu = Menu.buildFromTemplate([
    { label: 'Start Sequence 1 (Product)', click: () => { console.log('Start Seq 1'); } },
    { label: 'Stop All', click: () => { console.log('Stop All'); } },
    { type: 'separator' },
    { label: 'Open Dashboard', click: () => win?.show() },
    { type: 'separator' },
    {
      label: 'Quit Calma',
      click: () => {
        isQuitting = true;
        app.quit();
      }
    }
  ]);

  tray.setContextMenu(contextMenu);
  tray.on('double-click', () => win?.show());
}

app.on('window-all-closed', () => {
  // Respect Rule S+: Do nothing (stay in tray)
  // Usually this is where app.quit() goes, but we want to stay alive.
});

app.whenReady().then(() => {
  createWindow();
  createTray();

  // IPC Handlers
  ipcMain.handle('get-settings', () => store.store);
  ipcMain.handle('update-settings', (_, key, value) => {
    store.set(key, value);
    return store.store;
  });

  ipcMain.on('app-quit', () => {
    isQuitting = true;
    app.quit();
  });

  ipcMain.handle('start-sequence', (_, seqId) => {
    console.log('IPC: start-sequence', seqId);
  });

  ipcMain.handle('stop-all', () => {
    console.log('IPC: stop-all');
  });

  ipcMain.handle('run-audit', () => {
    console.log('IPC: run-audit');
  });

  ipcMain.handle('clear-cache', () => {
    console.log('IPC: clear-cache');
  });

  ipcMain.handle('sync-spec', () => {
    console.log('IPC: sync-spec');
  });
});

app.on('activate', () => {
  if (BrowserWindow.getAllWindows().length === 0) {
    createWindow();
  } else {
    win?.show();
  }
});
