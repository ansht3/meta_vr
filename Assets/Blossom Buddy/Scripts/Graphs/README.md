# Graph Visualization System

A complete Unity system for visualizing educational concept graphs in 3D space.

## 📁 Files

| File | Purpose |
|------|---------|
| **Models.cs** | Data structures for JSON deserialization |
| **GraphManager.cs** | Main controller - loads JSON and creates visualizations |
| **NodeView.cs** | Individual node behavior and appearance |
| **EdgeView.cs** | Edge (connection line) rendering |
| **Billboard.cs** | Makes text always face the camera |
| **GraphInteractionExample.cs** | Example code showing how to interact with graphs |
| **SETUP_GUIDE.md** | Detailed setup instructions |

## 🚀 Quick Start

1. **Create Node Prefab:**
   - 3D Sphere with TextMeshPro text above it
   - Add `NodeView` component to the sphere
   - Add `Billboard` component to the text
   - Save as prefab

2. **Set Up Scene:**
   - Create empty GameObject
   - Add `GraphManager` component
   - Assign your Node Prefab
   - Configure settings (layout, colors, etc.)

3. **Add JSON File:**
   - Place your JSON file in `Assets/StreamingAssets/graph_data/`
   - Set the filename in GraphManager
   - JSON must follow the required format (see below)
   - **Note**: StreamingAssets folder required for Android/Quest VR support

4. **Press Play!**

## 📋 JSON Format

```json
{
  "adjList": {
    "subject": "Graph Title",
    "nodes": [
      {
        "id": "node1",
        "topic": "Topic Name",
        "small_content": "Brief description",
        "resources": ["url1", "url2"],
        "overall_summary": "Detailed summary"
      }
    ],
    "edges": [
      ["node1", "node2"],
      ["node2", "node3"]
    ]
  }
}
```

## 🎮 Controls (GraphInteractionExample)

When using the example interaction script:
- **R** - Reload graph
- **I** - Display graph information
- **C** - Show connected nodes

## 🎨 Customization

### Layout Options
- **Circular**: Nodes arranged in a circle
- **Grid**: Nodes arranged in a grid pattern

### Visual Settings
- Node prefab appearance (color, size, effects)
- Edge color and width
- Text style and size
- Node spacing and positioning

## 📖 Key Methods

### GraphManager
```csharp
NodeView GetNodeView(string nodeId)                    // Get specific node
Dictionary<string, NodeView> GetAllNodeViews()         // Get all nodes
GraphData GetGraphData()                               // Get raw data
void ReloadGraph()                                     // Reload from JSON
```

### NodeView
```csharp
void Initialize(Node node)        // Set up node with data
Node GetNodeData()                // Get node information
void OnNodeClicked()              // Handle interaction
```

## 🔧 Requirements

- Unity 2020.3 or later
- TextMeshPro (imported via Package Manager)
- .NET Standard 2.0 or later

## 📚 Documentation

See **SETUP_GUIDE.md** for complete setup instructions and troubleshooting.

## 💡 Example Use Cases

- Educational VR/AR applications
- Interactive concept mapping
- Knowledge graph visualization
- Tutorial systems
- Study aids

## ⚡ Performance Notes

- Nodes are instantiated at Start()
- Edges update every frame (minimal overhead)
- Suitable for graphs with up to 100+ nodes
- For larger graphs, consider LOD or culling systems

## 🐛 Common Issues

**Nodes not appearing?**
- Check Node Prefab is assigned
- Verify JSON file path
- Look for errors in Console

**Text not visible?**
- Import TextMeshPro essentials
- Check text color contrast
- Verify Billboard component is added

**Edges not showing?**
- Enable "Show Edges" in GraphManager
- Verify node IDs match in edges array
- Check edge width isn't too small

---

Created for BlossomBuddy - Educational VR Experience

