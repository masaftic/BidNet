#!/bin/bash

# Ensure a migration name is provided
if [ -z "$1" ]; then
    echo "Usage: $0 <MigrationName>"
    exit 1
fi


MIGRATION_NAME=$1
STARTUP_PROJECT="src/BidNet/"
PROJECT="src/BidNet/"
MIGRATIONS_PATH="Data/Persistence/Migrations"

# Run the EF Core migration command
dotnet ef migrations add "$MIGRATION_NAME" \
    --project "$PROJECT" \
    --startup-project "$STARTUP_PROJECT" \
    --output-dir "$MIGRATIONS_PATH" # --verbose

# Check if migration was successful
if [ $? -eq 0 ]; then
    echo "Migration '$MIGRATION_NAME' added successfully in '$MIGRATIONS_PATH'."
else
    echo "Error adding migration."
fi