Yes, deploying your .NET minimal API to Azure can be simplified by using Azure App Service, which provides a straightforward way to host web applications. Here’s how you can do it:

### 1. **Install the Azure CLI (if not already installed)**
   - Download and install the Azure CLI from [here](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli).
   - Log in to your Azure account:

     ```bash
     az login
     ```

### 2. **Deploy from Visual Studio**
   If you're using Visual Studio, the deployment can be done directly:

   - Right-click your project in Solution Explorer.
   - Select **Publish**.
   - Choose **Azure** as the target.
   - Select **Azure App Service** and follow the prompts to create a new App Service or use an existing one.
   - Click **Publish** to deploy your application.

### 3. **Deploy Using the .NET CLI and Azure CLI**
   - Open your terminal and navigate to your project directory.
   - Publish your project:

     ```bash
     dotnet publish -c Release -o ./publish
     ```

   - Create an Azure App Service using the Azure CLI:

     ```bash
     az webapp create --resource-header <YourResourceHeader> --plan <YourAppServicePlan> --name <YourWebAppName> --runtime "DOTNET|7.0"
     ```

   - Deploy your application to Azure:

     ```bash
     az webapp deploy --resource-header <YourResourceHeader> --name <YourWebAppName> --src-path ./publish
     ```

### 4. **Deploy Using GitHub Actions**
   - If your project is in GitHub, you can set up continuous deployment:

     - Go to your GitHub repository.
     - Navigate to **Actions** > **New workflow**.
     - Choose the **Deploy to Azure WebApp** template.
     - Configure the workflow to deploy your .NET minimal API to Azure.

### 5. **Deploy from Azure Portal**
   - Go to the [Azure Portal](https://portal.azure.com/).
   - Create a new **App Service** instance.
   - Under **Deployment**, choose **Deployment Center**.
   - Select the source control option (e.g., GitHub, Bitbucket) or upload the ZIP file directly.

These methods provide a simple and effective way to deploy your .NET minimal API to Azure without needing to manage infrastructure manually.