import { useState, useEffect } from 'react'
import { container, TYPES } from '../../../Core/di/container'
import { IGreetingService } from './services/GreetingService'
import { IAuditor } from '../../../Core/conscience/interfaces'

// Import Project Configuration (MCP)
import gesferConfig from '../../../Projects/GesFer/initial.json'
import gesferServices from '../../../Projects/GesFer/services.json'

interface ActionButtonProps {
  onClick: () => void
  label: string
  icon: string
  variant?: 'default' | 'danger'
  disabled?: boolean
}

function App() {
  const [greeting, setGreeting] = useState<string>('')
  // Using unknown instead of any for status until shape is defined
  const [status, setStatus] = useState<Record<string, unknown>>({})

  // MCP State
  const [auditStatus, setAuditStatus] = useState<'idle' | 'loading' | 'success' | 'error'>('idle')
  const [auditHash, setAuditHash] = useState<string>('')

  useEffect(() => {
    // DI Resolution
    const greetingService = container.get<IGreetingService>(TYPES.GreetingService)
    setGreeting(greetingService.getGreeting())

    // Status subscription
    const unsubscribe = window.calmaAPI.onStatusChange((newStatus: unknown) => {
      setStatus(newStatus as Record<string, unknown>)
    })
    return () => unsubscribe()
  }, [])

  const handleStartProduct = () => window.calmaAPI.startSequence(1)
  const handleStopAll = () => window.calmaAPI.stopAll()

  const runAudit = () => window.calmaAPI.runAudit()
  const clearCache = () => window.calmaAPI.clearCache()
  const syncSpec = () => window.calmaAPI.syncSpec()

  // Golden Action: Auditor AP Registration
  const handleIotaAudit = async () => {
    setAuditStatus('loading')
    try {
        const auditor = container.get<IAuditor>(TYPES.Auditor)
        // Hash the current project config as the "Process" data
        const hash = await auditor.registerProcess(`MCP-${gesferConfig.id.toUpperCase()}`, {
             config: gesferConfig,
             services: gesferServices,
             timestamp: Date.now()
        })
        setAuditHash(hash)
        setAuditStatus('success')
    } catch (e) {
        console.error(e)
        setAuditStatus('error')
    }
  }

  return (
    <div className="flex h-screen flex-col bg-background p-6">
      <header className="mb-8 flex items-center justify-between">
        <div>
           <h1 className="text-2xl font-bold tracking-tight text-white">Calma Desktop</h1>
           <div className="flex items-center gap-2 mt-1">
             <p className="text-sm text-green-400">{greeting}</p>
             <span className="text-gray-600">|</span>
             <span className="text-xs text-gray-400">Project: {gesferConfig.name} ({gesferConfig.version})</span>
           </div>
        </div>
        <div className="flex gap-2 items-center">
          <span className="h-2 w-2 rounded-full bg-gray-500"></span>
          <span className="text-xs text-gray-500">System Idle</span>
        </div>
      </header>

      {/* MCP Project View */}
      <section className="mb-6 p-4 rounded-xl border border-border bg-surface/50">
        <div className="flex items-center justify-between">
            <h3 className="text-sm font-semibold text-gray-300 uppercase tracking-wider">Active Project (MCP)</h3>
            <div className="flex items-center gap-2">
                {auditStatus === 'success' && (
                    <div className="flex items-center gap-2 text-green-400 bg-green-900/20 px-3 py-1 rounded-full border border-green-900/50">
                        <span className="text-lg">✅</span>
                        <span className="text-xs font-mono truncate max-w-[150px]" title={auditHash}>{auditHash}</span>
                        <span className="text-xs font-bold">VERIFIED</span>
                    </div>
                )}
                <button
                    onClick={handleIotaAudit}
                    disabled={auditStatus === 'loading'}
                    className={`px-3 py-1 rounded text-xs font-medium transition flex items-center gap-2
                        ${auditStatus === 'loading' ? 'bg-yellow-500/20 text-yellow-500 cursor-wait' : 'bg-blue-500/20 text-blue-400 hover:bg-blue-500/30'}
                    `}
                >
                    {auditStatus === 'loading' ? 'Registering on IOTA...' : 'Audit Process (IOTA)'}
                </button>
            </div>
        </div>
      </section>

      <main className="grid grid-cols-2 gap-6">
        {/* Iterate over services from JSON could be done here, but keeping hardcoded domains for now to match UI design */}

        {/* Product Domain */}
        <div className="rounded-xl border border-border bg-surface p-6">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-lg font-semibold text-primary">Product Domain</h2>
            <button
              onClick={handleStartProduct}
              className="px-4 py-2 bg-primary/20 text-primary rounded-lg hover:bg-primary/30 transition text-sm font-medium"
            >
              Start Sequence
            </button>
          </div>
          <div className="space-y-3">
             <div className="flex justify-between items-center text-sm text-gray-400 p-2 rounded bg-background/50">
                <span>API (5000)</span>
                <span className="text-red-400 text-xs font-mono">OFFLINE</span>
             </div>
             <div className="flex justify-between items-center text-sm text-gray-400 p-2 rounded bg-background/50">
                <span>Front (3000)</span>
                <span className="text-red-400 text-xs font-mono">OFFLINE</span>
             </div>
          </div>
        </div>

        {/* Admin Domain */}
        <div className="rounded-xl border border-border bg-surface p-6">
           <div className="flex items-center justify-between mb-4">
            <h2 className="text-lg font-semibold text-secondary">Admin Domain</h2>
            <button
              className="px-4 py-2 bg-secondary/20 text-secondary rounded-lg hover:bg-secondary/30 transition text-sm font-medium"
            >
              Start Sequence
            </button>
          </div>
           <div className="space-y-3">
             <div className="flex justify-between items-center text-sm text-gray-400 p-2 rounded bg-background/50">
                <span>API (5010)</span>
                <span className="text-red-400 text-xs font-mono">OFFLINE</span>
             </div>
             <div className="flex justify-between items-center text-sm text-gray-400 p-2 rounded bg-background/50">
                <span>Front (3001)</span>
                <span className="text-red-400 text-xs font-mono">OFFLINE</span>
             </div>
          </div>
        </div>
      </main>

      <div className="mt-8">
        <h3 className="mb-4 text-xs font-semibold text-gray-500 uppercase tracking-wider">Quick Actions</h3>
        <div className="grid grid-cols-4 gap-4">
          <ActionButton onClick={runAudit} label="Run Full Audit" icon="🚀" />
          <ActionButton onClick={clearCache} label="Clear Docker Cache" icon="🧹" />
          <ActionButton onClick={syncSpec} label="Sync Spec" icon="🔄" />
          <ActionButton onClick={handleStopAll} label="Stop All Services" icon="🛑" variant="danger" />
        </div>
      </div>
    </div>
  )
}

function ActionButton({ onClick, label, icon, variant = 'default', disabled }: ActionButtonProps) {
  const base = "flex flex-col items-center gap-2 px-4 py-4 rounded-xl font-medium transition-all text-sm w-full justify-center border group"
  const styles = variant === 'danger'
    ? "bg-red-500/5 border-red-500/20 text-red-500 hover:bg-red-500/10 hover:border-red-500/30"
    : "bg-surface border-border hover:bg-zinc-800 text-gray-400 hover:text-white hover:border-zinc-700"

  return (
    <button onClick={onClick} disabled={disabled} className={`${base} ${styles} ${disabled ? 'opacity-50 cursor-not-allowed' : ''}`}>
      <span className="text-2xl mb-1 opacity-80 group-hover:opacity-100 transition-opacity">{icon}</span>
      <span className="text-xs">{label}</span>
    </button>
  )
}

export default App
