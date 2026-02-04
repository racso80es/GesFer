FROM node:20-alpine AS base

# Install dependencies only when needed
FROM base AS deps
RUN apk add --no-cache libc6-compat
WORKDIR /app

# Copy only package files to install dependencies
COPY src/Product/Front/package.json src/Product/Front/package-lock.json* ./src/Product/Front/

# Install dependencies
WORKDIR /app/src/Product/Front
# Aumentar timeout de npm a 60 s
RUN npm ci --fetch-timeout=60000

# Rebuild the source code only when needed
FROM base AS builder
WORKDIR /app
COPY src/Product/Front/ ./src/Product/Front/
COPY src/Shared/Front/ ./src/Shared/Front/
WORKDIR /app/src/Product/Front
# Place dependencies at /app/node_modules so shared components (in /app/src/Shared/Front)
# can resolve external modules during Next.js build
COPY --from=deps /app/src/Product/Front/node_modules /app/node_modules

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
COPY --from=builder /app/src/Product/Front/public ./public

# Set permissions
RUN mkdir .next
RUN chown nextjs:nodejs .next

# Copy standalone build
# The standalone folder will be at .next/standalone
# It typically contains the project structure.
# We copy it to /app
COPY --from=builder --chown=nextjs:nodejs /app/src/Product/Front/.next/standalone ./
COPY --from=builder --chown=nextjs:nodejs /app/src/Product/Front/.next/static ./.next/static

USER nextjs

EXPOSE 3000

ENV PORT 3000
ENV HOSTNAME "0.0.0.0"

# Note: If standalone build preserves structure, server.js might be nested.
# Common behavior: server.js is at root of standalone output.
CMD ["node", "server.js"]
