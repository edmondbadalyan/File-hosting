# Start from the SQL Server 2022 image
FROM mcr.microsoft.com/mssql/server:2022-latest

# Set environment variables
ENV ACCEPT_EULA=Y
ENV SA_PASSWORD=StrongPassword123!

# Switch to root user
USER root

# Install necessary packages
#RUN apt-get update && \
#    apt-get install -y curl gnupg

# Add the Microsoft repository
#RUN curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - && \
#    curl https://packages.microsoft.com/config/ubuntu/22.04/prod.list | tee /etc/apt/sources.list.d/mssql-release.list

# Install the sqlcmd utility
#RUN apt-get update && \
#    ACCEPT_EULA=Y apt-get install -y mssql-tools unixodbc-dev

# Set the PATH environment variable to include sqlcmd
#ENV PATH="${PATH}:/opt/mssql-tools/bin"

# Expose SQL Server port
EXPOSE 1433

# Switch back to the mssql user
USER mssql

