# Products API - Vaiheittainen Opas

Tämä opas opastaa sinut **.NET 9 Web API** -sovelluksen rakentamiseen tuotteiden hallintaan, sisältäen **OAuth-autentikoinnin**. Opit:

- Luomaan Web API -projektin
- Toteuttamaan CRUD-toiminnot tuotteille
- Suojaamaan API:n OAuth-autentikoinnilla (Google-kirjautuminen)
- (Valinnainen) Julkaisemaan ja testaamaan API:n

---

## 1. Luo uusi .NET Web API -projekti

### Käyttäen VS Coden komentopalettia
1. Avaa **VS Code**.
2. Avaa **Komentopaletti** (`Cmd + Shift + P`).
3. Kirjoita **“.NET: New Project”** ja valitse se.
4. Valitse **ASP.NET Core Web API**.
5. Valitse **.NET 9.0** kehyksenä.
6. Anna projektille nimi `ProductsApi` ja valitse kansio.
7. Paina **Luo**.

Tämä varmistaa, että projekti on oikein alustettu ja sisältää tarvittavat riippuvuudet.

---

## 2. Lisää tarvittavat paketit
### Käyttäen komentoriviä
```sh
# Entity Framework Core - In-Memory Database
dotnet add package Microsoft.EntityFrameworkCore.InMemory

# OAuth-autentikointi
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

### Käyttäen NuGet-pakettienhallintaa (VAIHTOEHTOISESTI)
1. Avaa **VS Code**.
2. Klikkaa **Laajennukset** ja etsi `NuGet Package Manager`.
3. Avaa `ProductsApi.csproj` **Explorerissa**.
4. Napsauta hiiren oikealla **Dependencies** → **Manage NuGet Packages**.
5. Etsi ja asenna `Microsoft.EntityFrameworkCore.InMemory` sekä `Microsoft.AspNetCore.Authentication.JwtBearer`.

### Muokkaamalla `.csproj`-tiedostoa suoraan (VAIHTOEHTOISESTI)
Avaa `ProductsApi.csproj` ja lisää:
```xml
<ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="9.0.0" />
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.0" />
</ItemGroup>
```

---

## 3. Määritä Product-malli

Luo tiedosto `Models/Product.cs`:
```csharp
namespace ProductsApi.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
```
### Selitys
#### Mikä on malli (Model)?
Malli kuvaa **tietorakennetta**, jota sovellus käsittelee. Se määrittelee, millaisessa muodossa tiedot tallennetaan ja lähetetään API:n kautta.

#### Miksi se on `Models/`-kansiossa?
Sovelluksissa on hyvä pitää **tiedot (mallit), liiketoimintalogiikka ja ohjaimet erillään**. Tämä helpottaa koodin ymmärrettävyyttä ja ylläpitoa.

#### Miten tätä mallia käytetään?
- **Ohjain (Controller)** hyödyntää tätä mallia vastauksissa.
- **Tietokanta** (tässä tapauksessa muistiin tallennettava tietokanta) käyttää tätä mallia taulun rakenteena.
- **Asiakasohjelmat** odottavat vastauksia tässä muodossa.

---

## 4. Määritä In-Memory Database

Luo tiedosto `Data/AppDbContext.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using ProductsApi.Models;

namespace ProductsApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Product> Products { get; set; }
    }
}
```

### Selitys
#### Miksi käytämme In-Memory Databasea?
Normaalisti tietokanta (esim. SQL Server) vaatii **taulujen luomisen ja hallinnan**. Käyttämällä **muistissa olevaa tietokantaa (InMemoryDatabase)** vältämme tämän monimutkaisuuden ja voimme keskittyä **API:n toiminnallisuuteen**.

Lisää `Program.cs`-tiedostoon:
```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("ProductsDb"));
```

---

## 5. Luo ProductController
Luo tiedosto `Controllers/ProductsController.cs`:
```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductsApi.Data;
using ProductsApi.Models;

namespace ProductsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetProducts), new { id = product.Id }, product);
        }
    }
}
```
### Selitys
#### Mikä on ohjain (Controller)?
Ohjain vastaanottaa **HTTP-pyyntöjä** ja palauttaa vastauksia. Se toimii **API:n keskeisenä osana**, joka käsittelee tietoja ja kommunikoi tietokannan kanssa.

#### Miten ohjain toimii?
- `[HttpGet]` hakee **kaikki tuotteet** tietokannasta.
- `[HttpPost]` lisää **uuden tuotteen** tietokantaan ja palauttaa sen asiakkaalle.
- **AppDbContext** yhdistää ohjaimen tietokantaan, jotta tuotteet voidaan hakea ja tallentaa.

---

## 6. Käynnistä API VS Codessa
Koska projektissa on **launch.json**, API voidaan käynnistää helposti:

### Käynnistys VS Coden pikanäppäimillä
- **Mac**: `Cmd + Shift + F5`
- (Katso tai muokkaa pikanäppäimiä: [VS Code Keyboard Shortcuts](https://code.visualstudio.com/docs/getstarted/keybindings))

---

## 7. (Valinnainen) Julkaise API Azureen

Voit julkaista API:n **Azure App Serviceen**, jos haluat sen toimivan verkossa.

Lisätietoa:
- [Julkaisu Azureen](https://learn.microsoft.com/en-us/aspnet/core/tutorials/publish-to-azure-webapp-using-vs)

---

## Yhteenveto
Nyt olet luonut **Products API** -sovelluksen ja testannut sen! 🚀 Julkaiseminen on valinnaista, mutta suositeltavaa, jos haluat käyttää API:ta verkossa.
