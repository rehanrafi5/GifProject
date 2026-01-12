using System;
using System.Collections.Generic;
using UnityEngine;

public class GameConstants : MonoBehaviour
{
    public const string IMAGE_EXTENSION = ".png";
    public const string MODEL_EXTENSION = ".gltf";

    public const string MAIN_TEXTURE = "_MainTex";
    public const string LINEART_TEXTURE = "_BlendTex";
    public const string OVERLAY_TEXTURE = "_OverlayTex";

    public const string ANIMATOR_SHOWN = "Shown";

    public const string POST_ACTION_FEATURE = "Feature artwork";
    public const string POST_ACTION_DELETE = "Delete";
    public const string POST_ACTION_REPORT = "Report";

    public const int TEXTURE_LAYER_COUNT = 3;
    public const int MAX_INTERESTS = 8;
    public const float GESTURES_DISTANCE = 50f;

    public static Color DRAGON_FRUIT_RED = new Color(204f/255f, 46f/255f, 94f/255f);
    public static Color LIGHT_GREY = new Color(141f/255f, 133f/255f, 127f/255f);

    [Serializable]
    public enum WindowType
    {
        Home = 0,
        PaintingActivity = 20,
        Playback = 21
    }

    public enum PopupType
    {
        Confirmation = 1,
        FeaturePost = 2,
        Loading = 3
    }

    public enum InfoPanelType
    {
        Painting = 0,
        Selected = 1
    }

    public enum LayerChangeType
    {
        Top,
        PlusOne,
        MinusOne,
        Bottom
    }

    public enum Directions
    {
        NW = 0,
        NE = 1,
        SW = 2,
        SE = 3
    }

    [Flags]
    public enum KeywordFilter
    {
        Channels = 0x1,
        Users = 0x2,
        Interests = 0x4,
        Posts = 0x8
    }

    public static string GetTextureLayer(int layerIndex)
    {
        string textureLayer;

        switch (layerIndex)
        {
            case 0:
                textureLayer = MAIN_TEXTURE;
                break;
            case 1:
                textureLayer = LINEART_TEXTURE;
                break;
            case 2:
                textureLayer = OVERLAY_TEXTURE;
                break;
            default:
                textureLayer = MAIN_TEXTURE;
                break;
        }

        return textureLayer;
    }
}
