# 🌌 Spatial Scholar — AI at Meta Hackathon

## 🏆 Overview

All four of us had incredible opportunity to be selected as **50 students across the nation** to participate in the **AI @ Meta Hackathon** at Meta’s Global HQ in **Menlo Park, CA**, where we explored how **VR, AI, and mixed reality** can make life easier and more meaningful.

---

## 🚀 Mission

Bring curiosity and intuition back to learning by turning abstract concepts into **immersive, hands-on experiences** in mixed reality.

The hard parts of learning — breaking down concepts, organizing knowledge, and visualizing relationships — should happen autonomously.

---

## 🎯 The Problem

Learning is still stuck in 2D — PDFs, videos, flashcards, and chatbot windows. This makes it hard for learners to:
- Visualize abstract concepts  
- Understand how topics connect  
- Stay engaged long enough to retain information  
- Get explanations tailored to their mental model  

AI tutors exist — but they still live on flat screens. There’s no spatial memory, no immersion, and no embodied understanding.

---

## ✅ Our Solution

We built a **VR learning companion** using **Meta Quest 3** that listens to what you want to learn, understands your voice, and anchors **immersive, explorable spatial experiences** directly into your environment. In our application, we enable any spoken request to turn into an **interactive 3D knowledge map** — automatically.

For example, a student could ask:  
> “Help me understand how neural networks learn.”  
And watch concepts literally come to life — nodes appear across the room, highlighting prerequisite knowledge with real examples in a spatial graph while an AI assistant walks you through the explanation using **Meta’s Passthrough Camera API**.

---

## 🧠 How It Works

1. **Voice Input**  
   The learner says something like:  
   “I need help learning binary trees.”  
   “Teach me BFS and DFS.”  

2. **LLM Understanding (Llama Integration)**  
   The system sends the request to a structured Llama prompt.  

3. **Autonomous Topic Breakdown**  
   The LLM generates:  
   - ✅ A spoken explanation  
   - ✅ A structured JSON concept graph (nodes + relationships)  

4. **3D Rendering in VR**  
   The concept graph is visualized as floating, interactive nodes in the user’s passthrough space.  

5. **Speech + Text Output**  
   Explanations are spoken aloud and stored for reuse or chaining.  

---

## 🎥 Demo

[![Watch the demo](https://img.youtube.com/vi/8RkEchuDLX4/maxresdefault.jpg)](https://youtube.com/shorts/8RkEchuDLX4)

---

## 🔧 Tech Stack

**Hardware**  
- **Meta Quest 3** — primary MR device for spatial tracking, hand gestures, and passthrough visualization  

**Core Technologies**  
- **Unity (C#)** — real-time speech recognition, TTS, and graph rendering  
- **Meta XR SDK** — Passthrough, Voice, and AI Blocks for spatial visualization  
- **Custom Agents** — LlmAgent, SpeechToTextAgent, TextToSpeechAgent for dialogue, voice, and response orchestration  
- **JSON Graph Parser** — converts AI outputs into structured 3D graph data  

**AI**  
- **Llama 4** — contextual reasoning, dialogue generation, and knowledge extraction  
- **Hugging Face & ElevenLabs** — model integration for reasoning and speech synthesis  

**Visualization**  
- **3D Dynamic Graphs** — nodes and edges rendered in real time within the passthrough environment  
- **Spatial Anchoring** — graphs positioned in user’s real-world space for immersive learning  

---

## 🔥 Why It’s Different

This isn’t VR as a gimmick — it uses spatial computing where it’s strongest:
- 🧠 Spatial memory & embodied cognition  
- 🌐 Topic mapping in 3D space  
- 🎙️ Voice-driven knowledge generation  
- 🛠️ Autonomous visualization of abstract ideas  

We don’t force learners to adapt — the system adapts to them.

---

## 📌 Example Use Cases

- Interactive explanations of LeetCode algorithms  
- Physics and math visual breakdowns  
- Biology & anatomy relationships  
- Language learning mind maps  
- Onboarding / training modules  

---

## ✅ MVP Features (Current)

- ✅ Voice-based topic requests  
- ✅ LLM explanations (speech + text)  
- ✅ JSON concept graph generation  
- ✅ 3D node/edge visualization  
- ✅ Interactive, expandable layout  

---

## 🎯 Next Steps

- Integration with learning services (Canvas, Google Classroom)  
- Hand-based node expansion  
- Visual subtopics & diagrams  
- Persistent “knowledge rooms”  
- Multi-user collaborative sessions  
- Interactive assessments  

---

## 🤝 Team Statement


We believe learning should feel like exploration, not obligation.  
By combining **AI and spatial computing**, we’re making knowledge something you can **see, move through, and remember**.
