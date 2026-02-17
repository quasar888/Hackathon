
## 🏆 Hackathon AI Dev Days

Ce projet a été développé pour le **AI Dev Days Hackathon** (février-mars 2026). Il utilise :

- **Microsoft Agent Framework** : Orchestration multi-agents pour la résilience communautaire.
- **Azure AI Foundry** : Prédiction de la demande de ressources via modèle déployé.
- **Azure Container Apps** : Hébergement scalable des microservices.
- **GitHub Copilot Agent Mode** : Assistance au développement et génération de code.

### Architecture

![Diagramme d'architecture](docs/architecture.png)

### Déploiement

1. Clonez le dépôt.
2. Construisez les images Docker : `docker-compose build`
3. Poussez les images vers Azure Container Registry.
4. Exécutez `deploy\deploy.ps1`.

### Démonstration vidéo

[Lien vers la vidéo YouTube](https://youtu.be/...)
