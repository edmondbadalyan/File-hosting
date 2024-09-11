#!/bin/bash
echo "Applying EF Core migrations..."
dotnet ef database update
echo "Migrations applied."
