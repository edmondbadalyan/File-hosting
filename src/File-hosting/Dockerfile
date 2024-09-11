# Use the official .NET SDK image to build and apply migrations
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the solution file
COPY src/File-hosting/*.sln ./

# Copy the project files
COPY src/File-hosting/Test_Server/Test_Server.csproj ./Test_Server/
COPY src/File-hosting/Test_Console/Test_Console.csproj ./Test_Console/
COPY src/File-hosting/TCPLib/TCPLib.csproj ./TCPLib/
COPY src/File-hosting/HostingLib/HostingLib.csproj ./HostingLib/

# Restore dependencies
RUN dotnet restore

# Install EF Core tools
RUN dotnet tool install --global dotnet-ef

# Ensure the dotnet tools are available in the PATH
ENV PATH="$PATH:/root/.dotnet/tools"

# Copy the rest of the files
COPY src/File-hosting/ ./File-hosting/

# Apply migrations
#WORKDIR /app/FileHosting/HostingLib
#RUN dotnet-ef database update --connection "Server=mssql;Database=Hosting_TestDb;User Id=sa;Password=StrongPassword123!;TrustServerCertificate=True;"

# Publish the application
WORKDIR /app/File-hosting/Test_Server
RUN dotnet publish -c Release -o /app/out

# Install dotnet-ef tool
#RUN dotnet tool install --global dotnet-ef

# Set PATH to include the .NET tools
ENV PATH="${PATH}:/root/.dotnet/tools"

# Use the official .NET runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

RUN apt-get update && apt-get install -y bash

WORKDIR /app
COPY --from=build /app/out .

# Expose the TCP port your server is listening on
EXPOSE 2024

ENTRYPOINT ["dotnet", "Test_Server.dll"]

