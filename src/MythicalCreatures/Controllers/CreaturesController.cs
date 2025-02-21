using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[ApiController]
[Route("api/creatures")]
public class CreaturesController : ControllerBase
{
    private const string DataFile = "Data/creatures.json";

    private List<MythicalCreature> LoadCreatures()
    {
        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), DataFile);
        Console.WriteLine($"Attempting to retrieve file: {fullPath}");

        if (!System.IO.File.Exists(fullPath))
            return new List<MythicalCreature>();
        return JsonConvert.DeserializeObject<List<MythicalCreature>>(System.IO.File.ReadAllText(fullPath)) ?? new List<MythicalCreature>();
    }

    private void SaveCreatures(List<MythicalCreature> creatures)
    {
        string fullPath = Path.Combine(Directory.GetCurrentDirectory(), DataFile);
        Console.WriteLine($"Saving creatures to file: {fullPath}");
        System.IO.File.WriteAllText(fullPath, JsonConvert.SerializeObject(creatures, Formatting.Indented));
    }

    [HttpGet]
    public ActionResult<List<MythicalCreature>> GetAll() => LoadCreatures();

    [HttpGet("{id}")]
    public ActionResult<MythicalCreature> Get(int id)
    {
        var creature = LoadCreatures().FirstOrDefault(c => c.Id == id);
        return creature == null ? NotFound() : Ok(creature);
    }

    [HttpPost]
    public ActionResult Create([FromBody] MythicalCreature creature)
    {
        var creatures = LoadCreatures();
        creature.Id = creatures.Count > 0 ? creatures.Max(c => c.Id) + 1 : 1;
        creatures.Add(creature);
        SaveCreatures(creatures);
        return CreatedAtAction(nameof(Get), new { id = creature.Id }, creature);
    }

    [HttpPut("{id}")]
    public ActionResult Update(int id, [FromBody] MythicalCreature updatedCreature)
    {
        var creatures = LoadCreatures();
        var creature = creatures.FirstOrDefault(c => c.Id == id);
        if (creature == null) return NotFound();

        creature.Name = updatedCreature.Name;
        creature.Type = updatedCreature.Type;
        creature.Power = updatedCreature.Power;
        creature.Weakness = updatedCreature.Weakness;
        SaveCreatures(creatures);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var creatures = LoadCreatures();
        var creature = creatures.FirstOrDefault(c => c.Id == id);
        if (creature == null) return NotFound();

        creatures.Remove(creature);
        SaveCreatures(creatures);
        return NoContent();
    }
}
