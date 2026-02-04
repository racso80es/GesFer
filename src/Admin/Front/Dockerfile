FROM node:20-alpine AS base

# Install dependencies only when needed
FROM base AS deps
RUN apk add --no-cache libc6-compat
WORKDIR /app

# Copy only package files to install dependencies
COPY src/Admin/Front/package.json src/Admin/Front/package-lock.json* ./src/Admin/Front/

# Install dependencies
WORKDIR /app/src/Admin/Front
# Aumentar timeout de npm a 60 s
RUN npm ci --fetch-timeout=60000

# Rebuild the source code only when needed
FROM base AS builder
WORKDIR /app
COPY src/Admin/Front/ ./src/Admin/Front/
COPY src/Shared/Front/ ./src/Shared/Front/
WORKDIR /app/src/Admin/Front
# Place dependencies at /app/node_modules so shared components (in /app/src/Shared/Front)
# can resolve external modules during Next.js build
COPY --from=deps /app/src/Admin/Front/node_modules /app/node_modules

ENV NEXT_TELEMETRY_DISABLED 1

RUN npm run build

# Production image, copy all the files and run next
FROM base AS runner
WORKDIR /app

ENV NODE_ENV production
ENV NEXT_TELEMETRY_DISABLED 1

RUN addgroup --system --gid 1001 nodejs
RUN adduser --system --uid 1001 nextjs

# Copy public folder
COPY --from=builder /app/src/Admin/Front/public ./public

# Set permissions
RUN mkdir .next
RUN chown nextjs:nodejs .next

# Copy standalone build
COPY --from=builder --chown=nextjs:nodejs /app/src/Admin/Front/.next/standalone ./
COPY --from=builder --chown=nextjs:nodejs /app/src/Admin/Front/.next/static ./.next/static

USER nextjs

EXPOSE 3001

ENV PORT 3001
ENV HOSTNAME "0.0.0.0"

CMD ["node", "server.js"]
