# Orthodontic Clinic App

Aplikacja webowa do zarządzania kliniką ortodontyczną stworzona w technologii ASP.NET Core Web API + Vue 3.

Projekt umożliwia zarządzanie pacjentami oraz lekarzami kliniki, w tym:

* wyświetlanie listy pacjentów,
* dodawanie pacjentów,
* edycję danych pacjentów,
* usuwanie pacjentów,
* podgląd szczegółów pacjenta,
* wyświetlanie listy lekarzy,
* dodawanie lekarzy,
* edycję danych lekarzy,
* usuwanie lekarzy,
* podgląd szczegółów lekarza.

---

# Technologie

## Backend

* ASP.NET Core Web API
* Entity Framework Core
* SQL Server
* Swagger

## Frontend

* Vue 3
* TypeScript
* Vue Router
* Axios
* Vite

---

# Funkcjonalności

## Backend

* REST API CRUD
* obsługa błędów
* Entity Framework Core
* migracje bazy danych
* SQL Server

## Frontend

* pełny CRUD pacjentów
* pełny CRUD lekarzy
* routing frontendowy
* dynamiczne odświeżanie danych
* walidacja formularzy
* loading states
* komunikaty sukcesu i błędów
* wyszukiwarka pacjentów
* wyszukiwarka lekarzy
* animacje i transitions
* responsywny interfejs (desktop / tablet / mobile)
* reusable Vue components
* TypeScript interfaces

---

# Struktura projektu

```text
backend/
└── OrthodonticClinic.Api/

frontend/
└── src/
    ├── components/
    ├── views/
    ├── services/
    ├── router/
    ├── utils/
    └── assets/
```

---

# Uruchomienie projektu

## Backend

Przejdź do folderu:

```bash
cd backend/OrthodonticClinic.Api
```

Uruchom backend:

```bash
dotnet run
```

Backend działa domyślnie na:

```text
http://localhost:5153
```

Swagger:

```text
http://localhost:5153/swagger
```

### Entity Framework CLI

Do obsługi migracji bazy danych wymagane jest narzędzie `dotnet-ef`.

Jeżeli narzędzie nie jest zainstalowane, należy wykonać:

```bash
dotnet tool install --global dotnet-ef
```

Jeżeli narzędzie jest już zainstalowane, ale wymaga aktualizacji:

```bash
dotnet tool update --global dotnet-ef
```

Aby zastosować migracje i utworzyć lub zaktualizować bazę danych, należy wykonać:

```bash
dotnet ef database update
```

---

## Frontend

Przejdź do folderu:

```bash
cd frontend
```

Zainstaluj zależności:

```bash
npm install
```

Uruchom frontend:

```bash
npm run dev
```

Frontend działa domyślnie na:

```text
http://localhost:5173
```

---

# Moduł pacjentów

Moduł pacjentów zawiera:

* listę pacjentów,
* szczegóły pacjenta,
* formularz dodawania,
* formularz edycji,
* usuwanie pacjenta z potwierdzeniem.

Dodatkowo:

* walidacja formularzy,
* komunikaty UX,
* loading spinner,
* animacje przejść,
* responsywność,
* wyszukiwarka pacjentów.

---

# Moduł lekarzy

Moduł lekarzy zawiera:

* listę lekarzy,
* szczegóły lekarza,
* formularz dodawania,
* formularz edycji,
* usuwanie lekarza z potwierdzeniem.

Dodatkowo:

* walidacja formularzy,
* komunikaty sukcesu i błędów,
* modal potwierdzenia usunięcia,
* wyszukiwarka lekarzy,
* komponent tabeli lekarzy,
* komponent formularza lekarza,
* TypeScript interfaces,
* połączenie z REST API przez Axios.

---

# Autorzy

Projekt realizowany zespołowo w ramach aplikacji Orthodontic Clinic App.