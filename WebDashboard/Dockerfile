# Use the official Node.js image as the base image
FROM node:24-alpine AS builder

# Set the working directory inside the container
WORKDIR /app

# Copy package.json and package-lock.json to the working directory
COPY package.json package-lock.json ./

# Install dependencies using npm ci for a clean install
RUN npm ci

# Copy the rest of the application source files
COPY ./WebDashboard ./

# Build the application
RUN npm run build

# Use a lightweight web server to serve the built files
FROM nginx:alpine

COPY --from=builder /app/build /usr/share/nginx/html

# Expose the port the application runs on
EXPOSE 80

# Start the web server
CMD ["nginx", "-g", "daemon off;"]
