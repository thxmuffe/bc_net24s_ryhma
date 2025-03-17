# OAuth Frontend - Integrointi Products API:in

Tässä ohjeessa opit toteuttamaan **frontend-autentikoinnin** käyttäen **Google OAuth** -kirjautumista. Saatu **ID Token** voidaan käyttää **Products API** -rajapinnan kutsuihin.

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

## 3. BONUS: Luo oma Google OAuth Client ID
Jos haluat käyttää omaa Google OAuth -tiliäsi:
1. Mene [Google Cloud Consoleen](https://console.cloud.google.com/).
2. Luo uusi projekti ja siirry **API & Services → Credentials**.
3. Luo **OAuth Client ID**.
4. Määritä **Allowed redirect URIs**:
   - `http://localhost:5000/afterlogin.html`
5. Tallenna **Client ID** ja **Client Secret**, ja päivitä ne HTML-tiedostoihin.

---

## 4. Yhteenveto
✅ **VS Coden launch.json tukee nyt projektia suoraan ilman .dll-tiedostoa**.  
✅ **Google Cloud -asetukset ovat nyt erillisenä lisäosiona niille, jotka haluavat käyttää omaa kirjautumista**.  
✅ **ID Tokenin hankkiminen ja käyttö API:ssa on selkeästi kuvattu**.  

Tämä tekee testauksesta helppoa kaikille! 🚀