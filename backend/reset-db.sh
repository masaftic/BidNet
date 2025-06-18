#!/bin/bash

# Stop and remove the existing database container
docker-compose down

# Remove the associated volume
docker volume rm bidnet_db_data

# Start the database container again
docker-compose up -d bidnet-db