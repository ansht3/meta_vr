using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Meta.XR.BuildingBlocks.AIBlocks;
using System.IO;
using System.Text;
using System;
using Meta.XR;
using BlossomBuddy.Graphs;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class KnowledgeGraphData
{
    public AdjacencyList adjList;
    
    public override string ToString()
    {
        if (adjList == null) return "Empty graph";
        return $"Subject: {adjList.subject}\nNodes: {adjList.nodes?.Count ?? 0}\nEdges: {adjList.edges?.Count ?? 0}";
    }
}

[System.Serializable]
public class AdjacencyList
{
    public string subject;
    public List<GraphNode> nodes;
    public List<string[]> edges;
}

[System.Serializable]
public class GraphNode
{
    public string id;
    public string topic;
    public List<string> small_content;
    public string resources;
    public string overall_summary;
    
    public override string ToString()
    {
        return $"[{id}] {topic}: {overall_summary}";
    }
}

public class LearningGraphAssistant : MonoBehaviour
{
    [Header("AI Components")]
    [SerializeField] private SpeechToTextAgent stt;
    [SerializeField] private LlmAgent llm;
    [SerializeField] private TextToSpeechAgent tts;
    
    [Header("Graph Components")]
    [SerializeField] private GraphManager graphManager;

    [Header("Stored Knowledge Data")]
    public List<KnowledgeGraphData> knowledgeGraphHistory = new List<KnowledgeGraphData>();
    
    [Header("Dev Settings")]
    [SerializeField] private bool printResponsesToConsole = true;
    [SerializeField] private bool useVisionMode = true;
    [Tooltip("Additional context to add when vision is used")]
    [SerializeField] private string visionContext = "Analyze the problem or content visible in the image and incorporate it into your learning graph.";

    private PassthroughCameraAccess _ptCam;

    [Header("System Prompt")]
    [TextArea(6, 12)]
        public string systemPrompt =
        "You are a learning assistant. Your response must have TWO parts separated by '---JSON---':\n\n" +
        "PART 1 - SPOKEN INTRODUCTION:\n" +
        "1. One sentence describing the problem or topic\n" +
        "2. If it's a specific problem: provide step-by-step solution instructions\n" +
        "3. End with something like: 'To delve deeper, I have also created a spatial learning graph to cover the topics you need to further understand.'\n\n" +
        "Then write EXACTLY: ---JSON---\n\n" +
        "PART 2 - JSON LEARNING GRAPH:\n" +
        "Create a HIGH-LEVEL conceptual learning graph. Focus on BROAD topics and fundamental concepts, NOT step-by-step instructions.\n" +
        "{\n" +
        "  \"adjList\": {\n" +
        "    \"subject\": \"<broad topic area — e.g. 'Graph Algorithms for Technical Interviews'>\",\n" +
        "    \"nodes\": [\n" +
        "      { \"id\": \"<unique_id>\", \"topic\": \"<BROAD concept like 'DFS', 'Graph Theory', 'Interview Patterns'>\", \"small_content\": [\"<1-2 key insights>\"], \"resources\": \"<relevant links>\", \"overall_summary\": \"<why this concept matters>\" }\n" +
        "    ],\n" +
        "    \"edges\": [[\"<prerequisite_node_id>\", \"<node_id>\"]]\n" +
        "  }\n" +
        "}\n\n" +
        "Create 3-6 nodes representing MAJOR concepts or knowledge areas. EDGES represent PREREQUISITES: [\"<prerequisite_id>\", \"<node_id>\"] means prerequisite_id must be learned BEFORE node_id. Example: [\"graph_basics\", \"dfs\"].\n" +
        "CRITICAL: The JSON MUST have the \"adjList\" wrapper. NO markdown, NO code fences, NO extra text after JSON.";

    private bool isListening = false;
    public bool IsListening => isListening;
    private Coroutine listenTimeoutCoroutine;

        private void Awake()
    {
        // Auto-find AI components if not assigned
        if (stt == null) stt = FindAnyObjectByType<SpeechToTextAgent>();
        if (llm == null) llm = FindAnyObjectByType<LlmAgent>();
        if (tts == null) tts = FindAnyObjectByType<TextToSpeechAgent>();
        if (graphManager == null) graphManager = FindAnyObjectByType<GraphManager>();
        _ptCam = FindAnyObjectByType<PassthroughCameraAccess>();
        
        Debug.Log($"[LearningGraphAssistant] Components found - STT: {stt != null}, LLM: {llm != null}, TTS: {tts != null}, Camera: {_ptCam != null}, GraphManager: {graphManager != null}");
        Debug.Log($"[LearningGraphAssistant] Vision mode: {useVisionMode}");
        Debug.Log($"[LearningGraphAssistant] Graph will be loaded directly from LLM response (no file I/O)");
        Debug.Log($"[LearningGraphAssistant] Current System Prompt:\n{systemPrompt}");
    }

    public void StartVoiceCapture()
    {
        Debug.Log("[LearningGraphAssistant] StartVoiceCapture called");
        
        if (isListening)
        {
            Debug.LogWarning("[LearningGraphAssistant] Already listening, ignoring request");
            return;
        }
        
        if (stt == null)
        {
            Debug.LogError("[LearningGraphAssistant] SpeechToTextAgent not found! Cannot start voice capture.");
            return;
        }
        
        if (llm == null)
        {
            Debug.LogError("[LearningGraphAssistant] LlmAgent not found! Cannot process queries.");
            return;
        }

        isListening = true;
        Debug.Log("[LearningGraphAssistant] Listening for user query...");
        stt.onTranscript.AddListener(OnTranscriptReceived);
        stt.StartListening();
        if (listenTimeoutCoroutine != null)
        {
            StopCoroutine(listenTimeoutCoroutine);
            listenTimeoutCoroutine = null;
        }
        listenTimeoutCoroutine = StartCoroutine(ListenTimeout(12f));
    }

    private void OnTranscriptReceived(string userText)
    {
        if (stt != null)
        {
            stt.onTranscript.RemoveListener(OnTranscriptReceived);
        }
        if (listenTimeoutCoroutine != null)
        {
            StopCoroutine(listenTimeoutCoroutine);
            listenTimeoutCoroutine = null;
        }
        isListening = false;

        if (string.IsNullOrWhiteSpace(userText))
        {
            Debug.LogWarning("[LearningGraphAssistant] Empty transcript, ignoring.");
            return;
        }

        Debug.Log($"[LearningGraphAssistant] Heard: {userText}");
        StartCoroutine(HandleQuery(userText));
    }

    private IEnumerator ListenTimeout(float seconds)
    {
        float elapsed = 0f;
        while (elapsed < seconds && isListening)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (isListening)
        {
            Debug.LogWarning("[LearningGraphAssistant] Listening timed out, stopping.");
            if (stt != null)
            {
                stt.onTranscript.RemoveListener(OnTranscriptReceived);
            }
            isListening = false;
        }
        listenTimeoutCoroutine = null;
    }

    private IEnumerator HandleQuery(string userQuery)
    {
        string llmReply = null;
        void LlmCallback(string response) => llmReply = response;

        llm.OnAssistantReply += LlmCallback;
        
        // Build the prompt with vision context if enabled
        string finalPrompt = $"System: {systemPrompt}\n\nUser: {userQuery}";
        if (useVisionMode && !string.IsNullOrEmpty(visionContext))
        {
            finalPrompt += $"\n\n{visionContext}";
        }
        
        // Use vision mode if enabled and camera is available
        bool canUseVision = useVisionMode && _ptCam != null && _ptCam.IsPlaying;
        
        Debug.Log($"[LearningGraphAssistant] Using vision mode: {canUseVision}");
        Debug.Log($"[LearningGraphAssistant] Sending prompt: {finalPrompt}");
        
        if (canUseVision)
        {
            // Send with passthrough image (multimodal)
            llm.SendPromptWithPassthroughImageAsync(finalPrompt);
        }
        else
        {
            // Fallback to text-only
            llm.SendTextOnlyAsync(finalPrompt);
        }

        while (llmReply == null)
            yield return null;

        llm.OnAssistantReply -= LlmCallback;

        // Print the full LLM response for debugging
        if (printResponsesToConsole)
        {
            Debug.Log("=== LLM RESPONSE START ===");
            Debug.Log($"[LearningGraphAssistant] Response length: {llmReply?.Length ?? 0} characters");
            Debug.Log($"[LearningGraphAssistant] Response contains '{{': {llmReply?.Contains("{") ?? false}");
            Debug.Log($"[LearningGraphAssistant] Response contains '}}': {llmReply?.Contains("}") ?? false}");
            
            // Log in chunks to avoid truncation
            if (!string.IsNullOrEmpty(llmReply))
            {
                int chunkSize = 500;
                for (int i = 0; i < llmReply.Length; i += chunkSize)
                {
                    int length = Mathf.Min(chunkSize, llmReply.Length - i);
                    string chunk = llmReply.Substring(i, length);
                    Debug.Log($"[LearningGraphAssistant] Response chunk {i / chunkSize + 1}: {chunk}");
                }
            }
            Debug.Log("=== LLM RESPONSE END ===");
        }

        // Split response on delimiter
        string delimiter = "---JSON---";
        string spokenPart = llmReply;
        string jsonPart = "";
        
        int delimiterIndex = llmReply.IndexOf(delimiter);
        Debug.Log($"[LearningGraphAssistant] Delimiter '---JSON---' found at index: {delimiterIndex}");
        
        if (delimiterIndex >= 0)
        {
            // Split into spoken and JSON parts
            spokenPart = llmReply.Substring(0, delimiterIndex).Trim();
            jsonPart = llmReply.Substring(delimiterIndex + delimiter.Length).Trim();
            
            Debug.Log($"[LearningGraphAssistant] Spoken part length: {spokenPart.Length}");
            Debug.Log($"[LearningGraphAssistant] JSON part length: {jsonPart.Length}");
        }
        else
        {
            Debug.LogWarning("[LearningGraphAssistant] Delimiter not found, falling back to old method");
            // Fallback: try to find JSON start
            int jsonStart = llmReply.IndexOf('{');
            if (jsonStart > 0)
            {
                spokenPart = llmReply.Substring(0, jsonStart).Trim();
                jsonPart = llmReply.Substring(jsonStart).Trim();
            }
        }

        // (A) Speak ONLY Part 1
        if (tts != null && !string.IsNullOrEmpty(spokenPart))
        {
            Debug.Log("[LearningGraphAssistant] Speaking Part 1 via TTS");
            Debug.Log($"[LearningGraphAssistant] Speaking text: '{spokenPart}'");
            tts.SpeakText(spokenPart);
        }
        else
        {
            Debug.LogWarning("[LearningGraphAssistant] TTS component not found or no spoken part, cannot speak response");
        }

        // (B) Extract JSON from Part 2 only
        string jsonExtract = ExtractJsonOnly(jsonPart);

        if (!string.IsNullOrEmpty(jsonExtract))
        {
            Debug.Log("=== JSON EXTRACTION START ===");
            Debug.Log($"[LearningGraphAssistant] JSON Extract length: {jsonExtract.Length} characters");
            
            // Log JSON in chunks to avoid truncation
            int jsonChunkSize = 300;
            for (int i = 0; i < jsonExtract.Length; i += jsonChunkSize)
            {
                int length = Mathf.Min(jsonChunkSize, jsonExtract.Length - i);
                string chunk = jsonExtract.Substring(i, length);
                Debug.Log($"[LearningGraphAssistant] JSON chunk {i / jsonChunkSize + 1}: {chunk}");
            }
            
            // Parse JSON into object for validation
            KnowledgeGraphData graphData = ParseJsonToObject(jsonExtract);
            if (graphData != null && graphData.adjList != null)
            {
                knowledgeGraphHistory.Add(graphData);
                Debug.Log("=== KNOWLEDGE GRAPH DATA ===");
                Debug.Log($"Subject: {graphData.adjList.subject ?? "null"}");
                Debug.Log($"Nodes: {graphData.adjList.nodes?.Count ?? 0}");
                Debug.Log($"Edges: {graphData.adjList.edges?.Count ?? 0}");
                
                // Convert and load graph directly from object
                LoadGraphFromObject(graphData);
                
                Debug.Log("=== END KNOWLEDGE GRAPH DATA ===");
            }
            else if (graphData != null && graphData.adjList == null)
            {
                Debug.LogError("[LearningGraphAssistant] graphData.adjList is null - JSON parsing incomplete");
            }
            else
            {
                Debug.LogError("[LearningGraphAssistant] Failed to parse JSON into KnowledgeGraphData object");
            }
            Debug.Log("=== JSON EXTRACTION END ===");
        }
        else
        {
            Debug.LogWarning("[LearningGraphAssistant] No valid JSON found in the response.");
            Debug.LogWarning($"[LearningGraphAssistant] Raw response was: {llmReply}");
        }
    }

    private string ExtractJsonOnly(string text)
    {
        int start = text.IndexOf('{');
        int end = text.LastIndexOf('}');
        if (start >= 0 && end >= 0 && end > start)
        {
            return text.Substring(start, (end - start + 1));
        }
        return null;
    }

    private KnowledgeGraphData ParseJsonToObject(string jsonString)
    {
        try
        {
            Debug.Log($"[LearningGraphAssistant] Attempting to parse JSON: {jsonString}");

            // Simple JSON parsing (you could use JsonUtility or Newtonsoft.Json for more robust parsing)
            var graphData = JsonUtility.FromJson<KnowledgeGraphData>(jsonString);

            if (graphData != null)
            {
                Debug.Log($"[LearningGraphAssistant] Successfully parsed JSON into KnowledgeGraphData:\n{graphData.ToString()}");
                return graphData;
            }
            else
            {
                Debug.LogError("[LearningGraphAssistant] JsonUtility.FromJson returned null");
                return null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[LearningGraphAssistant] Failed to parse JSON: {e.Message}");
            Debug.LogError($"[LearningGraphAssistant] JSON string was: {jsonString}");
            return null;
        }
    }

    /// <summary>
    /// Load graph directly from object (no file I/O needed)
    /// </summary>
    private void LoadGraphFromObject(KnowledgeGraphData graphData)
    {
        Debug.Log("[LearningGraphAssistant] === LOADING GRAPH FROM OBJECT ===");
        
        // Convert to GraphData format (Models.cs format)
        BlossomBuddy.Graphs.GraphData convertedGraphData = ConvertToGraphDataFormat(graphData);
        
        if (convertedGraphData == null)
        {
            Debug.LogError("[LearningGraphAssistant] Failed to convert graph data");
            return;
        }
        
        Debug.Log($"[LearningGraphAssistant] Converted graph: {convertedGraphData.adjList.nodes.Count} nodes, {convertedGraphData.adjList.edges.Count} edges");
        
        // Load the graph directly
        if (graphManager != null)
        {
            Debug.Log("[LearningGraphAssistant] Loading graph into GraphManager...");
            graphManager.LoadGraphFromData(convertedGraphData);
            Debug.Log("[LearningGraphAssistant] ✓ Graph loaded successfully");
        }
        else
        {
            Debug.LogWarning("[LearningGraphAssistant] GraphManager not found, cannot load graph");
        }
        
        Debug.Log("[LearningGraphAssistant] === END LOADING GRAPH FROM OBJECT ===");
    }

    private BlossomBuddy.Graphs.GraphData ConvertToGraphDataFormat(KnowledgeGraphData source)
    {
        if (source == null || source.adjList == null)
        {
            Debug.LogError("[LearningGraphAssistant] Source data is null");
            return null;
        }
        
        var converted = new BlossomBuddy.Graphs.GraphData();
        converted.adjList = new BlossomBuddy.Graphs.AdjacencyList();
        converted.adjList.subject = source.adjList.subject;
        converted.adjList.nodes = new List<BlossomBuddy.Graphs.Node>();
        converted.adjList.edges = new List<List<string>>();
        
        // Convert nodes
        if (source.adjList.nodes != null)
        {
            foreach (var sourceNode in source.adjList.nodes)
            {
                var convertedNode = new BlossomBuddy.Graphs.Node();
                convertedNode.id = sourceNode.id;
                convertedNode.topic = sourceNode.topic;
                
                // Convert small_content from List<string> to single string
                if (sourceNode.small_content != null && sourceNode.small_content.Count > 0)
                {
                    convertedNode.small_content = string.Join(" ", sourceNode.small_content);
                }
                else
                {
                    convertedNode.small_content = "";
                }
                
                convertedNode.overall_summary = sourceNode.overall_summary ?? "";
                
                // Convert resources from string to List<string>
                convertedNode.resources = new List<string>();
                if (!string.IsNullOrEmpty(sourceNode.resources))
                {
                    // Split by common delimiters or just add as single resource
                    var resourceParts = sourceNode.resources.Split(new[] { '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in resourceParts)
                    {
                        string trimmed = part.Trim();
                        if (!string.IsNullOrEmpty(trimmed))
                        {
                            convertedNode.resources.Add(trimmed);
                        }
                    }
                }
                
                converted.adjList.nodes.Add(convertedNode);
            }
        }
        
        // Convert edges
        if (source.adjList.edges != null)
        {
            foreach (var edge in source.adjList.edges)
            {
                if (edge != null && edge.Length >= 2)
                {
                    var convertedEdge = new List<string> { edge[0], edge[1] };
                    converted.adjList.edges.Add(convertedEdge);
                }
            }
        }
        
        Debug.Log($"[LearningGraphAssistant] Converted {converted.adjList.nodes.Count} nodes and {converted.adjList.edges.Count} edges");
        return converted;
    }

    private void OnDisable()
    {
        if (listenTimeoutCoroutine != null)
        {
            StopCoroutine(listenTimeoutCoroutine);
            listenTimeoutCoroutine = null;
        }
        if (stt != null)
        {
            stt.onTranscript.RemoveListener(OnTranscriptReceived);
        }
        isListening = false;
    }

#if UNITY_EDITOR
    [ContextMenu("Test Voice Capture")]
    private void __TestVoiceCapture() => StartVoiceCapture();

    [ContextMenu("Test JSON Parsing")]
    private void __TestJsonParsing()
    {
        string testJson = @"{
            ""topic"": ""Test Topic"",
            ""subtopics"": [""Subtopic 1"", ""Subtopic 2"", ""Subtopic 3""],
            ""summary"": ""This is a test JSON for development purposes."",
            ""relations"": [
                {""from"": ""Subtopic 1"", ""to"": ""Subtopic 2"", ""type"": ""related""}
            ]
        }";

        Debug.Log("=== TEST JSON PARSING ===");
        KnowledgeGraphData testData = ParseJsonToObject(testJson);
        if (testData != null)
        {
            knowledgeGraphHistory.Add(testData);
            Debug.Log($"Test data added to history. Total entries: {knowledgeGraphHistory.Count}");
            Debug.Log($"Test data: {testData}");
        }
        Debug.Log("=== END TEST JSON PARSING ===");
    }

    [ContextMenu("Print Knowledge History")]
    private void __PrintKnowledgeHistory()
    {
        Debug.Log("=== KNOWLEDGE HISTORY ===");
        Debug.Log($"Total entries: {knowledgeGraphHistory.Count}");
        for (int i = 0; i < knowledgeGraphHistory.Count; i++)
        {
            Debug.Log($"Entry {i + 1}: {knowledgeGraphHistory[i]}");
        }
        Debug.Log("=== END KNOWLEDGE HISTORY ===");
    }

    [ContextMenu("Clear Knowledge History")]
    private void __ClearKnowledgeHistory()
    {
        knowledgeGraphHistory.Clear();
        Debug.Log("[LearningGraphAssistant] Knowledge history cleared.");
    }
#endif
}