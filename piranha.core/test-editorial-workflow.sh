#!/bin/bash

echo "======================================"
echo "🧪 Testing Piranha Editorial Workflow"
echo "======================================"

echo ""
echo "1️⃣ Building the solution..."
dotnet build

if [ $? -eq 0 ]; then
    echo "✅ Build successful!"
else
    echo "❌ Build failed!"
    exit 1
fi

echo ""
echo "2️⃣ Running unit tests..."
dotnet test test/Piranha.EditorialWorkflow.Tests/

if [ $? -eq 0 ]; then
    echo "✅ Tests passed!"
else
    echo "❌ Tests failed!"
    exit 1
fi

echo ""
echo "3️⃣ Running example..."
dotnet run --project examples/EditorialWorkflowExample/

echo ""
echo "🎉 All tests completed successfully!"
