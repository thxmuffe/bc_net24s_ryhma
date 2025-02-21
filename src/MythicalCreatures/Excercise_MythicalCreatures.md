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

Save the files. Now, launching the API will automatically build the project before running.

---

## Vaihe 4: Rakenna `Program.cs` askel askeleelta

Avaa `Program.cs` ja kirjoita koodi vaiheittain.

### 4.1: Lisää tarvittavat nimiavaruudet

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
```

### 4.2: Luo WebApplication Builder

```csharp
var builder = WebApplication.CreateBuilder(args);
```

- Tämä alustaa uuden Web API -sovelluksen.

### 4.3: Määritä palvelut (Dependency Injection)

```csharp
builder.Services.AddControllers();
```

- Tämä rekisteröi ohjaimet, jotta ne voivat käsitellä HTTP-pyyntöjä.

### 4.4: Rakenna sovellus

```csharp
var app = builder.Build();
```

- Tämä rakentaa sovelluksen määritetyillä asetuksilla.

### 4.5: Määritä reititys ja valtuutus

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();
```

- `UseRouting()` ohjaa pyynnöt oikeille ohjaimille.
- `MapControllers()` mahdollistaa ohjainten käyttämisen API-kutsuihin.

### 4.6: Käynnistä sovellus

```csharp
app.Run();
```

- Tämä käynnistää palvelimen ja mahdollistaa API-kutsut.

Lopullinen `Program.cs` näyttää tältä:

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

## Vaihe 5: Luo Mythical Creature -malli

Luo tiedosto `Models/Creature.cs`:

```csharp
using System.Text.Json.Serialization;

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

### Selitys:

- Tämä malli määrittää olentojen ominaisuudet.
- Se sisältää ID:n, nimen, tyypin ja kolme kykyä.

---

## Vaihe 6: Luo JSON-tiedoston hallinta

Luo tiedosto `Data/CreatureRepository.cs`:

```csharp
using System.Text.Json;
using MythicalCreatures.Models;

namespace MythicalCreatures.Data;

public class CreatureRepository
{
    private const string FilePath = "creatures.json";
    private static readonly object _lock = new();

    public List<Creature> LoadCreatures()
    {
        if (!File.Exists(FilePath)) return new List<Creature>();

        lock (_lock)
        {
            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<Creature>>(json) ?? new List<Creature>();
        }
    }

    public void SaveCreatures(List<Creature> creatures)
    {
        lock (_lock)
        {
            var json = JsonSerializer.Serialize(creatures, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
    }
}
```

### Selitys:

- `LoadCreatures()`: Lataa olennot `creatures.json`-tiedostosta ja deserialisoi ne.
- `SaveCreatures()`: Tallentaa olennot JSON-muodossa.

---

## Vaihe 7: Luo Creature Controller JSON-tiedostopohjaisella tallennuksella

Luo tiedosto `Controllers/CreaturesController.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using MythicalCreatures.Models;
using MythicalCreatures.Data;

namespace MythicalCreatures.Controllers;

[ApiController]
[Route("[controller]")]
public class CreaturesController : ControllerBase
{
    private readonly CreatureRepository _repository;

    public CreaturesController()
    {
        _repository = new CreatureRepository();
    }

    [HttpGet]
    public ActionResult<IEnumerable<Creature>> GetAll()
    {
        var creatures = _repository.LoadCreatures();
        return Ok(creatures);
    }

    [HttpGet("{id}")]
    public ActionResult<Creature> Get(int id)
    {
        var creatures = _repository.LoadCreatures();
        var creature = creatures.FirstOrDefault(c => c.Id == id);
        return creature is null ? NotFound() : Ok(creature);
    }

    [HttpPost]
    public ActionResult Add(Creature creature)
    {
        var creatures = _repository.LoadCreatures();
        creature.Id = creatures.Any() ? creatures.Max(c => c.Id) + 1 : 1;
        creatures.Add(creature);
        _repository.SaveCreatures(creatures);
        return CreatedAtAction(nameof(Get), new { id = creature.Id }, creature);
    }

    [HttpPut("{id}")]
    public ActionResult Update(int id, Creature updatedCreature)
    {
        var creatures = _repository.LoadCreatures();
        var index = creatures.FindIndex(c => c.Id == id);
        if (index == -1) return NotFound();

        creatures[index] = updatedCreature;
        _repository.SaveCreatures(creatures);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var creatures = _repository.LoadCreatures();
        var creature = creatures.FirstOrDefault(c => c.Id == id);
        if (creature is null) return NotFound();

        creatures.Remove(creature);
        _repository.SaveCreatures(creatures);
        return NoContent();
    }
}
```

### Selitys:

- `GetAll()`: Hakee kaikki olennot.
- `Get(id)`: Palauttaa yksittäisen olennon ID:n perusteella.
- `Add(creature)`: Lisää uuden olennon ja tallentaa tiedoston.
- `Update(id, creature)`: Päivittää olemassa olevan olennon.
- `Delete(id)`: Poistaa olennon tiedostosta.

---

## Vaihe 8: Käynnistä ja testaa API

1. Avaa **Run and Debug** -paneeli (`Cmd + Shift + D` macOS:ssä, `Ctrl + Shift + D` Windowsissa).
2. Valitse **Launch API**.
3. Paina **F5** tai käytä aiemmin luotua pikanäppäintä (`Cmd + Alt + R` tai `Ctrl + Alt + R`).

API käynnistyy, ja URL näkyy terminaalissa.

---

## Vaihe 9: Testaa API

(Sama kuin aiemmin, ei muutoksia.)

---

## Yhteenveto

Olet nyt rakentanut **Mythical Creatures Web API** -sovelluksen, joka tallentaa tiedot **JSON-tiedostoon**. Se sisältää:

✅ **Täydellinen ohjelma, ohjaimet ja tietovarasto**  
✅ **Käynnistys- ja testausohjeet**  
✅ **Täysi CRUD-toiminnallisuus**  

Nyt voit laajentaa API:ta lisäämällä tietokantatuen tai uusia ominaisuuksia! 🚀