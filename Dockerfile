# ===========================
#       BUILD STAGE
# ===========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY Notes.sln .
COPY Notes.API/*.csproj Notes.API/

# Restore dependencies
RUN dotnet restore

# Copy all source code
COPY . .

# Build and publish
RUN dotnet publish Notes.API -c Release -o /app/publish


# ===========================
#       RUNTIME STAGE
# ===========================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy published output from build stage
COPY --from=build /app/publish .

# Configure ASP.NET Core port
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Entry point
ENTRYPOINT ["dotnet", "Notes.API.dll"]