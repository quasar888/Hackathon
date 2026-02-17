targetScope = 'resourceGroup'

param containerAppsEnvironmentName string = 'resilience-env'
param containerAppsName string = 'resilience-orchestrator'
param location string = resourceGroup().location

resource containerEnv 'Microsoft.App/managedEnvironments@2022-03-01' = {
  name: containerAppsEnvironmentName
  location: location
  properties: {}
}

resource containerApp 'Microsoft.App/containerApps@2022-03-01' = {
  name: containerAppsName
  location: location
  properties: {
    managedEnvironmentId: containerEnv.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
      }
    }
    template: {
      containers: [
        {
          name: 'orchestrator'
          image: 'yourregistry.azurecr.io/orchestrator:latest'
          env: [
            { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
          ]
        }
        // Ajoutez les autres conteneurs ici
      ]
    }
  }
}
