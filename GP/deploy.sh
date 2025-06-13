#!/bin/bash

# Course Generation API Deployment Script
# This script sets up the complete environment for the Course Generation API

echo "Setting up Course Generation API..."

# Create project directory structure
echo "Creating directory structure..."
mkdir -p course_outputs
mkdir -p logs
mkdir -p uploads

# Install Python dependencies
echo "Installing Python dependencies..."
pip3 install -r requirements.txt

# Initialize database
echo "Initializing database..."
python3 database_setup.py

# Run tests to verify setup
echo "Running system tests..."
python3 main.py &
SERVER_PID=$!

# Wait for server to start
sleep 5

# Run tests
python3 test_api.py

# Stop test server
kill $SERVER_PID

echo "Setup complete! To start the server, run: python3 main.py"
echo "API will be available at: http://localhost:8000"
echo "API documentation at: http://localhost:8000/docs"

