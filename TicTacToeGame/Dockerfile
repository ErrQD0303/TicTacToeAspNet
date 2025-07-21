# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY /*.csproj ./
RUN dotnet restore

# Copy the rest of the source code
COPY . ./
# Public the application to Linux-x64 runtime with no self-contained deployment
RUN dotnet publish -c Release -o /app/publish --runtime linux-x64 --self-contained false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Set the environment variable to Production
ENV ASPNETCORE_ENVIRONMENT=Production

# Linux Default port is usually 80, but we will expose port 8080 for this application like we defined in the appsettings.Production.json
EXPOSE 8080

# Set the entry point for the application, the command to run the application
ENTRYPOINT ["dotnet", "TicTacToeGame.dll"]