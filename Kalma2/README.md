# Kalma2

Kalma2 is the next-generation architecture for the GesFer ecosystem, designed with a strict separation between Core Logic and Interfaces ("Senses").

## Architecture

### 1. Core (`Kalma2/Core`)
The central nervous system. Contains business logic, the "Conscience" (Judge & Auditor), and domain rules. It is agnostic to the interface.

### 2. Interfaces (`Kalma2/Interfaces`)
The presentation layers.
- **Desktop**: An Electron-based application that serves as the primary "body" for the Core in a desktop environment.

### 3. Projects (MCP) (`Kalma2/Projects`)
Kalma2 operates as a Master Control Program (MCP). It does not "contain" the projects it manages but "orchestrates" them.
- **Configuration**: Each managed project (e.g., `GesFer`) has its own folder in `Projects/` containing `initial.json` and `services.json`.

## Getting Started

To run the Desktop interface:
1. Navigate to `Kalma2/Interfaces/Desktop`.
2. Run `npm install`.
3. Run `npm run dev` for development or `npm run build` for production.

## Golden Rules
Refer to [CONSTITUTION.md](./CONSTITUTION.md) for the fundamental laws governing this architecture.
