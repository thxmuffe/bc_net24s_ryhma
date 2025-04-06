
# ASP.NET Web API + Microsoft Entra + Azure SQL – Ohjeet

Tässä ohjeessa rakennetaan moderni, tietoturvallinen Web API -ratkaisu ilman salasanoja. Web API autentikoituu Microsoft Entran kautta ja pääsee Azure SQL -tietokantaan tokenin avulla. Kaikki yhteydet ja konfiguraatiot tehdään appsettings.json-tiedoston kautta.

## 1. Projektin valmistelu

### ✔ Luo ASP.NET Web API -projekti
```bash
dotnet new webapi -n todo
```

### ✔ Asenna tarvittavat paketit
```bash
dotnet add package Microsoft.Data.SqlClient
dotnet add package Azure.Identity
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Swashbuckle.AspNetCore
```

## 2. appsettings.json – yhteydet ja asetukset

Tiedosto `appsettings.json` sisältää konfiguraatiotiedot, kuten tietokantayhteyden ja Microsoft Entra -asetukset.

### Mihin tätä käytetään?
Tässä määritellään ulkoiset riippuvuudet ja yhteysmerkkijonot, joita koodi voi lukea ajonaikaisesti. Ne ovat erotettu koodista, jotta ne on helppo vaihtaa ilman uudelleenrakennusta.

### Voinko vain lukea suoraan appsettings.jsonista koodissa?
Voisit, mutta seuraava vaihe parantaa luotettavuutta ja luettavuutta: 

- ✅ **Tyypitetty konfiguraatio** (IOptions) varmistaa, että kaikki asetukset ovat oikein nimettyjä ja alustettuja
- ✅ **Virheiden havaitseminen ajoissa**: saat build-aikaiset virheet puuttuvista kentistä
- ✅ **Koodin siisteys**: pääset eroon suoran merkkijonojen hakemisesta koodin sisällä

### Haittoja?
- ❌ Lisää yksi pieni tiedosto (esim. AzureAdOptions.cs)
- ❌ Aavistuksen pidempi oppimiskäyrä aloittelijoille

Yhteenveto: **pieni lisävaiva, suuri hyöty pitkässä juoksussa**.

### Esimerkkirakenne projektissa

```
src/
├── todo/
│   ├── Models/
│   │   └── TodoItem.cs
│   ├── Data/
│   │   └── TodoContext.cs
│   ├── Configuration/
│   │   └── AzureAdOptions.cs
│   └── appsettings.json
```

## 3. AzureADOptions.cs – tyypitetty asetusluokka

Tämä tiedosto määrittelee C#-luokan, johon `appsettings.json`-tiedoston `AzureAd`-osiota bindataan.

### Miksi teen tämän erikseen?
- ✅ Saamme **type safety**: jos joku kenttä puuttuu tai on kirjoitettu väärin, se huomataan heti build-vaiheessa
- ✅ Luettavuus: asetukset näkyvät selkeästi ja intellisense toimii
- ✅ Helppo testata ja uudelleenkäyttää

### Missä tämä tiedosto kannattaa olla?
Sijoita se esim. kansioon `Configuration/` tai `Options/` projektin sisällä:

```
src/todo/Configuration/AzureAdOptions.cs
```

## 4. Program.cs – vaiheittain

Alla on jaoteltu kaikki Program.cs:n osat selitysten kera. Voit kopioida ne oikeaan paikkaan tai tarkistaa lopusta täyden yhdistetyn version.

### 🔐 a) AzureAd-kredentiaalien asetus
```csharp
builder.Services.Configure<AzureAdOptions>(
    builder.Configuration.GetSection("AzureAd"));
```
Tämä lukee Entra-asetukset appsettings.json-tiedostosta ja tarjoaa ne palveluihin injektoitavaksi.

### 🌐 b) SQL Server -yhteyden muodostus AccessTokenilla
```csharp
builder.Services.AddDbContext<TodoContext>((sp, options) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var azureAd = sp.GetRequiredService<IOptions<AzureAdOptions>>().Value;
    var connectionString = config.GetConnectionString("SqlServer");

    var credential = new ClientSecretCredential(
        azureAd.TenantId,
        azureAd.ClientId,
        azureAd.ClientSecret);

    var token = credential.GetToken(
        new TokenRequestContext(new[] { "https://database.windows.net/.default" }));

    var conn = new SqlConnection(connectionString)
    {
        AccessToken = token.Token
    };

    options.UseSqlServer(conn);
});
```
Tässä yhdistetään tietokantaan ilman käyttäjätunnusta ja salasanaa – vain Entran tokenilla.

### 🛠️ c) Automaattinen tietokantamigraatio
```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TodoContext>();
    db.Database.Migrate();
}
```
Tämä luo puuttuvat taulut automaattisesti EF-migraatioiden perusteella.

### 🧩 Lopullinen Program.cs-yhdistelmä
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AzureAdOptions>(
    builder.Configuration.GetSection("AzureAd"));

builder.Services.AddDbContext<TodoContext>((sp, options) =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var azureAd = sp.GetRequiredService<IOptions<AzureAdOptions>>().Value;
    var connectionString = config.GetConnectionString("SqlServer");

    var credential = new ClientSecretCredential(
        azureAd.TenantId,
        azureAd.ClientId,
        azureAd.ClientSecret);

    var token = credential.GetToken(
        new TokenRequestContext(new[] { "https://database.windows.net/.default" }));

    var conn = new SqlConnection(connectionString)
    {
        AccessToken = token.Token
    };

    options.UseSqlServer(conn);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TodoContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
```

## 5. EF Migrations - käynnistäminen

EF tarvitsee ainakin yhden migraation, jotta tietokanta luodaan automaattisesti:

```bash
dotnet tool install --global dotnet-ef
dotnet ef migrations add Init
```

### ✅ Mitä tapahtuu kun ajat `dotnet ef migrations add Init` ja mistä saamme TenantId, ClientId ja ClientSecret

Kun suoritat `dotnet ef migrations add Init`, luodaan seuraavat tiedostot:

- `Init.cs` → kuvaa, mitä muutoksia tietokantaan tehdään (CREATE TABLE jne)
- `Init.Designer.cs` → tekninen metatiedosto, joka tarvitaan migraation suorittamiseen
- `TodoContextModelSnapshot.cs` → kuvaa nykyisen mallin tilan (EF vertaa tätä uuteen migraatioon seuraavalla kerralla)

**Mista saamme TenantId, ClientId ja ClientSecret?** Nämä tiedot tulevat Microsoft Entraan rekisteröidystä sovelluksesta. Ohjeet siihen on erillisessä tiedostossa **[azure-setup.md](azure-setup.md)**, jossa on vaiheittaiset ohjeet rekisteröinnistä ja tarvittavien tietojen hankkimisesta.

## 6. launchSettings.json ja VS Code -integraatio

Tarkista `Properties/launchSettings.json`, että se sisältää oikean profiilin:

```json
{
  "profiles": {
    "TODO EF": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "api/todoitems",
      "applicationUrl": "https://localhost:7234",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

---

## ⭐ Azure-toiminnot erillisessä tiedostossa

Tässä vaiheessa siirryt tiedostoon **`azure-setup.md`**, jossa tehdään:

- Web API:n rekisteröinti Microsoft Entraan
- Azure SQL -instanssin perustaminen
- Palomuurin avaaminen
- Entra-käyttäjän lisääminen SQL Serveriin
- Sovelluksen (App Registration) lisääminen Azure SQL -tietokannan käyttäjäksi

Referenssi: `[azure-setup.md](azure-setup.md)`

---

## 7. HTTP-testi VS Code -ympäristössä

Luo `test_request.http`:

```http
POST https://localhost:7234/api/todoitems
Content-Type: application/json

{
  "name": "muista hedelmät"
}
```

---

Jatkuu tiedostossa `azure-setup.md`...
