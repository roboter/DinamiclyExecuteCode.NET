# ⚡ Dynamically Execute Code .NET (DynCode)

[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.5%2B-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/Language-C%23-239120?logo=csharp&logoColor=white)](https://docs.microsoft.com/dotnet/csharp/)
[![Platform](https://img.shields.io/badge/Platform-Windows%20Forms-0078D6?logo=windows&logoColor=white)](https://docs.microsoft.com/dotnet/desktop/winforms/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

A Windows Forms application demonstrating how to **compile and execute C# code dynamically at runtime** in memory using `System.CodeDom.Compiler` and `Microsoft.CSharp.CSharpCodeProvider`.

Write custom filtering logic or algorithms directly in the user interface, compile them into an in-memory assembly without touching the disk, and immediately execute the resulting code against live data.

---

## 📸 Application Screenshot

![Application Screenshot](Screenshot.png)

---

## 🔄 Code Execution Process

The system compiles user-provided C# code snippets in memory, dynamically instantiates the generated class, and executes methods against UI data.

### High-Level Execution Flow

```mermaid
flowchart TD
    subgraph UI["1. User Interface (Form1)"]
        A["User enters C# Code<br/><code>txtCodeToRun</code>"] --> B["Click 'Run Code'<br/><code>btnRunCode</code>"]
    end

    subgraph Prepare["2. Code Wrapping"]
        B --> C["Wrap snippet into class template:<br/><code>namespace CountryList</code><br/><code>class SelectCountries</code><br/><code>List&lt;string&gt; FilterCountries(ListBox)</code>"]
    end

    subgraph Compile["3. In-Memory Compilation (CodeHelper)"]
        C --> D["Initialize <code>CSharpCodeProvider</code><br/>CompilerVersion: <code>v4.0</code>"]
        D --> E["Configure <code>CompilerParameters</code><br/>• GenerateExecutable = false<br/>• GenerateInMemory = true<br/>• CompilerOptions = /optimize<br/>• References: System, System.Windows.Forms, System.Core"]
        E --> F["Invoke <code>provider.CompileAssemblyFromSource(...)</code>"]
    end

    subgraph ResultBranch["4. Compilation Result"]
        F --> G{"Any errors?<br/><code>results.Errors.Count != 0</code>"}
        G -- Yes (Errors) --> H["Return <code>CompilerErrorCollection</code>"]
        G -- No (Success) --> I["Instantiate class via Reflection:<br/><code>results.CompiledAssembly.CreateInstance(...)</code>"]
    end

    subgraph Execution["5. Execution & UI Feedback"]
        H --> J["Format Line, Column & Message<br/>Display in <code>txtErrors</code>"]
        I --> K["Invoke dynamic method:<br/><code>classRef.FilterCountries(lstSource)</code>"]
        K --> L["Extract filtered country list<br/>Populate <code>lstTarget</code>"]
    end

    classDef ui fill:#24292e,stroke:#4f5d75,stroke-width:1px,color:#ffffff;
    classDef process fill:#1f3a60,stroke:#3182ce,stroke-width:1px,color:#e2e8f0;
    classDef success fill:#1c4424,stroke:#38a169,stroke-width:1px,color:#e6fffa;
    classDef failure fill:#5c1d1d,stroke:#e53e3e,stroke-width:1px,color:#fff5f5;

    class A,B,J,L ui;
    class C,D,E,F,K process;
    class I success;
    class H failure;
```

---

### Sequence Diagram

```mermaid
sequenceDiagram
    autonumber
    actor User as User
    participant Form as Form1 (WinForms UI)
    participant Helper as CodeHelper
    participant Provider as CSharpCodeProvider
    participant MemoryAsm as In-Memory Assembly
    participant Target as lstTarget (UI ListBox)

    User->>Form: Enters C# snippet and clicks "Run Code"
    Form->>Form: Wrap snippet into CountryList.SelectCountries template
    Form->>Helper: HelperFunction(classCode, "CountryList.SelectCountries", extraAssemblies)
    Helper->>Provider: CompileAssemblyFromSource(CompilerParameters, classCode)

    alt Compilation Errors Encountered
        Provider-->>Helper: CompilerResults (Errors collection)
        Helper-->>Form: Return CompilerErrorCollection
        Form->>User: Display line, column, error code & text in txtErrors
    else Compilation Succeeded
        Provider-->>MemoryAsm: Emit compiled byte code into memory
        MemoryAsm-->>Helper: Return instantiated SelectCountries object
        Helper-->>Form: Return dynamic class instance (classRef)
        Form->>MemoryAsm: classRef.FilterCountries(lstSource)
        MemoryAsm-->>Form: Return List<string> (filtered results)
        Form->>Target: Clear items & AddRange(filteredResults)
        Form->>User: Display filtered countries in target list
    end
```

---

## ⚙️ How It Works Under the Hood

### 1. Template Code Wrapping
The user's code snippet is wrapped inside a complete C# class structure inside [Form1.cs](file:///Users/alekseiprokopov/DinamiclyExecuteCode.NET/Form1.cs#L21-L47):

```csharp
using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace CountryList
{
    public class SelectCountries
    {
        public List<string> FilterCountries(ListBox countryList)
        {
            // <-- USER CODE INJECTED HERE -->
        }
    }
}
```

### 2. In-Memory Dynamic Compilation
[CodeHelper.cs](file:///Users/alekseiprokopov/DinamiclyExecuteCode.NET/CodeHelper.cs#L8-L51) configures the C# compiler:

- **`GenerateInMemory = true`**: Generates the assembly purely in RAM without producing persistent `.dll` files on disk.
- **`CompilerVersion = "v4.0"`**: Utilizes the modern compiler provider version.
- **`CompilerOptions = "/optimize"`**: Enables compiler optimizations.
- **Assembly References**: Automatically links default assemblies (`system.dll`, `System.Windows.Forms.dll`, `System.Core.dll`), plus any optional caller-specified assemblies.

### 3. Diagnostics & Error Handling
If syntax or compilation errors occur, `CodeHelper` returns the `CompilerErrorCollection`. The UI parses each error's:
- **Line number**
- **Column number**
- **Error code** (e.g. `CS1002`)
- **Error description**

and prints them directly into the error console textbox.

### 4. Late-Bound Dynamic Execution
Using C#'s `dynamic` keyword, the application invokes the method without requiring a pre-compiled shared interface:

```csharp
dynamic classRef = CodeHelper.HelperFunction(classCode, "CountryList.SelectCountries", new object[] { });
List<string> targetValues = classRef.FilterCountries(lstSource);
```

---

## 💡 Example Snippet

Here is the default snippet demonstrating how to filter listbox items:

```csharp
List<string> result = new List<string>();

foreach (string country in countryList.Items)
{
    // Filter countries starting with 'C' (case-insensitive)
    if (country.StartsWith("C", StringComparison.InvariantCultureIgnoreCase))
    {
        result.Add(country);
    }
}

return result;
```

---

## 📂 Project Structure

```text
DinamiclyExecuteCode.NET/
├── CodeHelper.cs         # Core compilation logic using CSharpCodeProvider & CodeDom
├── Form1.cs              # Main UI logic, snippet wrapper & dynamic invocation
├── Form1.Designer.cs     # Windows Forms layout & component initialization
├── Form1.resx            # WinForms UI resources & default starter snippet
├── Program.cs            # WinForms application entry point
├── DynCode.csproj        # Visual Studio C# Project configuration (.NET 4.5)
├── DynCode.sln           # Visual Studio Solution
├── Screenshot.png        # UI preview screenshot
├── LICENSE               # MIT License
└── readme.md             # Project documentation & execution architecture
```

---

## 🚀 Getting Started

### Prerequisites
- Windows OS
- [.NET Framework 4.5](https://dotnet.microsoft.com/download/dotnet-framework/net45) or higher
- Visual Studio 2012 or newer / Rider / MSBuild

### Building & Running

1. **Clone the repository:**
   ```bash
   git clone https://github.com/roboter/DinamiclyExecuteCode.NET.git
   ```

2. **Open the solution:**
   - Double-click `DynCode.sln` to open in Visual Studio.

3. **Build & Run:**
   - Press <kbd>F5</kbd> or click **Start** in Visual Studio.
   - Enter or modify the C# code in the text box.
   - Click **Run Code** to compile and execute on the fly!

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.