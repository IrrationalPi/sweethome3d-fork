# Default recipe
default: build

# Build the project
build:
    dotnet build

# Run tests
test:
    dotnet test

# Run tests with code coverage
test-coverage:
    dotnet test --collect:"XPlat Code Coverage"

# Run the UI project
run:
    dotnet run --project src/OpenHome3D.UI