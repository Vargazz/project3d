# ── Stage 1: Build Angular ───────────────────────────────────────────────────
FROM node:20-alpine AS build
WORKDIR /app

# Instalar dependências primeiro (cache eficiente)
COPY package*.json ./
RUN npm ci --silent

# Copiar código-fonte e fazer build
COPY . .
RUN npm run build -- --configuration production

# ── Stage 2: Nginx para servir o SPA ─────────────────────────────────────────
FROM nginx:alpine AS runtime

# Remover config padrão do nginx
RUN rm /etc/nginx/conf.d/default.conf

# Adicionar nossa config customizada
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copiar build do Angular
COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
