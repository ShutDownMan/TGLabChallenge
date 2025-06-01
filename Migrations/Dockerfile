FROM mcr.microsoft.com/dotnet/sdk:6.0 AS base
WORKDIR /app

# Copy the necessary project files
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["API/API.csproj", "API/"]
RUN dotnet restore "Infrastructure/Infrastructure.csproj"

# Copy the full source code
COPY . .

# Ensure the working directory is set to the correct project folder
WORKDIR "/app/Infrastructure"

# Set environment variables for database connection
ENV POSTGRES_USER=${POSTGRES_USER}
ENV POSTGRES_PASSWORD=${POSTGRES_PASSWORD}
ENV POSTGRES_DB=${POSTGRES_DB}
ENV POSTGRES_HOST=${POSTGRES_HOST}
ENV POSTGRES_PORT=${POSTGRES_PORT}

# Install dotnet-ef tool and update the database
ENTRYPOINT bash -c "dotnet tool install --global dotnet-ef --version 6.0.10 && \
                    export PATH=\"$PATH:/root/.dotnet/tools\" && \
                    echo 'Environment Variables:' && printenv && \
                    dotnet ef database update"
