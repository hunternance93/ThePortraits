
using System.Collections.Generic;
using UnityEngine;

public static class GraphicsQuality
{
    public struct GraphicsQualityPreset
    {
        public GraphicsQualityPreset(string displayName, int drawDistance)
        {
            DisplayName = displayName;
            DrawDistance = drawDistance;
        }
        
        public string DisplayName { get; }
        public int DrawDistance { get; }
    }

    public static Dictionary<string, GraphicsQualityPreset> Presets = new Dictionary<string, GraphicsQualityPreset>
    {
        ["ultra"] = new GraphicsQualityPreset("Ultra", 1000),
        ["high"] = new GraphicsQualityPreset("High", 400),
        ["medium"] = new GraphicsQualityPreset("Medium", 300),
        ["low"] = new GraphicsQualityPreset("Low", 150),
        ["vlow"] = new GraphicsQualityPreset("Very Low", 125)
    };

    public const float OVERSEER_MINIMUM_DRAW_DISTANCE = 300f;
}
