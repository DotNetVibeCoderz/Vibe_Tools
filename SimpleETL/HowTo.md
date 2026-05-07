# How To Use SimpleETL

Welcome to SimpleETL! This guide will explain how to use the visual drag-and-drop designer, how to work with variables, and provide examples of how to configure the most common components.

---

## 1. Getting Started in the Designer
1. **Canvas**: The middle area is where you build your pipeline. Drag components from the left panel (or simply click them) to add them to the canvas.
2. **Order Matters**: Components are executed sequentially from top to bottom.
3. **Properties Panel**: Click any component on the canvas to display its settings on the right panel.

---

## 2. Using Flow Variables
Variables allow you to create dynamic pipelines. You can define them in the **"Flow Variables"** panel on the right.

- **Defining**: Click `Add Variable`, set its name (e.g., `BaseUrl`), select its type (`String`), and give it an initial value (e.g., `https://api.example.com`).
- **Using Variables**: In almost any text field (URL, Query, Scripts), use the double-bracket syntax `{{VariableName}}`.
  - Example: `{{BaseUrl}}/users` will be resolved to `https://api.example.com/users` at runtime.

---

## 3. Component Configuration Examples

Here are examples of what to input into the properties panel for each major component.

### A. Sources (Data Extraction)

**1. SQL Server Source**
- **Connection String**: `Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;` *(Can use variables like `{{DbConn}}`)*
- **Query / Table Name**: `SELECT id, name, email FROM Users WHERE status = 'active'`

**2. CSV Source**
- **Connection String / Path**: `C:\data\input_sales.csv` *(or `./data/input.csv`)*
- *Note: Ensure the CSV file has a header row.*

**3. REST API Source**
- **API URL**: `https://jsonplaceholder.typicode.com/users`
- **HTTP Method**: `GET`
- **Payload**: *(Leave empty for GET)*

---

### B. Transformations (Data Processing)

**1. Filter**
- **Code Editor (Condition)**: `row.age > 18 && row.status == 'active'`
- *Explanation*: The system evaluates this JavaScript condition for every `row`. If it returns `true`, the row is kept.

**2. Set Variable Value**
- **Select Variable**: Choose the variable you want to modify (e.g., `CurrentDate`).
- **New Value**: `2023-10-25` *(You can also use other variables here, e.g., `Prefix-{{OtherVar}}`)*

**3. C# Script**
Allows complex transformations. The pipeline data is passed as `Input` (`IEnumerable<IDictionary<string, object>>`).
```csharp
var list = Input.ToList();
foreach(var row in list)
{
    // Modify existing column or create new one
    row["FullName"] = row["FirstName"].ToString() + " " + row["LastName"].ToString();
    // Use flow variables
    row["ProcessedBy"] = Variables["SystemUser"];
}
return list;
```

**4. Python Script**
The pipeline data is passed as `data` (List of Dictionaries).
```python
def transform(data):
    for row in data:
        # Create a new column
        row["tax"] = float(row["price"]) * 0.1
        # Access flow variable
        row["currency"] = Variables["CurrencyCode"]
    return data
```

**5. PowerShell Script**
Execute native system commands.
- **Command / Script**:
  ```powershell
  Write-Host "Starting ETL pipeline for date: {{CurrentDate}}"
  Copy-Item -Path "C:\temp\file.txt" -Destination "C:\archive\"
  ```

**6. LLM Action (OpenAI)**
*(Requires `OPENAI_API_KEY` in System Environment Variables to execute actual calls).*
- **Model**: `gpt-3.5-turbo`
- **System Prompt**: `You are a data normalizer assistant.`
- **Prompt Instruction**: `Normalize the names in the provided data to Title Case.`

---

### C. Destinations (Data Loading)

**1. SQL Destination**
- **Connection String**: `Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;`
- **Table Name**: `ProcessedUsers`
- *Note: The system will automatically map the keys in your pipeline data to the columns in the SQL table and perform a batch `INSERT INTO ProcessedUsers`.*

**2. CSV Destination**
- **Connection String / Path**: `C:\data\output\processed_data_{{CurrentDate}}.csv`
- *Note: Using a variable in the file path helps generate dynamic file names.*

**3. REST API Destination**
- **API URL**: `https://api.mycompany.com/v1/data-ingest`
- **HTTP Method**: `POST`
- **Payload**:
  ```json
  {
      "batch_id": "{{BatchId}}",
      "timestamp": "{{CurrentTime}}",
      "records": {{data}}
  }
  ```
  *Explanation*: The magic word `{{data}}` will automatically be replaced with the stringified JSON array of your entire processed pipeline data.

---

## 4. Execution & Monitoring
- **Run**: Click the green `Run` button in the top toolbar to start the flow.
- **Stop**: Click the red `Stop` button if a process gets stuck or runs too long.
- **Output Logs**: Keep an eye on the black terminal box at the bottom of the canvas. It provides real-time logging, data counts, and error tracing.

*Enjoy building pipelines effortlessly with SimpleETL!*
