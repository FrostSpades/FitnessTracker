## Description
Fitness Windows Application in development. Tracks various fitness information for the user.

## Environment Setup
This is a WPF application that runs using the dotnet framework.

Install the dotnet framework from [here](https://dotnet.microsoft.com/en-us/download/dotnet-framework).

Then in the project's root directory build it using:

```bash
dotnet build
```

This builds an executable at `FitnessTracker/bin/Debug/net9.0-windows/FitnessTracker.exe` that you can run.

Alternatively, you can simply open the `FitnessTracker.sln` file in an IDE (like JetBrains Rider for example), and run the application from there.

## Testing
The test files exist in `FitnessTracker.Tests/`.

To run the tests, run:
```bash
dotnet test
```

**IMPORTANT NOTE:** The application (not project) root is at `FitnessTracker/bin/Debug/net9.0-windows/`.