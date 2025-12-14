# Spatial Scholar - AI @ Meta Hackathon

Spatial Scholar is a mixed reality learning companion built for **Meta Quest 3** during the **AI at Meta Hackathon 2025 (Menlo Park, CA)**. It turns a spoken learning request into an **interactive 3D concept map** anchored in your real space, with an AI-guided explanation.

## Demo

[![Watch the demo](https://img.youtube.com/vi/8RkEchuDLX4/maxresdefault.jpg)](https://youtube.com/shorts/8RkEchuDLX4)

## What it does

- **Voice → lesson**: you ask a question out loud (e.g., “Teach me BFS vs DFS”)
- **Concept breakdown**: the system generates a short explanation + a structured concept graph
- **3D visualization**: nodes/edges render as a spatial graph in passthrough
- **Speech + text**: the explanation is spoken and saved for reuse

## Why we built it

Learning content still lives on flat screens (docs, videos, chat windows). We wanted a way to:
- visualize abstract topics in 3D
- show relationships/prereqs spatially
- make explanations feel more “hands-on” and memorable

## How it works (high level)

1. **Speech input** in Unity  
2. A structured prompt is sent to **Llama 4**  
3. The model returns:
   - a short explanation
   - a **JSON concept graph** (nodes + relationships)
4. Unity parses the JSON and renders a **dynamic 3D graph** in passthrough
5. **TTS** reads the explanation out loud

## Tech stack

- **Meta Quest 3**
- **Unity (C#)**
- **Meta XR SDK** (passthrough / spatial visualization)
- **Llama 4** (reasoning + JSON graph generation)
- **Hugging Face + ElevenLabs** (model + speech integration)
- Custom agents:
  - `LlmAgent`
  - `SpeechToTextAgent`
  - `TextToSpeechAgent`
- JSON graph parser + runtime graph layout/renderer

## Current MVP

- Voice-based requests
- LLM explanation output (speech + text)
- JSON concept graph generation
- 3D nodes + edges in passthrough
- Expandable/interactable graph layout

## Next steps

- Hand-based node expansion and richer visual subtopics
- Persistent “knowledge rooms”
- Multi-user sessions
- Light assessments (quick checks / recall prompts)
- Optional integrations (Canvas / Google Classroom)

## Team

Built by Ansh, Riten, Vinay, Ashlie, and Nathan
