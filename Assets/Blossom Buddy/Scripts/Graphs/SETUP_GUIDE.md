# Graph Visualization System - Setup Guide

This guide will help you set up the graph visualization system for BlossomBuddy.

## Overview

The graph visualization system creates 3D representations of educational concept graphs. Each node in the graph is represented by a sphere with text displaying its ID, and edges connect related concepts.

## Components

### Scripts

1. **Models.cs** - Defines the data structure for JSON deserialization
2. **GraphManager.cs** - Main controller that loads JSON and creates visualizations
3. **NodeView.cs** - Controls individual node appearance and behavior
4. **EdgeView.cs** - Controls edge (connection line) appearance
5. **Billboard.cs** - Makes text always face the camera

## Setup Instructions

### Step 1: Create the Node Prefab

1. **Create a new GameObject in Unity:**
   - Right-click in Hierarchy → 3D Object → Sphere
   - Name it "NodePrefab"

2. **Add TextMeshPro text above the sphere:**
   - Right-click on NodePrefab → 3D Object → Text - TextMeshPro
   - If prompted, import TMP Essentials
   - Name the text object "NodeText"
   - Position it above the sphere (e.g., Y = 1.5)
   - Adjust the RectTransform:
     - Width: 2
     - Height: 0.5
   - Set Font Size to ~0.5 or adjust as needed
   - Set Alignment to Center/Middle

3. **Add the NodeView component:**
   - Select the NodePrefab in Hierarchy
   - Click "Add Component"
   - Search for "Node View" and add it
   - Drag the NodeText TextMeshPro object to the "Node Text" field in the NodeView component

4. **Add the Billboard component to the text:**
   - Select the NodeText object
   - Click "Add Component"
   - Search for "Billboard" and add it
   - This will make the text always face the camera

5. **Style the sphere (optional):**
   - Create a new Material (Right-click in Project → Create → Material)
   - Name it "NodeMaterial"
   - Adjust color, metallic, smoothness as desired
   - Drag the material onto the sphere

6. **Save the prefab:**
   - Drag the NodePrefab from Hierarchy to your Prefabs folder
   - Delete the NodePrefab from the Hierarchy

### Step 2: Set Up the Scene

1. **Create a GraphManager GameObject:**
   - Right-click in Hierarchy → Create Empty
   - Name it "GraphManager"

2. **Add the GraphManager component:**
   - Select the GraphManager object
   - Click "Add Component"
   - Search for "Graph Manager" and add it

3. **Configure GraphManager settings:**
   - **Prefab Settings:**
     - Drag your NodePrefab to the "Node Prefab" field
   
   - **Edge Settings:**
     - Check "Show Edges" if you want to visualize connections
     - Edge Color: Choose a color (default cyan)
     - Edge Width: 0.05 (adjust as needed)
   
   - **JSON Settings:**
     - JSON File Name: "example.json" (or your JSON file name)
   
   - **Layout Settings:**
     - Node Spacing: 2.0 (for grid layout)
     - Circle Radius: 5.0 (for circular layout)
     - Use Circular Layout: Check this for circular arrangement
   
   - **Parent Transform:**
     - Leave empty - will be created automatically
     - Or create empty GameObjects named "GraphNodes" and "GraphEdges" and assign them

### Step 3: JSON File Format

Your JSON files should be located at:
```
Assets/StreamingAssets/graph_data/[filename].json
```

**Important**: JSON files MUST be in the `StreamingAssets` folder to work on all platforms (especially Android/Quest VR).

**JSON Structure:**
```json
{
  "adjList": {
    "subject": "Your subject title",
    "nodes": [
      {
        "id": "unique_node_id",
        "topic": "Node topic",
        "small_content": "Brief description",
        "resources": [
          "https://resource1.com",
          "https://resource2.com"
        ],
        "overall_summary": "Detailed summary"
      }
    ],
    "edges": [
      ["node_id_1", "node_id_2"],
      ["node_id_2", "node_id_3"]
    ]
  }
}
```

**Important Notes:**
- Each node must have a unique `id`
- Edges connect nodes by their `id` values
- The `id` will be displayed as the text above each sphere
- All fields are required (use empty arrays `[]` if no resources)

### Step 4: Run the Scene

1. Press Play in Unity
2. The graph should automatically load and display
3. Check the Console for any errors or confirmation messages

## Customization

### Changing Layout

In the GraphManager component:
- **Circular Layout:** Arranges nodes in a circle
  - Adjust `Circle Radius` to change the size
- **Grid Layout:** Arranges nodes in a grid
  - Uncheck `Use Circular Layout`
  - Adjust `Node Spacing` to change distance between nodes

### Changing Node Appearance

Edit the NodePrefab:
- Change sphere scale to make nodes larger/smaller
- Modify the material for different colors
- Adjust TextMeshPro font size and style
- Add particle effects or other visual elements

### Changing Edge Appearance

In the GraphManager component:
- Modify `Edge Color` for different line colors
- Adjust `Edge Width` for thicker/thinner lines
- Create a custom material and assign it to `Edge Material`

### Adding Interactions

In `NodeView.cs`, modify the `OnNodeClicked()` method to add custom behavior:

```csharp
public void OnNodeClicked()
{
    if (nodeData != null)
    {
        // Display node details in UI
        // Play audio
        // Highlight related nodes
        // etc.
    }
}
```

To detect clicks, add a collider and script to the NodePrefab that calls `OnNodeClicked()`.

## Troubleshooting

### Nodes not appearing
- Check that the Node Prefab is assigned in GraphManager
- Verify the JSON file path is correct
- Check Console for error messages
- Ensure JSON format is valid

### Text not visible
- Make sure TextMeshPro is imported (Window → TextMeshPro → Import TMP Essentials)
- Check that NodeText is assigned in the NodeView component
- Verify the text color contrasts with the background

### Edges not appearing
- Ensure "Show Edges" is checked in GraphManager
- Verify edge node IDs match actual node IDs in the JSON
- Check that both nodes exist before creating the edge

### JSON parsing errors
- Validate your JSON using a JSON validator (jsonlint.com)
- Ensure all strings use double quotes, not single quotes
- Check for trailing commas (not allowed in JSON)

## API Reference

### GraphManager Methods

```csharp
// Get a specific node view by ID
NodeView GetNodeView(string nodeId)

// Get all node views
Dictionary<string, NodeView> GetAllNodeViews()

// Get the loaded graph data
GraphData GetGraphData()

// Reload the graph from JSON
void ReloadGraph()
```

### NodeView Methods

```csharp
// Initialize with node data
void Initialize(Node node)

// Get the node's data
Node GetNodeData()

// Handle node interaction
void OnNodeClicked()
```

## Example Use Cases

### Loading Different JSON Files

```csharp
// Change the JSON file name in GraphManager
graphManager.jsonFileName = "algebra_basics.json";
graphManager.ReloadGraph();
```

### Accessing Node Information

```csharp
GraphManager gm = FindObjectOfType<GraphManager>();
NodeView node = gm.GetNodeView("eq_start");
if (node != null)
{
    Node data = node.GetNodeData();
    Debug.Log($"Topic: {data.topic}");
    Debug.Log($"Summary: {data.overall_summary}");
}
```

## Next Steps

- Create additional JSON files for different subjects
- Implement UI panels to display node details
- Add node interaction with VR controllers
- Create animations for node highlighting
- Implement progressive reveal of concepts

