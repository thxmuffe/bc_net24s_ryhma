# Mythical Creatures Web API - Step-by-Step Guide

## Prerequisites

- Install .NET 9.0 SDK: [Download](https://dotnet.microsoft.com/download)
- Install Visual Studio Code and the C# extension

---

## Step 1: Create a New Web API Project

1. Open Terminal and run:

   ```sh
   dotnet new webapi -n MythicalCreatures
   cd MythicalCreatures
   code .
   ```

2. Delete `WeatherForecast.cs` and its references in `Program.cs`.

---

## Step 2: Set Up VS Code Launch Configuration

1. Open the **Command Palette** (`Cmd + Shift + P` on macOS, `Ctrl + Shift + P` on Windows).
2. Search for **"Debug: Open launch.json"** and select it.
3. If you don’t have a `launch.json`, click **Add Configuration** and choose **.NET Core Launch (web)**.
4. Modify the `.vscode/launch.json` file:

```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": "Launch API",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build",
            "program": "${workspaceFolder}/bin/Debug/net9.0/MythicalCreatures.dll",
            "args": [],
            "cwd": "${workspaceFolder}",
            "stopAtEntry": false,
            "serverReadyAction": {
                "action": "openExternally",
                "pattern": "\bNow listening on:\s+(https?://\S+)"
            },
            "env": {
                "ASPNETCORE_ENVIRONMENT": "Development"
            },
            "sourceFileMap": {
                "/Users/your-user-name": "${workspaceFolder}"
            }
        }
    ]
}
```

5. Open `.vscode/tasks.json` and add:

```json
{
    "version": "2.0.0",
    "tasks": [
        {
            "label": "build",
            "command": "dotnet build",
            "type": "shell",
            "problemMatcher": "$msCompile",
            "group": "build",
            "presentation": {
                "reveal": "always",
                "panel": "shared"
            }
        }
    ]
}
```

6. Save the files. Now, launching the API will automatically build the project before running.

---

## Step 3: Understanding and Building `Program.cs`

We will build `Program.cs` step by step, understanding what each part does.

### Step 3.1: Add Required Namespaces

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
```

These namespaces provide the core functionality for building and running an ASP.NET Web API.

### Step 3.2: Create a WebApplication Builder

```csharp
var builder = WebApplication.CreateBuilder(args);
```

- This initializes a new Web API application.
- It automatically loads settings from `appsettings.json`, environment variables, and command-line arguments.

### Step 3.3: Configure Services (Dependency Injection)

```csharp
builder.Services.AddControllers();
```

- Registers support for MVC-style controllers.
- Allows defining routes and handling HTTP requests.

### Step 3.4: Build the Application

```csharp
var app = builder.Build();
```

- This finalizes the application configuration and prepares it for execution.

### Step 3.5: Configure Middleware and Routing

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();
```

- `UseDeveloperExceptionPage()` provides detailed errors during development.
- `UseRouting()` enables request handling.
- `UseAuthorization()` (optional) enforces security policies.
- `MapControllers()` activates controller routes.

### Step 3.6: Start the API

```csharp
app.Run();
```

- This starts the application, making it accessible via HTTP.

---

## Step 4: Define the Mythical Creature Model

Create a new file `Models/Creature.cs`:

```csharp
namespace MythicalCreatures.Models;

public class Creature
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Strength { get; set; }
    public int Agility { get; set; }
    public int Intelligence { get; set; }
}
```

### Explanation:

- `Id`: A unique identifier for each creature.
- `Name`: The name of the mythical creature.
- `Type`: A classification (e.g., Fire, Water, Air).
- `Strength`, `Agility`, `Intelligence`: Key attributes defining abilities.

---

## Step 5: Create the Creature Controller

Create a new file `Controllers/CreaturesController.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using MythicalCreatures.Models;

namespace MythicalCreatures.Controllers;

[ApiController]
[Route("[controller]")]
public class CreaturesController : ControllerBase
{
    private static List<Creature> _creatures = new()
    {
        new Creature { Id = 1, Name = "Phoenix", Type = "Fire", Strength = 80, Agility = 90, Intelligence = 85 },
        new Creature { Id = 2, Name = "Griffin", Type = "Air", Strength = 85, Agility = 80, Intelligence = 70 }
    };

    [HttpGet]
    public ActionResult<IEnumerable<Creature>> GetAll() => Ok(_creatures);

    [HttpGet("{id}")]
    public ActionResult<Creature> Get(int id)
    {
        var creature = _creatures.FirstOrDefault(c => c.Id == id);
        return creature is null ? NotFound() : Ok(creature);
    }

    [HttpPost]
    public ActionResult Add(Creature creature)
    {
        creature.Id = _creatures.Max(c => c.Id) + 1;
        _creatures.Add(creature);
        return CreatedAtAction(nameof(Get), new { id = creature.Id }, creature);
    }

    [HttpPut("{id}")]
    public ActionResult Update(int id, Creature updatedCreature)
    {
        var index = _creatures.FindIndex(c => c.Id == id);
        if (index == -1) return NotFound();
        _creatures[index] = updatedCreature;
        return NoContent();
    }

    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var creature = _creatures.FirstOrDefault(c => c.Id == id);
        if (creature is null) return NotFound();
        _creatures.Remove(creature);
        return NoContent();
    }
}
```

### Explanation:

- `GetAll()`: Returns all creatures.
- `Get(id)`: Fetches a single creature by ID.
- `Add(creature)`: Adds a new creature to the list.
- `Update(id, creature)`: Updates an existing creature.
- `Delete(id)`: Removes a creature from the list.

---

## Step 6: Run the API Using VS Code

1. Open the **Run and Debug** panel (`Cmd + Shift + D` on macOS, `Ctrl + Shift + D` on Windows).
2. Select **Launch API**.
3. Click **Start Debugging** (`F5`).

Your API is now running.

---

## Step 7: Create a `test.http` File

Save the following as `test.http` in your project root:

```
@baseUrl = http://localhost:5000

### Get all creatures
GET {{baseUrl}}/creatures
Accept: application/json

### Get a specific creature
GET {{baseUrl}}/creatures/1
Accept: application/json

### Add a new creature
POST {{baseUrl}}/creatures
Content-Type: application/json

{
  "name": "Dragon",
  "type": "Fire",
  "strength": 95,
  "agility": 80,
  "intelligence": 85
}

### Delete a creature
DELETE {{baseUrl}}/creatures/1
```

### Running the Requests

1. Install the **REST Client** extension in VS Code.
2. Open `test.http` and click **Send Request** above any request.

---

## Conclusion

You've built a fully functional Mythical Creatures Web API with detailed explanations. Now, try adding persistence using a database or authentication.
