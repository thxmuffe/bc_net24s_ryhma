
# Azure konfiguraatio

Tähän ohjeeseen on eriytetty Azure portaalissa tehtävät asiat.
---

### **1. Registering the Web API in Microsoft Entra**

**Tarkoitus**: Rekisteröimme Web API:n **Microsoft Entra**an, jotta se voi käyttää Microsoftin identiteetti-alustaa autentikointiin ja päästä SQL Serveriin tokenin avulla.

**Vaiheet**:
1. **Azure Portal**: Avaa [Azure Portal](https://portal.azure.com/).
2. **Navigate to Microsoft Entra**: Etsi ja valitse **Microsoft Entra** vasemman laidan valikosta.
3. **Create a New App Registration**:
   - Siirry kohtaan **App registrations** → **New registration**.
   - Täytä **Name** (esim. `net24s_webapi`).
   - Valitse **Single tenant** (jos sovellus on organisaatiossa käytettävä).
   - Ohita **Redirect URI** (ellet tarvitse sitä OAuth-loginia varten).
   - Klikkaa **Register**.

**Mitä tämä tekee**:  
- Luo **Client ID** ja **Tenant ID**, joita käytetään autentikointiin ja resursseihin pääsyyn.

**Miten tämä liittyy Web API:n koodiin**:  
- Web API käyttää **Client ID** ja **Tenant ID** tietoja saadakseen **Microsoft Entra**:sta tokenin turvallista käyttöä varten. 

---

### **2. Granting the Web API Access to SQL Server**

**Tarkoitus**: Nyt kun Web API on rekisteröity **Microsoft Entra**an, sinun täytyy konfiguroida SQL Server niin, että se tunnistaa ja autentikoi sovelluksen.

**Vaiheet**:
1. **Navigate to SQL Server**: Siirry **SQL Servers** kohtaan Azure Portaalissa ja valitse SQL Server, johon haluat yhdistää.
2. **Enable Active Directory Admin**: Siirry **Active Directory admin** kohtaan ja määritä **AD-admin käyttäjä**.
3. **Query Editor**:
   - Siirry **SQL Databases** → valitse tietokanta → klikkaa **Query Editor (preview)**.
   - Kirjaudu sisään **Microsoft Entra** -admin-käyttäjällä.
4. **Create SQL User for the Web API**:
   - Aja seuraavat SQL-komennot Query Editorissa:
     ```sql
     CREATE USER [<AppName>] FROM EXTERNAL PROVIDER;
     ALTER ROLE db_datareader ADD MEMBER [<AppName>];
     ALTER ROLE db_datawriter ADD MEMBER [<AppName>];
     ```
     [<AppName>] on esim net24s_webapi. Sama kuin minkä teit aikaisemmin Entra-osiossa.

**Mitä tämä tekee**:  
- Luo SQL Serveriin käyttäjän, joka on autentikoitu **Microsoft Entra**:n avulla ja myöntää oikeudet tietokannan käyttöön.

**Miten tämä liittyy Web API:n koodiin**:  
- Web API voi nyt käyttää **Microsoft Entra** -tokenia ja yhdistää SQL Serveriin turvallisesti.

---

### **3. Exposing the Web API**

**Tarkoitus**: Tässä vaiheessa määritetään sovellukselle käyttöoikeudet ja altistetaan API **Entran** kautta.

**Vaiheet**:
1. **Go to Expose an API**:
   - Siirry **App registrations** → valitse sovellus (esim. `net24s_webapi`).
   - Siirry **Expose an API** -kohtaan.
   - Määritä **Application ID URI** (esim. `api://<Client-ID>`).
   - Lisää **Scopes** kuten `access_as_user` haluamiesi käyttöoikeuksien määrittämiseksi.
   - Tallenna muutokset.

**Mitä tämä tekee**:  
- API altistetaan muiden sovellusten käytettäväksi ja määritellään tarvittavat **scopes**, joita tarvitaan pääsyyn API:in.

**Miten tämä liittyy Web API:n koodiin**:  
- Altistamalla API:n **Microsoft Entra**:lle varmistetaan, että vain valtuutetut sovellukset voivat käyttää sitä.

---

### **4. Configuring Certificates & Secrets**

**Tarkoitus**: Sertifikaatti tai asiakassalaisuus tarvitaan, jotta Web API voi autentikoitua **Microsoft Entra**:n kautta ja hakea tokenin.

**Vaiheet**:
1. **Add Client Secret**:
   - Siirry **Certificates & secrets** -kohtaan **App registrations** -osiossa.
   - Klikkaa **New client secret**, anna sille kuvaus ja valitse voimassaoloaika.
   - Tallenna salaisuus ja kopioi se, sillä sitä ei voi saada myöhemmin.

**Mitä tämä tekee**:  
- **Client Secret** mahdollistaa **Microsoft Entra**:n kautta tapahtuvan autentikoinnin ja tokenin hakemisen.

**Miten tämä liittyy Web API:n koodiin**:  
- Web API käyttää **client secret**-avainta koodissaan saadakseen **auth-tokenin**, joka tarvitaan SQL Serverin käyttöön.

---

### **5. Setting Permissions in SQL Server IAM**

**Tarkoitus**: IAM:llä hallitaan, kuka voi käyttää **SQL Serveriä** ja määritellään, mitä käyttöoikeuksia kullakin käyttäjällä on.

**Vaiheet**:
1. **Navigate to SQL Server IAM**:
   - Siirry **SQL Server** → **Access Control (IAM)**.
   - Jos et löydä rooleja kuten **SQL DB Contributor** tai **Directory Readers**, voit käyttää **Owner**-roolia, kuten kuvassa näkyy, joka antaa täydet käyttöoikeudet SQL Serverin hallintaan.
   - Klikkaa **+ Add** ja valitse rooli, kuten **Owner**.
   - Lisää **App Registration** rooliin (valitse sovelluksesi).

**Mitä tämä tekee**:  
- **IAM** varmistaa, että Web API:lle annetaan **täydet käyttöoikeudet** SQL Serveriin. Tässä vaiheessa **Owner**-rooli on käyttökelpoinen, jos muita tarkempia rooleja ei löydy.

**Miten tämä liittyy Web API:n koodiin**:  
- Web API saa täyden pääsyn SQL Serveriin ja voi tehdä tarvittavat toimet tietokannan kanssa (luku, kirjoitus).

---

### **6. Running EF Migrations**

**Tarkoitus**: **Entity Framework (EF)** -migreeraukset luovat tietokannan taulut ja varmistavat, että skeema on ajantasalla.

**Vaiheet**:
1. **Run Migrations**:
(Huom! sinulla on oltava kaikki muu koodi kunnossa ennen kuin voit ajaa tämän)
   - Avaa terminaali ja aja seuraavat komennot:
     ```bash
     dotnet ef migrations add Init
     dotnet ef database update
     ```
   - Tämä luo tarvittavat taulut (esim. `TodoItems`) SQL-tietokantaan.

**Mitä tämä tekee**:  
- Se luo ja soveltaa migraatioita, jotka varmistavat, että tietokannan skeema vastaa sovelluksen **entity-malleja**.

**Miten tämä liittyy Web API:n koodiin**:  
- **EF Migrations** varmistaa, että SQL-tietokanta luodaan ja päivitetään oikein, jotta Web API voi käyttää sitä.
