# deploy.ps1
# Assurez-vous d'être connecté à Azure (az login) avant d'exécuter ce script.
$resourceGroup = "ResilienceOrchestratorRG"
$location = "westeurope"

# Connexion (décommentez si nécessaire)
# az login

# Création du groupe de ressources
az group create --name $resourceGroup --location $location

# Déploiement du template Bicep
az deployment group create --resource-group $resourceGroup --template-file main.bicep
