# 🚀 Getting Started

This guide will help you set up and run the Azure AI Agents project on your local machine.

---

## 📋 Prerequisites

### Required Software

1. **✅ .NET 10 SDK**
   - Download: [https://dotnet.microsoft.com/download/dotnet/10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
   - Verify installation:
     ```bash
     dotnet --version
     ```
   - Expected output: `10.0.x` or higher

2. **✅ Azure CLI**
   - Download: [https://learn.microsoft.com/cli/azure/install-azure-cli](https://learn.microsoft.com/cli/azure/install-azure-cli)
   - Verify installation:
     ```bash
     az --version
     ```

3. **✅ Code Editor**
   - [Visual Studio 2026](https://visualstudio.microsoft.com/) with ASP.NET workload, or
   - [Visual Studio Code](https://code.visualstudio.com/) with [C# extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)

### Azure Resources

4. **✅ Azure Subscription**
   - Create a free account: [https://azure.microsoft.com/free/](https://azure.microsoft.com/free/)

5. **✅ Azure AI Foundry Project**
   - Create an Azure AI Foundry resource
   - Deploy an Azure OpenAI model (e.g., GPT-4o, GPT-4o-mini)
   - Note your endpoint URL and deployment name

📖 **Documentation:** [Create an Azure AI Foundry project](https://learn.microsoft.com/azure/ai-foundry/how-to/create-projects)

---

## 🔧 Installation

### 1. Clone the Repository

```bash
git clone https://github.com/bovrhovn/azure-demos-agents-from-scratch-to-enterprise.git
cd azure-demos-agents-from-scratch-to-enterprise
```

### 2. Restore Dependencies

```bash
dotnet restore
```

This will download all required NuGet packages for the solution.

---

## 🔑 Authentication Setup

### Step 1: Authenticate with Azure CLI

```bash
az login
```

This opens a browser window for authentication. Sign in with your Azure credentials.

📖 **Documentation:** [Azure CLI Authentication](https://learn.microsoft.com/cli/azure/authenticate-azure-cli)

### Step 2: Set Default Subscription (if you have multiple)

```bash
az account list --output table
az account set --subscription "Your-Subscription-ID"
```

### Step 3: Verify Authentication

```bash
az account show
```

---

## ⚙️ Configuration

### Environment Variables

Set the following environment variables based on your Azure AI Foundry project:

#### **Windows (PowerShell)**
```powershell
$env:ENDPOINT = "https://your-project.services.ai.azure.com"
$env:DEPLOYMENTNAME = "your-deployment-name"
```

#### **Windows (Command Prompt)**
```cmd
set ENDPOINT=https://your-project.services.ai.azure.com
set DEPLOYMENTNAME=your-deployment-name
```

#### **Linux/macOS (Bash)**
```bash
export ENDPOINT="https://your-project.services.ai.azure.com"
export DEPLOYMENTNAME="your-deployment-name"
```

### Finding Your Configuration Values

1. **ENDPOINT**: 
   - Navigate to [Azure AI Foundry](https://ai.azure.com/)
   - Open your project
   - Go to **Settings** → **Project properties**
   - Copy the **Project endpoint** (format: `https://<resource-name>.services.ai.azure.com`)

2. **DEPLOYMENTNAME**:
   - In your AI Foundry project, go to **Deployments**
   - Copy the name of your deployed model (e.g., `gpt-4o`, `gpt-4o-mini`)

📖 **Documentation:** [Azure AI Foundry SDK Setup](https://learn.microsoft.com/azure/ai-services/openai/how-to/create-resource)

---

## 🏃 Running the Applications

### 1. Simple Agent

Run the basic agent that answers general questions:

```bash
cd src\AgentScratchEnterprise\ASE.SimpleAgent
dotnet run
```

**Expected output:**
```
Ask your question [What is the 2nd largest city in Poland by population size?]: 
```

Enter your question and press Enter. The agent will respond with an AI-generated answer.

📖 **Documentation:** [Your First Agent](https://learn.microsoft.com/agent-framework/get-started/your-first-agent)

---

### 2. Agent with Search

Run the agent with document search capabilities:

```bash
cd src\AgentScratchEnterprise\ASE.SimpleAgentSearch
dotnet run
```

**Expected output:**
```
Ask your question [What is the return policy?]: 
```

This agent can search through documents and provide cited responses.

📖 **Documentation:** [RAG with Text Search](https://learn.microsoft.com/agent-framework/concepts/rag)

---

## 🧪 Running Tests

Execute the unit tests to verify everything is working correctly:

```bash
cd tests\ASE.Libraries.Tests
dotnet test --logger "console;verbosity=detailed"
```

**Expected output:**
```
Test Run Successful.
Total tests: 51
     Passed: 51
 Total time: ~8 seconds
```

For more testing information, see [Testing Documentation](./testing.md).

---

## 🔍 Verifying Your Setup

Run this quick verification checklist:

```bash
# ✅ Check .NET version
dotnet --version

# ✅ Check Azure CLI version
az --version

# ✅ Verify Azure login
az account show

# ✅ Verify environment variables (PowerShell)
echo $env:ENDPOINT
echo $env:DEPLOYMENTNAME

# ✅ Restore and build the solution
dotnet build

# ✅ Run tests
cd tests\ASE.Libraries.Tests
dotnet test
```

If all commands execute successfully, your setup is complete! ✅

---

## 🚨 Troubleshooting

### Issue: "DefaultAzureCredential failed to retrieve a token"

**Solution:**
1. Ensure you're logged in: `az login`
2. Check your subscription: `az account show`
3. Verify you have access to the Azure AI Foundry resource

📖 **Documentation:** [DefaultAzureCredential Troubleshooting](https://learn.microsoft.com/dotnet/api/overview/azure/identity-readme#troubleshooting)

---

### Issue: "Environment variable ENDPOINT is not set"

**Solution:**
1. Set the environment variables as shown in the [Configuration](#-configuration) section
2. Verify they're set:
   ```powershell
   echo $env:ENDPOINT
   echo $env:DEPLOYMENTNAME
   ```

---

### Issue: "Model deployment not found"

**Solution:**
1. Verify your deployment name in Azure AI Foundry
2. Ensure the model is fully deployed (not in "Creating" state)
3. Check spelling and casing of `DEPLOYMENTNAME`

---

### Issue: Build fails with ".NET 10 SDK not found"

**Solution:**
1. Download and install .NET 10 SDK: [https://dotnet.microsoft.com/download/dotnet/10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
2. Restart your terminal/IDE
3. Verify: `dotnet --version`

---

## 📚 Next Steps

Now that you have the project running, explore these topics:

1. 📖 [Architecture Overview](./architecture.md) - Understand the system design
2. 📖 [Projects Documentation](./projects.md) - Deep dive into each project
3. 📖 [Configuration Guide](./configuration.md) - Advanced configuration options

---

## 🆘 Getting Help

- 📧 [Report an issue](https://github.com/bovrhovn/azure-demos-agents-from-scratch-to-enterprise/issues)
- 💬 [Join discussions](https://github.com/bovrhovn/azure-demos-agents-from-scratch-to-enterprise/discussions)
- 📖 [Microsoft Learn Documentation](https://learn.microsoft.com/agent-framework/)

---

## 🔗 Additional Resources

- 📘 [Azure AI Projects SDK Documentation](https://learn.microsoft.com/dotnet/api/overview/azure/ai.projects.agents-readme?view=azure-dotnet)
- 📘 [Microsoft Agent Framework Guide](https://learn.microsoft.com/agent-framework/overview/agent-framework-overview)
- 📘 [Azure OpenAI Service](https://learn.microsoft.com/azure/ai-services/openai/)
- 📘 [Azure Identity SDK](https://learn.microsoft.com/dotnet/api/overview/azure/identity-readme)

---

*Ready to build enterprise AI agents? Let's go! 🚀*
