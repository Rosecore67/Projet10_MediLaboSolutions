# MediLaboSolutions

Projet .NET de type microservices avec Docker, JWT Auth et Gateway API. Il permet la gestion des patients, des notes médicales, l'analyse de risques de diabète, et propose une interface web centralisée.

## 🌐 Vue d'ensemble des services

- **BackPatient** : Gestion des patients (SQL Server).
- **NoteService** : Gestion des notes (MongoDB).
- **DiabeteCheck** : Analyse du risque de diabète selon les notes.
- **IdentityAuthService** : Authentification via JWT.
- **GatewayAPI** : Gateway centrale via Ocelot.
- **MicroFrontEnd** : Interface utilisateur ASP.NET MVC.

## 🧵 Démarrage rapide avec Docker Compose

### 1. Prérequis
- Docker & Docker Compose installés

### 2. Lancer les services
```bash
docker-compose up --build
```
> Les services seront disponibles sur les ports suivants :
> - Front : [http://localhost:5055](http://localhost:5055)
> - Gateway : [http://localhost:8080](http://localhost:8080)

### 3. Identifiants de connexion
- **Nom d'utilisateur** : `admin`
- **Mot de passe** : `adminpass123`

> Ces identifiants sont définis dans `IdentityAuthService/appsettings.Docker.json` et peuvent être modifiés.

---

## 🚫 Authentification JWT

Toutes les APIs (hors authentification) sont **protégées** par JWT. L'utilisateur doit se connecter via le frontend pour obtenir un token.

- Le token JWT est stocké en session par `MicroFrontEnd`.
- En l'absence de token, l'utilisateur est **redirigé vers /Auth/Login**.

### ⚠️ Important :
- La clé secrète JWT est définie dans `appsettings.Docker.json` de chaque service. Elle **doit être modifiée en production** :
```json
"SecretKey": "MaSuperCleUltraSecretePourJwtToken123!"
```

---

## 📁 Structure Docker Compose

```yaml
services:
  sqlserver:      # Base de données MSSQL pour BackPatient
  mongo:          # Base MongoDB pour les notes
  backpatient:    # API patients
  noteservice:    # API notes (MongoDB)
  diabetecheck:   # API analyse de risque diabète
  identityauthservice: # Authentification JWT
  microfrontend:  # Interface utilisateur MVC
  gateway-api:    # Ocelot Gateway
```

Tous les services communiquent via un réseau `bridge` Docker : `medilabo-net`.

### Initialisation automatique
- **BackPatient** initialise des patients dès le premier lancement.
- Cela permet de **créer des notes dès le départ** dans le front sans erreurs.

---

## 📊 Fonctionnalités clés

| Microservice       | Description |
|--------------------|-------------|
| BackPatient        | CRUD patients, base MSSQL, JWT secure |
| NoteService        | CRUD notes médicales MongoDB, JWT secure |
| DiabeteCheck       | Analyse du risque en fonction des notes |
| IdentityAuthService| Auth via JWT, vérification d'identifiants admin |
| GatewayAPI         | Routeur Ocelot central, sécurisé JWT |
| MicroFrontEnd      | MVC, session avec JWT, auth redirection |

---

## 🧬 Sécurité
- JWT signé avec clé secrète (modifiables dans les settings).
- Validation du token sur **toutes les API** (via Middleware).
- Routes backend inaccessibles sans token.
- Frontend MVC redirige si absence de token.

---

## 🎡 Program.cs de chaque service

Chaque service configure :
- Sa propriétaire Auth avec JWT
- L'environnement `Docker` via `appsettings.Docker.json`
- Swagger actif en mode dev (sauf frontend)
- HttpClient si besoin (ex : DiabeteCheck, Notes)

Les JWT sont validés avec :
```csharp
ValidateIssuer = true;
ValidateAudience = true;
ValidateLifetime = true;
ValidateIssuerSigningKey = true;
```

---

## 🌱 Approche Green Code

Le projet **MediLaboSolutions** intègre plusieurs bonnes pratiques du Green Code afin de favoriser un développement responsable et durable :

- **Optimisation du stockage des données** :  
  Les bases SQL et NoSQL sont conçues pour ne stocker que les données essentielles. Les patients sont normalisés en 3NF, et les notes médicales n'enregistrent que les informations utiles au diagnostic.

- **Réduction des appels inutiles** :  
  L'application utilise un front centralisé et un système d'authentification JWT pour éviter les appels redondants vers les API. Les requêtes sont uniquement déclenchées si l'utilisateur est authentifié.

- **Déploiement containerisé et léger** :  
  Chaque microservice est isolé dans un conteneur Docker, ce qui permet une gestion fine des ressources et une meilleure scalabilité. Cela limite la consommation énergétique inutile.

- **Responsabilité de traitement répartie** :  
  Chaque microservice est spécialisé et indépendant (principe du SRP), ce qui permet de ne faire tourner que les composants nécessaires, sans surcharger inutilement le système.

---



## 🎓 Projet à but pédagogique
Ce projet a été réalisé dans le cadre d'un exercice de formation.
L'objectif est de montrer :
- L'architecture microservices avec .NET 7
- L'utilisation de JWT et d'une Gateway Ocelot
- La communication entre services (Mongo, MSSQL)
- L'utilisation de Docker pour tout orchestrer

---

## 🚀 Améliorations possibles
- Peuplage automatisé de NoteService (via init script)
- Ajout de tests unitaires
- Authentification plus fine (rôles, permissions)
- Monitoring via Prometheus + Grafana

---

## 📅 Auteur
Projet réalisé par RoseCore67 dans le cadre du module Projet10 - MediLaboSolutions.

