#!/bin/bash

echo "Building Docx JA Translator..."

# Restore dependencies
echo "Restoring dependencies..."
dotnet restore

# Build the project
echo "Building project..."
dotnet build --configuration Release

# Run tests
echo "Running tests..."
dotnet test --configuration Release

# Publish as single file
echo "Publishing as single file..."
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true

echo "Build completed successfully!"
echo "Executable available at: bin/Release/net8.0/osx-x64/publish/DocxJaTranslator"
