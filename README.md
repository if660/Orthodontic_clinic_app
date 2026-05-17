# Orthodontic Clinic App

Aplikacja webowa do zarządzania kliniką ortodontyczną stworzona w technologii ASP.NET Core Web API + Vue 3.

Projekt umożliwia zarządzanie pacjentami kliniki, w tym:

* wyświetlanie listy pacjentów,
* dodawanie pacjentów,
* edycję danych pacjentów,
* usuwanie pacjentów,
* podgląd szczegółów pacjenta.

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
* routing frontendowy
* dynamiczne odświeżanie danych
* walidacja formularzy
* loading states
* komunikaty sukcesu i błędów
* wyszukiwarka pacjentów
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

# Autorzy

Projekt realizowany zespołowo w ramach aplikacji Orthodontic Clinic App.
