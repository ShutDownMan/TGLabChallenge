# Use the official Node.js image as the base image
FROM node:20-alpine AS builder

# Set the working directory inside the container
WORKDIR /app

# Copy package.json to the working directory
COPY WebDashboard/package.json ./

# Instead of npm ci (which strictly requires lock file to match),
# use npm install to update the dependencies
RUN npm install

# Copy the rest of the application source files
COPY WebDashboard/src ./src
COPY WebDashboard/public ./public

# Create a production build using environment variables from Docker
ARG REACT_APP_API_URL
ENV REACT_APP_API_URL=${REACT_APP_API_URL:-http://localhost:8080}

# Build the application
RUN npm run build

# Use a lightweight web server to serve the built files
FROM nginx:alpine

# Copy built files from builder stage to nginx
COPY --from=builder /app/build /usr/share/nginx/html

# Copy custom nginx config
COPY WebDashboard/nginx.conf /etc/nginx/conf.d/default.conf

# Expose the port the application runs on
EXPOSE 3000

# Start the web server
CMD ["nginx", "-g", "daemon off;"]
