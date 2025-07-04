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

To test if goals are being loaded into memory properly, simply run this from the solution root:
```bash
dotnet test
```
To test the GUI behavior: 

1. Click Set Goal
2. Enter random values and click OK
3. The JSON file with the goal data should be created at `FitnessTracker/bin/Debug/net9.0-windows/FitnessTracker/SaveData/goals.json`
    - **IMPORTANT NOTE:** The reason the json file is expected at this location is because the "application root" gets resolved to the location of the executable (which is at `FitnessTracker/bin/Debug/net9.0-windows/`) 