# 🎒 RappSack  
*A helper library for Dataverse plugins, console apps, and Azure Functions.*


---
![Rapp with Rucksack](Images/RappSack_sqr_tsp_150px.png)

## ❓ Why RappSack?

I have created a bunch of "base classes" over time - sometimes including too much, sometimes too simple. I've called them xxxBag, xxxContainer, xxxUtils etc. etc.

I wanted a single, reliable toolkit that could use **base classes** and **helpers** to work with **Dataverse**, and the _**hardest thing**_ is, of course, to find a proper name for it. I want to have an easy name that explains what it does, a thing that contains an `IOrganizationService`, somewhere to log it, and might have info from the context... Trying to open my mind, letting Ms. Copilot help me. _Bag, Purse, Container, Sack, Grip_...?

A **'sack'** is a part of a **'rucksack'**... I like to use a rucksack, which is easy to carry and great for having everything I need in my backpack. I've used it forever; I never use a briefcase.

**That’s why I created `RappSack` — my personal backpack of essentials for Dataverse development.**

---
## ✅ Overview  
RappSack is a C# library that provides **base classes and utilities** for working with Microsoft Dataverse. It simplifies service access, logging, and context handling across different environments:

- **Plugins**  
- **Console applications**  
- **Azure Functions**

---

## 🧩 Architecture  
- **RappSackCore** – Common functionality for all environments, implements `IOrganizationService`
- **RappSackTracerCore** – Abstract class for unified tracing/logging  
- **RappSackPlugin** – Base class for **Dataverse plugins**, inheriting `RappSackCore`, implements `IPlugin` and `ITracingService`
- **RappSackConsole** – Base class **console apps**, inheriting `RappSackCore`
- **Static helpers** – `RappSackMeta`, `RappSackUtils`
- **CanaryTracer** – Unifying logging even more, especially for plugins

### RappSack for [Microsoft.PowerPlatform.Dataverse](https://www.nuget.org/packages/Microsoft.PowerPlatform.Dataverse.Client)
- **RappSackDVCode** – The same above, but handles newer stuff

---

## 🚀 Quick Start  

### Install  
1. Add RappSack as a submodule:  
```bash
  git submodule add https://github.com/rappen/RappSack.git
```
2. Add those shared project you need in your solution.
3. Add added shared projects as refereces to your project(s).

<!--
Or via NuGet (if published):  
```bash
dotnet add package RappSackCore
```
-->
---
<!--
### Example: Console App  
```csharp
using RappSackConsole;

class Program
{
    static void Main(string[] args)
    {
        var rapp = new RappSackConsole("connection-string");
        rapp.Trace("Starting console app...");
        
        var account = rapp.Service.Retrieve("account", Guid.NewGuid(), new ColumnSet(true));
        rapp.Trace($"Retrieved account: {account["name"]}");
    }
}
```

---
-->
### Example: Plugin  
```csharp
using RappSackPlugin;

public class SamplePlugin : RappSackPluginBase
{
    public override void Execute()
    {
        Trace("Plugin execution started.");
        
        var target = Target;
        Trace($"Target entity: {target.LogicalName}");
        var preimage = ContextEntity[ContextEntityType.PreImage];
        // Your logic here
    }
}
```

---

## 📚 Advanced Usage  
- **Azure Functions integration**  
- **Custom tracing providers**  
- **Metadata helpers (`RappSackMeta`)**

---

## 🤝 Contributing  
Pull requests are welcome! Please open an issue for discussion before major changes.

---

## 📄 License  
![License](https://img.shields.io/github/license/rappen/RappSack)   – see [LICENSE](LICENSE).
