// (c) Meta Platforms, Inc. and affiliates. Confidential and proprietary.

using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Load the Blossom Buddy scene
    public void LoadBlossomBuddy()
    {
        SceneManager.LoadScene("Blossom Buddy");
    }

    // Load the LLM Sample scene
    public void LoadLlmSample()
    {
        SceneManager.LoadScene("LLM Sample");
    }

    // Load the Object Detection Sample scene
    public void LoadObjectDetectionSample()
    {
        SceneManager.LoadScene("Object Detection Sample");
    }
}
