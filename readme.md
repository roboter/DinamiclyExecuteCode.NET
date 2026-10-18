# ⚡ Dynamically Execute Code .NET (DynCode) - C# & F#

[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.5%2B-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/Language-C%23-239120?logo=csharp&logoColor=white)](https://docs.microsoft.com/dotnet/csharp/)
[![F#](https://img.shields.io/badge/Language-F%23-378BBA?logo=fsharp&logoColor=white)](https://fsharp.org/)
[![Platform](https://img.shields.io/badge/Platform-Windows%20Forms-0078D6?logo=windows&logoColor=white)](https://docs.microsoft.com/dotnet/desktop/winforms/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

A Windows Forms application demonstrating how to **compile and execute C# and F# code dynamically at runtime** in memory using `System.CodeDom.Compiler`, `Microsoft.CSharp.CSharpCodeProvider`, and `FSharp.Compiler.CodeDom.FSharpCodeProvider`.

Select your language (**C#** or **F#**), write custom filtering logic directly in the user interface, compile into an in-memory assembly without writing files to disk, and immediately execute against live data.

---

## 📸 Application Screenshot

![Application Screenshot](Screenshot.png)

The application features an intuitive left-to-right processing pipeline:
1. **Source Country Names (Left)**: The raw input list of items.
2. **Routine & Language Selector (Middle)**: Select between **C#** and **F#**, customize the filtering routine directly in the editor, and click **Execute Code**.
3. **Selected Country Names (Right)**: The dynamic filtered results populated immediately by the in-memory compiled assembly.
4. **Compiler Diagnostics (Bottom)**: Live error console providing line, column, error code, and diagnostic messages for syntax or compilation errors.

---

## 🔄 Code Execution Process

The system compiles user-provided C# or F# code snippets entirely in memory, instantiates the dynamically generated class, and executes the filter method against UI controls via dynamic reflection.

### High-Level Execution Flow

```mermaid
flowchart TD
    subgraph UI["1. User Interface (Form1)"]
        A["User enters Code<br/><code>txtCodeToRun</code>"] --> B["Select Language<br/><b>C#</b> or <b>F#</b> (<code>cmbLanguage</code>)"]
        B --> C["Click 'Execute Code'<br/><code>btnRunCode</code>"]
    end

    subgraph Prepare["2. Language-Specific Code Wrapping"]
        C --> D{"Selected Language?"}
        D -- "C#" --> E["Wrap in C# Class:<br/><code>namespace CountryList</code><br/><code>class SelectCountries</code><br/><code>FilterCountries(ListBox): List&lt;string&gt;</code>"]
        D -- "F#" --> F["Wrap in F# Type:<br/><code>namespace CountryList</code><br/><code>type SelectCountries() =</code><br/><code>FilterCountries(countryList): List&lt;string&gt;</code>"]
    end

    subgraph Compile["3. In-Memory Compilation (CodeHelper)"]
        E --> G["Resolve <code>CSharpCodeProvider</code><br/>CompilerVersion: <code>v4.0</code>"]
        F --> H["Resolve <code>FSharpCodeProvider</code><br/>(via FSharp.Compiler.CodeDom)"]
        G --> I["Configure <code>CompilerParameters</code><br/>• GenerateInMemory = true<br/>• /optimize flag<br/>• System, System.Windows.Forms, System.Core"]
        H --> J["Configure <code>CompilerParameters</code><br/>• GenerateInMemory = true<br/>• /optimize flag<br/>• References + <b>FSharp.Core.dll</b>"]
        I --> K["Invoke <code>provider.CompileAssemblyFromSource(...)</code>"]
        J --> K
    end

    subgraph ResultBranch["4. Compilation Result"]
        K --> L{"Any errors?<br/><code>results.Errors.Count != 0</code>"}
        L -- Yes (Errors) --> M["Return <code>CompilerErrorCollection</code>"]
        L -- No (Success) --> N["Instantiate class via Reflection:<br/><code>results.CompiledAssembly.CreateInstance(...)</code>"]
    end

    subgraph Execution["5. Execution & UI Feedback"]
        M --> O["Format Line, Column & Message<br/>Display in <code>txtErrors</code>"]
        N --> P["Invoke dynamic method:<br/><code>classRef.FilterCountries(lstSource)</code>"]
        P --> Q["Extract filtered country list<br/>Populate <code>lstTarget</code>"]
    end

    classDef ui fill:#24292e,stroke:#4f5d75,stroke-width:1px,color:#ffffff;
    classDef process fill:#1f3a60,stroke:#3182ce,stroke-width:1px,color:#e2e8f0;
    classDef success fill:#1c4424,stroke:#38a169,stroke-width:1px,color:#e6fffa;
    classDef failure fill:#5c1d1d,stroke:#e53e3e,stroke-width:1px,color:#fff5f5;

    class A,B,C,O,Q ui;
    class E,F,G,H,I,J,K,P process;
    class N success;
    class M failure;
```

---

### Sequence Diagram

```mermaid
sequenceDiagram
    autonumber
    actor User as User
    participant Form as Form1 (WinForms UI)
    participant Helper as CodeHelper
    participant Provider as CodeDomProvider (C# / F#)
    participant MemoryAsm as In-Memory Assembly
    participant Target as lstTarget (UI ListBox)

    User->>Form: Selects Language (C# or F#) & enters code snippet
    User->>Form: Clicks "Execute Code"
    Form->>Form: Wraps snippet into CountryList.SelectCountries template (C# or F#)
    Form->>Helper: HelperFunction(classCode, "CountryList.SelectCountries", extraAssemblies, language)
    Helper->>Provider: Instantiate CSharpCodeProvider or FSharpCodeProvider
    Helper->>Provider: CompileAssemblyFromSource(CompilerParameters, classCode)

    alt Compilation Errors Encountered
        Provider-->>Helper: CompilerResults (Errors collection)
        Helper-->>Form: Return CompilerErrorCollection
        Form->>User: Display line, column, error code & text in txtErrors
    else Compilation Succeeded
        Provider-->>MemoryAsm: Emit compiled byte code into RAM
        MemoryAsm-->>Helper: Return instantiated SelectCountries object
        Helper-->>Form: Return dynamic class instance (classRef)
        Form->>MemoryAsm: Dynamic invoke: classRef.FilterCountries(lstSource)
        MemoryAsm-->>Form: Return List<string> (filtered results)
        Form->>Target: Clear items & AddRange(filteredResults)
        Form->>User: Render filtered countries in target ListBox
    end
```

---

## ⚙️ How It Works Under the Hood

### 1. Template Code Wrapping

#### C# Template
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
            // <-- C# USER CODE INJECTED HERE -->
        }
    }
}
```

#### F# Template
```fsharp
namespace CountryList

open System
open System.Windows.Forms
open System.Collections.Generic

type SelectCountries() =
    member this.FilterCountries(countryList: ListBox) : List<string> =
        // <-- F# USER CODE INJECTED HERE (INDENTED) -->
```

---

### 2. In-Memory Dynamic Compilation

[CodeHelper.cs](file:///Users/alekseiprokopov/DinamiclyExecuteCode.NET/CodeHelper.cs) orchestrates the compilation pipeline:

- **Provider Resolution**:
  - **C#**: Uses `CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v4.0" } })`.
  - **F#**: Uses `FSharp.Compiler.CodeDom.FSharpCodeProvider` (resolved dynamically via reflection or `CodeDomProvider.CreateProvider("fs")`).
- **In-Memory Assemblies**: `CompilerParameters.GenerateInMemory = true` keeps compilation inside RAM with zero temporary disk artifacts.
- **Reference Resolution**:
  - Both: `system.dll`, `System.Windows.Forms.dll`, `System.Core.dll`.
  - F#: Automatically includes `FSharp.Core.dll` and `mscorlib.dll`.

---

### 3. Diagnostics & Error Handling
If syntax or type errors occur, `CodeHelper` returns the `CompilerErrorCollection`. The UI parses:
- **Line number**
- **Column number**
- **Error code** (e.g. `CS1002` in C# or `FS0039` in F#)
- **Error description**

and renders them in `txtErrors`.

---

### 4. Dynamic Invocation
Regardless of whether C# or F# was used, the compiled assembly exposes the same contract:
```csharp
dynamic classRef = CodeHelper.HelperFunction(classCode, "CountryList.SelectCountries", new object[] { }, language);
List<string> targetValues = classRef.FilterCountries(lstSource);
```

---

## 💡 Code Examples

### C# Starter Snippet
```csharp
List<string> result = new List<string>();

foreach (string country in countryList.Items)
{
    if (country.StartsWith("C", StringComparison.InvariantCultureIgnoreCase))
    {
        result.Add(country);
    }
}

return result;
```

### F# Starter Snippet
```fsharp
let result = new List<string>()

for item in countryList.Items do
    let country = string item
    if country.StartsWith("C", StringComparison.InvariantCultureIgnoreCase) then
        result.Add(country)

result
```

### F# Functional / Pipeline Snippet
```fsharp
countryList.Items
|> Seq.cast<obj>
|> Seq.map string
|> Seq.filter (fun c -> c.StartsWith("C", StringComparison.InvariantCultureIgnoreCase))
|> ResizeArray
```

> **Tip**: In F#, `ResizeArray<'T>` is an alias for `System.Collections.Generic.List<'T>`, seamlessly matching the expected method signature.

---

## 📂 Project Structure

```text
DinamiclyExecuteCode.NET/
├── CodeHelper.cs         # Unified C# and F# CodeDom compilation engine
├── Form1.cs              # UI logic, language switching & dynamic execution
├── Form1.Designer.cs     # Windows Forms layout (includes cmbLanguage dropdown)
├── Form1.resx            # WinForms UI resources
├── Program.cs            # WinForms application entry point
├── App.config            # .NET runtime & system.codedom F# provider configuration
├── DynCode.csproj        # Visual Studio C# Project (.NET 4.5, PackageReferences)
├── DynCode.sln           # Visual Studio Solution
├── Screenshot.png        # UI preview screenshot
├── LICENSE               # MIT License
└── readme.md             # Project documentation & Mermaid execution diagrams
```

---

## 🚀 Getting Started

### Prerequisites
- Windows OS
- [.NET Framework 4.5](https://dotnet.microsoft.com/download/dotnet-framework/net45) or higher
- Visual Studio 2012 or newer / Rider / MSBuild
- NuGet packages restored:
  - `FSharp.Core` (4.7.2+)
  - `FSharp.Compiler.CodeDom` (1.0.0.1)

### Building & Running

1. **Clone the repository:**
   ```bash
   git clone https://github.com/roboter/DinamiclyExecuteCode.NET.git
   ```

2. **Open the solution:**
   - Open `DynCode.sln` in Visual Studio.
   - NuGet packages (`FSharp.Core` & `FSharp.Compiler.CodeDom`) will restore automatically.

3. **Run the application:**
   - Press <kbd>F5</kbd>.
   - Switch between **C#** and **F#** using the **Language** dropdown.
   - Click **Execute Code** to compile and run on the fly!

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.