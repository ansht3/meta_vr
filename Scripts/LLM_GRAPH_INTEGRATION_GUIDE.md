# LLM → Graph Integration Guide

## Overview

The `LearningGraphAssistant` now writes LLM-generated JSON directly to `example.json` and triggers the `GraphManager` to reload the graph automatically.

## How It Works

### Flow:
1. **User speaks** a question/problem (voice capture via STT)
2. **LLM generates response** with two parts:
   - **Part 1**: Spoken explanation (sent to TTS)
   - **Part 2**: JSON learning graph (after `---JSON---` delimiter)
3. **JSON is written** to `Assets/StreamingAssets/graph_data/example.json`
4. **GraphManager reloads** and displays the new graph with 3D nodes + info panels

### Key Changes Made:

#### 1. Added Graph Integration
```csharp
[Header("Graph Components")]
[SerializeField] private GraphManager graphManager;
[SerializeField] private string outputJsonFileName = "example.json";
```

#### 2. File Writing System
- `WriteJsonToFileAndReloadGraph()` coroutine:
  - Converts LLM format → GraphData format (Models.cs)
  - Writes JSON to StreamingAssets
  - Triggers GraphManager.ReloadGraph()

#### 3. Format Conversion
The LLM generates this format:
```json
{
  "id": "node1",
  "topic": "Topic Name",
  "small_content": ["Point 1", "Point 2"],  // List<string>
  "resources": "https://link.com",          // string
  "overall_summary": "Summary text"
}
```

But `Models.cs` expects:
```json
{
  "id": "node1",
  "topic": "Topic Name",
  "small_content": "Point 1 Point 2",      // string (joined)
  "resources": ["https://link.com"],        // List<string> (split)
  "overall_summary": "Summary text"
}
```

The `ConvertToGraphDataFormat()` method handles this conversion automatically.

## Unity Setup

### 1. Assign Components (Auto-finds if not assigned)
In the LearningGraphAssistant Inspector:
- **STT**: SpeechToTextAgent
- **LLM**: LlmAgent
- **TTS**: TextToSpeechAgent
- **Graph Manager**: GraphManager (in your scene)

### 2. Output JSON File Name
- Default: `example.json`
- This is the file that will be overwritten in `StreamingAssets/graph_data/`

### 3. System Prompt
The current system prompt instructs the LLM to:
- Create 3-6 HIGH-LEVEL conceptual nodes (not step-by-step instructions)
- Use edges as prerequisites: `["prerequisite_id", "node_id"]`
- Separate spoken intro from JSON with `---JSON---`

## Usage

### In VR:
1. Press the button mapped to `StartVoiceCapture()`
2. Speak your question: *"How do I solve graph traversal problems?"*
3. LLM responds with:
   - **Spoken**: "Graph traversal involves... [explanation]"
   - **Visual**: 3D graph appears with nodes like "DFS", "BFS", "Graph Theory"

### From Code:
```csharp
LearningGraphAssistant assistant = FindObjectOfType<LearningGraphAssistant>();
assistant.StartVoiceCapture();
```

## Features

### ✅ What's Working:
- Voice input → LLM → TTS spoken response
- JSON generation and parsing
- Automatic file writing to `example.json`
- Automatic graph reload
- Format conversion between LLM and GraphManager formats
- Info panels display on each node with:
  - Topic (bold header)
  - Content
  - Summary
  - Resources (first 3 links)

### 🎯 The Graph System Includes:
- **Nodes**: 3D spheres with ID text below
- **Info Panels**: Large text blocks above each node (always face camera)
- **Edges**: Lines connecting related concepts (optional)
- **Circular Layout**: Nodes arranged in a circle for better visibility

## File Locations

```
Assets/
├── Blossom Buddy/
│   ├── Scripts/
│   │   ├── LearningGraphAssistant.cs          ← Main integration script
│   │   └── Graphs/
│   │       ├── GraphManager.cs                ← Loads and displays graph
│   │       ├── NodeView.cs                    ← Individual node display
│   │       ├── Models.cs                      ← Data structures
│   │       └── Billboard.cs                   ← Makes text face camera
│   └── Scenes/
│       └── Blossom Buddy.unity                ← Main scene
└── StreamingAssets/
    └── graph_data/
        └── example.json                        ← Generated JSON file
```

## Debugging

### Enable Console Logs:
Set `printResponsesToConsole = true` in Inspector to see:
- Full LLM response
- JSON extraction
- File writing progress
- Graph reload status

### Check File Path:
```csharp
Debug.Log(Path.Combine(Application.streamingAssetsPath, "graph_data", "example.json"));
```

### Manual Reload:
Right-click GraphManager in Hierarchy → `Reload Graph` (Play mode only)

## Customization

### Change Output File:
```csharp
outputJsonFileName = "my_custom_graph.json";
```
(Make sure GraphManager's `jsonFileName` matches)

### Modify System Prompt:
Edit the `systemPrompt` field in Inspector to change:
- Number of nodes (currently 3-6)
- Node detail level
- Edge relationship types

### Adjust Text Display:
In NodeView Inspector:
- **Info Font Size**: Currently `0.8f` (larger for VR)
- **Info Width**: `3.5` (how wide the text box is)
- **Info Height**: `4.0` (how tall the text box is)
- **Info Offset Y**: `1.0` (distance above node)

## Troubleshooting

### Issue: Graph doesn't update
**Solution**: 
1. Check if GraphManager is assigned in Inspector
2. Verify file path in console logs
3. Ensure `example.json` is being written (check file timestamp)

### Issue: JSON format error
**Solution**: 
1. Check console for "Failed to parse JSON"
2. LLM might have wrapped JSON in code fences (```) - the extractor removes these
3. Verify JSON structure matches expected format

### Issue: Nodes appear but no info text
**Solution**: 
1. Check NodeView settings: `showInfo` should be true
2. Verify JSON has `topic`, `small_content`, `overall_summary` fields
3. Check font size isn't too small

### Issue: Graph appears in wrong location
**Solution**: 
1. GraphManager creates graph at origin (0, 0, 0)
2. Adjust `circleRadius` for larger/smaller layouts
3. Parent GraphManager to a positioned object to move entire graph

## Future Enhancements

Potential additions:
- [ ] Save multiple graph versions (history)
- [ ] Interactive node selection (grab/highlight)
- [ ] Dynamic node coloring by category
- [ ] Animated transitions when reloading
- [ ] Voice command to reload previous graphs
- [ ] Export graphs to other formats

## Example LLM Prompt/Response

**User**: "How do I prepare for graph algorithm interviews?"

**LLM Response**:
```
PART 1 (Spoken):
To prepare for graph algorithm interviews, focus on understanding traversal 
patterns, common problem types, and practice implementation. I've created a 
spatial learning graph to help you explore the key concepts.

---JSON---

PART 2 (JSON):
{
  "adjList": {
    "subject": "Graph Algorithms for Technical Interviews",
    "nodes": [
      {
        "id": "graph_basics",
        "topic": "Graph Fundamentals",
        "small_content": ["Vertices, edges, directed vs undirected"],
        "resources": "https://www.example.com/graphs",
        "overall_summary": "Core concepts needed before algorithms"
      },
      {
        "id": "dfs",
        "topic": "Depth-First Search",
        "small_content": ["Stack-based traversal", "Recursive implementation"],
        "resources": "https://www.example.com/dfs",
        "overall_summary": "Essential for backtracking problems"
      }
    ],
    "edges": [
      ["graph_basics", "dfs"]
    ]
  }
}
```

## Notes

- **Vision Mode**: If enabled, LLM can analyze images from passthrough camera
- **Edges**: Represent prerequisites - source must be learned before target
- **Node IDs**: Must be unique within the graph
- **Resources**: Can be comma/newline separated, will be split automatically

