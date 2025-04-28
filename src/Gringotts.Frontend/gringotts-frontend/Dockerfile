# -----------------------------------
# Build Stage
# -----------------------------------
FROM node:18-alpine AS build
WORKDIR /app

# Install dependencies
COPY package*.json ./
RUN npm ci

# Copy source code and build
COPY . .
RUN npm run build -- --configuration production

# -----------------------------------
# Serve Stage
# -----------------------------------
FROM nginx:1.23-alpine
WORKDIR /usr/share/nginx/html

# Clean default Nginx files
RUN rm -rf ./*

# Copy license and prerender data not needed now but can be useful later
COPY --from=build /app/dist/gringotts-frontend/3rdpartylicenses.txt     /usr/share/nginx/html/
COPY --from=build /app/dist/gringotts-frontend/prerendered-routes.json  /usr/share/nginx/html/

# Copy the browser build output into the web root
COPY --from=build /app/dist/gringotts-frontend/browser/               /usr/share/nginx/html/

# Expose HTTP port
EXPOSE 80

# Start Nginx in the foreground
CMD ["nginx", "-g", "daemon off;"]
