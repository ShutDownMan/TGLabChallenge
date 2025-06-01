#!/bin/sh

# Set ASPNETCORE_Kestrel__Certificates__Default__Password at runtime
export ASPNETCORE_Kestrel__Certificates__Default__Password=${CERTIFICATE_PASSWORD}

# Print environment variables for debugging (optional)
printenv

# Start the application
exec dotnet API.dll
