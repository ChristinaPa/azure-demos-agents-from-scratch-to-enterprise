# 📚 Azure AI Agents - From Scratch to Enterprise

Welcome to the comprehensive documentation for the **Azure AI Agents** project. This project demonstrates building AI agents from simple implementations to enterprise-ready solutions using Azure AI services.

---

## 📖 Table of Contents

1. [Getting Started](./getting-started.md) - Prerequisites, installation, and setup
2. [Architecture](./architecture.md) - System design and component overview
3. [Projects](./projects.md) - Detailed information about each project
4. [Configuration](./configuration.md) - Environment variables and settings
5. [Testing](./testing.md) - Testing strategy and running tests
6. [Test Summary](./test-summary.md) - Comprehensive test results and coverage
7. [Troubleshooting](./troubleshooting.md) - Common issues and solutions
8. [📊 Diagrams](./diagrams.md) - Mermaid diagrams: architecture, flows, class relationships, test coverage

---

## 🚀 Quick Links

### Official Microsoft Documentation

- 📘 [Azure AI Projects SDK for .NET](https://learn.microsoft.com/dotnet/api/overview/azure/ai.projects.agents-readme?view=azure-dotnet)
- 📘 [Microsoft Agent Framework Overview](https://learn.microsoft.com/agent-framework/overview/agent-framework-overview)
- 📘 [Azure AI Foundry Documentation](https://learn.microsoft.com/azure/ai-foundry/)
- 📘 [DefaultAzureCredential Authentication](https://learn.microsoft.com/dotnet/api/azure.identity.defaultazurecredential)
- 📘 [.NET 10 Getting Started](https://dotnet.microsoft.com/learn/dotnet/hello-world-tutorial/intro)

### Key Technologies

- 🔷 [Azure AI Projects](https://learn.microsoft.com/azure/ai-services/)
- 🔷 [Azure OpenAI Service](https://learn.microsoft.com/azure/ai-services/openai/)
- 🔷 [Microsoft.Extensions.AI](https://learn.microsoft.com/dotnet/api/microsoft.extensions.ai)
- 🔷 [Azure Identity SDK](https://learn.microsoft.com/dotnet/api/overview/azure/identity-readme)

---

## 🎯 Project Overview

This solution contains three main projects that demonstrate the progression from a simple agent to enterprise-ready implementations:

### 1. **ASE.SimpleAgent** 🤖
A foundational AI agent implementation using Azure AI Projects SDK.

**Key Features:**
- Direct Azure AI Projects integration
- Simple question-answer capability
- Azure credential authentication
- Console-based interaction

[Learn more →](./projects.md#1-asesimpleagent)

### 2. **ASE.SimpleAgentSearch** 🔍
An enhanced agent with document search and RAG (Retrieval-Augmented Generation) capabilities.

**Key Features:**
- Text search provider integration
- RAG pattern implementation
- Document citation support
- Context-aware responses

[Learn more →](./projects.md#2-asesimpleagentsearch)

### 3. **ASE.Libraries** 📦
Shared library containing reusable components and adapters.

**Key Features:**
- Document search adapter
- Mock search backend
- Extensible architecture
- Comprehensive test coverage

[Learn more →](./projects.md#3-aselibraries)

---

## 🛠 Technology Stack

| Component | Technology | Version |
|-----------|-----------|---------|
| **Framework** | .NET | 10.0 |
| **AI SDK** | Azure.AI.Projects | 2.0.0 |
| **Authentication** | Azure.Identity | 1.21.0 |
| **Agent Framework** | Microsoft.Agents.AI.Foundry | 1.1.0 |
| **UI** | Spectre.Console | 0.55.0 |
| **Testing** | xUnit | 2.9.3 |

---

## 📋 Prerequisites

Before you begin, ensure you have the following:

- ✅ [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) installed
- ✅ An [Azure subscription](https://azure.microsoft.com/free/)
- ✅ [Azure AI Foundry project](https://learn.microsoft.com/azure/ai-foundry/how-to/create-projects) created
- ✅ [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) installed (for authentication)
- ✅ Visual Studio 2026 or VS Code with C# extension

[Detailed setup instructions →](./getting-started.md)

---

## ⚡ Quick Start

1. **Clone the repository**
   ```bash
   git clone https://github.com/bovrhovn/azure-demos-agents-from-scratch-to-enterprise.git
   cd azure-demos-agents-from-scratch-to-enterprise
   ```

2. **Set environment variables**
   ```bash
   set ENDPOINT=https://your-project.services.ai.azure.com
   set DEPLOYMENTNAME=your-deployment-name
   ```

3. **Authenticate with Azure**
   ```bash
   az login
   ```

4. **Run a simple agent**
   ```bash
   cd src\AgentScratchEnterprise\ASE.SimpleAgent
   dotnet run
   ```

[Complete setup guide →](./getting-started.md)

---

## 🧪 Testing

The project includes comprehensive unit tests covering all library components.

```bash
cd tests\ASE.Libraries.Tests
dotnet test
```

**Test Results:**
- ✅ 51 tests passed
- ✅ 0 tests failed
- ✅ Comprehensive coverage of all library components

[Testing documentation →](./testing.md) | [Detailed test summary →](./test-summary.md)

---

## 🏗 Enterprise Features Roadmap

This project demonstrates the progression to enterprise-ready AI agents:

- ✅ **Simple Agent** - Basic question-answer functionality
- ✅ **Search Integration** - RAG with document search
- 🔄 **Vector Search** - Semantic search capabilities (coming soon)
- 🔄 **Caching** - Response caching for performance (coming soon)
- 🔄 **API Gateway** - Centralized API management (coming soon)
- 🔄 **Monitoring** - Application Insights integration (coming soon)
- 🔄 **Security** - Advanced authentication and authorization (coming soon)

---

## 🤝 Contributing

Contributions are welcome! Please open an issue or pull request on GitHub.

---

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](../LICENSE) file for details.

---

## 🆘 Support

- 📧 [Report an issue](https://github.com/bovrhovn/azure-demos-agents-from-scratch-to-enterprise/issues)
- 💬 [Discussions](https://github.com/bovrhovn/azure-demos-agents-from-scratch-to-enterprise/discussions)
- 📖 [Microsoft Learn Documentation](https://learn.microsoft.com/agent-framework/)

---

## 📚 Additional Resources

### Learning Resources
- 📖 [Azure AI Fundamentals](https://learn.microsoft.com/training/paths/get-started-with-artificial-intelligence-on-azure/)
- 📖 [Build AI Apps with .NET](https://learn.microsoft.com/dotnet/ai/)
- 📖 [Responsible AI Principles](https://learn.microsoft.com/azure/ai-services/responsible-use-of-ai-overview)

### Community
- 💻 [.NET Community](https://dotnet.microsoft.com/platform/community)
- 💬 [Azure Developer Community](https://techcommunity.microsoft.com/t5/azure-developer-community-blog/bg-p/AzureDevCommunityBlog)

---

*Built with ❤️ using Azure AI and .NET*
