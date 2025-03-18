# OAuth Frontend - Integrointi Products API:in

Tässä ohjeessa opit toteuttamaan **frontend-autentikoinnin** käyttäen **Google OAuth** -kirjautumista. Saatu **ID Token** voidaan käyttää **Products API** -rajapinnan kutsuihin.

Huom! Sinun ei tarvitse tehdä itse Google:n palvelussa uutta authorize clienttiä, vaan voit käyttää opettajan luomaa ClientID:tä (Secret löytyy ItsLearning tehtävänannossa)

## 1. Luo Frontend-tiedostot

### 1.1 `index.html` (Kirjautumissivu)
Luo tiedosto **`index.html`** ja lisää seuraava koodi:

```html
<!DOCTYPE html>
<html lang="fi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Google OAuth Kirjautuminen</title>
</head>
<body>
    <h2>Kirjaudu sisään Googlella</h2>
    <button onclick="loginWithGoogle()">Kirjaudu</button>

    <script>
        function loginWithGoogle() {
            const clientId = 'YOUR_GOOGLE_CLIENT_ID';
            const redirectUri = 'http://localhost:5000/afterlogin.html'; // Huomaa uusi portti!
            const scope = 'openid email profile';
            const authUrl = `https://accounts.google.com/o/oauth2/v2/auth?client_id=${clientId}&redirect_uri=${redirectUri}&response_type=code&scope=${scope}&access_type=offline`;
            
            window.location.href = authUrl;
        }
    </script>
</body>
</html>
```

---

### 1.2 `afterlogin.html` (Käsittelee kirjautumisvastauksen)
Luo tiedosto **`afterlogin.html`** ja lisää seuraava koodi:

```html
<!DOCTYPE html>
<html lang="fi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>OAuth Callback</title>
</head>
<body>
    <h2>Kirjautuminen onnistui</h2>
    <p id="status">Käsitellään...</p>
    <p><strong>ID Token:</strong> <span id="idToken"></span></p>

    <script>
        function getQueryParam(name) {
            const urlParams = new URLSearchParams(window.location.search);
            return urlParams.get(name);
        }

        async function exchangeCodeForToken(code) {
            const tokenUrl = "https://oauth2.googleapis.com/token";

            const data = new URLSearchParams({
                client_id: "YOUR_GOOGLE_CLIENT_ID",
                client_secret: "YOUR_GOOGLE_CLIENT_SECRET",
                code: code,
                grant_type: "authorization_code",
                redirect_uri: "http://localhost:5000/afterlogin.html"
            });
            
            try {
                const response = await fetch(tokenUrl, {
                    method: "POST",
                    headers: { "Content-Type": "application/x-www-form-urlencoded" },
                    body: data
                });
                
                const tokenInfo = await response.json();
                
                if (tokenInfo.id_token) {
                    document.getElementById("status").innerText = "Kirjautuminen onnistui!";
                    document.getElementById("idToken").innerText = tokenInfo.id_token;
                    saveIdToken(tokenInfo.id_token);
                } else {
                    document.getElementById("status").innerText = "Virhe: ID Tokenia ei saatu.";
                }

            } catch (error) {
                document.getElementById("status").innerText = "Virhe: " + error.message;
            }
        }

        function saveIdToken(token) {
            const blob = new Blob([token], { type: "text/plain" });
            const a = document.createElement("a");
            a.href = URL.createObjectURL(blob);
            a.download = "id_token.txt";
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
        }

        const code = getQueryParam("code");
        
        if (code) {
            exchangeCodeForToken(code);
        } else {
            document.getElementById("status").innerText = "Virhe: OAuth-koodia ei löydy.";
        }
    </script>
</body>
</html>
```

---

## 2. Käynnistä Frontend ja API

### Käynnistä API ja Frontend yhdessä VS Codessa
Avaa **VS Code** ja lisää `launch.json`-tiedostoon seuraava konfiguraatio:

```json
{
    "version": "0.2.0",
    "compounds": [
        {
            "name": "Run API and Frontend",
            "configurations": ["Launch API", "Launch Frontend"]
        }
    ],
    "configurations": [
        {
            "name": "Launch API",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build",
            "program": "${workspaceFolder}/ProductsApi",
            "cwd": "${workspaceFolder}/ProductsApi",
            "stopAtEntry": false
        },
        {
            "name": "Launch Frontend",
            "type": "node",
            "request": "launch",
            "cwd": "${workspaceFolder}/frontend",
            "runtimeExecutable": "python3",
            "runtimeArgs": ["-m", "http.server", "5000"]
        }
    ]
}
```

Käynnistä molemmat painamalla **F5** ja valitsemalla **"Run API and Frontend"**.

---
## 3. Mitä frontend tekee?

Frontend toteuttaa seuraavat vaiheet:
1. Käyttäjä kirjautuu sisään Googlella tai Microsoftilla.
2. Käyttäjä ohjataan takaisin **`afterlogin.html`**-sivulle, jossa OAuth-palvelu palauttaa **ID Tokenin**.
3. Frontend **tallentaa ID Tokenin** tiedostona (`id_token.txt`).
4. Tätä tokenia voidaan käyttää **API-kutsujen autentikointiin**.

### Jatkaminen: Testaa Web API ID Tokenilla
1. Avaa **`id_token.txt`** ja kopioi token.
2. Käytä tokenia **API-pyynnöissä**:

```http
GET https://localhost:5001/api/products
Authorization: Bearer {{YOUR_ID_TOKEN}}
```

Tämä toimii **VS Code REST Client** -laajennuksella tai **curlilla**.

```sh
curl -X GET https://localhost:5001/api/products \
     -H "Authorization: Bearer YOUR_ID_TOKEN"
```

---

## 4. Miten autentikointiprosessi toimii?

1. **OAuth-kirjautuminen**: Käyttäjä siirtyy Google/Microsoft-kirjautumissivulle.
2. **OAuth-palvelu palauttaa koodin** (`authorization_code`), joka vaihdetaan ID Tokeniksi.
3. **Frontend tallentaa ID Tokenin** selaimessa tai tiedostona.
4. **ID Token lähetetään API:lle** jokaisen pyynnön mukana.
5. **API varmistaa tokenin aitouden** ja hyväksyy tai hylkää pyynnön.

Lisälukemista:
- [OAuth 2.0 Authorization Code Flow](https://auth0.com/docs/get-started/authentication-and-authorization-flow/authorization-code-flow)
- [JSON Web Tokens (JWT)](https://jwt.io/)

---

## 5. HTTP-evästeet (cookies) ID Tokenin tallentamiseen

Tällä hetkellä **ID Token tallennetaan tiedostona**, mutta parempi vaihtoehto on **HTTP-evästeet**, koska:
✅ Token voidaan säilyttää turvallisesti selaimessa.  
✅ API voi lukea tokenin automaattisesti ilman, että käyttäjän tarvitsee kopioida sitä.  

### **Miten tallentaa ID Token evästeenä?**
Korvaa **tokenin tallennus** seuraavalla koodilla `afterlogin.html`-tiedostossa:

```js
function saveIdToken(token) {
    document.cookie = `id_token=${token}; Path=/; Secure; HttpOnly; SameSite=Strict`;
}
```

### **Miten API voi lukea evästeen?**
Backend voi lukea evästeen **HTTP-pyynnöistä**:
```csharp
string token = Request.Cookies["id_token"];
```

Lisälukemista:
- [Using HTTP Cookies for Authentication](https://developer.mozilla.org/en-US/docs/Web/HTTP/Cookies)
- [How to Securely Store JWT Tokens](https://developer.okta.com/blog/2017/08/17/where-to-store-jwts-cookies-vs-html5-web-storage)

---

Tämä tekee järjestelmästä **turvallisemman ja helpommin käytettävän**! 🚀


---

### **Bonus: Lisää tyylit ja logot**
Kun perustoiminnallisuus toimii, voit lisätä **CSS-tyylit ja logot**:

```html
<!DOCTYPE html>
<html lang="fi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>OAuth Kirjautuminen</title>
</head>
<body>
    <h2>Kirjaudu sisään</h2>
    <button onclick="loginWithGoogle()">Kirjaudu Googlella</button>
    <button onclick="loginWithMicrosoft()">Kirjaudu Microsoftilla</button>

    <script>
        function loginWithGoogle() {
            const clientId = 'YOUR_GOOGLE_CLIENT_ID';
            const redirectUri = 'http://localhost:5000/afterlogin.html';
            const scope = 'openid email profile';
            const authUrl = `https://accounts.google.com/o/oauth2/v2/auth?client_id=${clientId}&redirect_uri=${redirectUri}&response_type=code&scope=${scope}&access_type=offline`;
            window.location.href = authUrl;
        }

        function loginWithMicrosoft() {
            const clientId = 'YOUR_MICROSOFT_CLIENT_ID';
            const redirectUri = 'http://localhost:5000/afterlogin.html';
            const scope = 'openid email profile';
            const authUrl = `https://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id=${clientId}&response_type=code&redirect_uri=${redirectUri}&scope=${scope}`;
            window.location.href = authUrl;
        }
    </script>
</body>
</html>
```

1. Lisää `style.css` ja muokkaa painikkeiden ulkoasua.
2. Päivitä `index.html` lisäämällä tyylit ja logot.

```html
<head>
    <link rel="stylesheet" href="style.css">
</head>
```

```css
.login-btn {
    display: flex;
    align-items: center;
    padding: 10px;
    border: none;
    cursor: pointer;
    font-size: 16px;
    margin: 10px;
}
.login-btn img {
    width: 20px;
    height: 20px;
    margin-right: 10px;
}
```

```html
<button class="login-btn" onclick="loginWithGoogle()">
    <img src="https://upload.wikimedia.org/wikipedia/commons/5/53/Google_%22G%22_Logo.svg" alt="Google">Kirjaudu Googlella
</button>
<button class="login-btn" onclick="loginWithMicrosoft()">
    <img src="https://upload.wikimedia.org/wikipedia/commons/4/44/Microsoft_logo.svg" alt="Microsoft">Kirjaudu Microsoftilla
</button>
```

---

## BONUS: Luo oma Google OAuth Client ID
Jos haluat käyttää omaa Google OAuth -tiliäsi:
1. Mene [Google Cloud Consoleen](https://console.cloud.google.com/).
2. Luo uusi projekti ja siirry **API & Services → Credentials**.
3. Luo **OAuth Client ID**.
4. Määritä **Allowed redirect URIs**:
   - `http://localhost:5000/afterlogin.html`
5. Tallenna **Client ID** ja **Client Secret**, ja päivitä ne HTML-tiedostoihin.

---

## BONUS Luo Microsoft Client ID
Jos haluat lisätä **Microsoft Loginin**, sinun täytyy luoda **Microsoft Client ID**:

### **1. Luo Microsoft Client ID**
1. Mene [Microsoft Azure Portal](https://portal.azure.com/).
2. Siirry **Azure Active Directory → App Registrations**.
3. Paina **+ New Registration**.
4. Anna sovellukselle nimi (esim. `ProductsAPI-OAuth`).
5. Valitse **Account type**:
   - **"Accounts in any organizational directory and personal Microsoft accounts"**
6. Syötä **Redirect URI** → `http://localhost:5000/afterlogin.html`.
7. Paina **Register**.
8. Kopioi **Application (client) ID** ja päivitä se `index.html`-tiedostoon kohtaan `YOUR_MICROSOFT_CLIENT_ID`.

---

## Yhteenveto
✅ **VS Coden launch.json tukee nyt projektia suoraan ilman .dll-tiedostoa**.  
✅ **Google Cloud -asetukset ovat nyt erillisenä lisäosiona niille, jotka haluavat käyttää omaa kirjautumista**.  
✅ **ID Tokenin hankkiminen ja käyttö API:ssa on selkeästi kuvattu**.  

Tämä tekee testauksesta helppoa kaikille! 🚀