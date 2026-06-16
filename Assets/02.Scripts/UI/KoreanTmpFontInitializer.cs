using System.Collections.Generic;
using TMPro;
using UnityEngine;

public static class KoreanTmpFontInitializer
{
    private const int PointSize = 90;
    private const string FontStyleName = "Regular";
    private const string FallbackFontAssetName = "Korean TMP Fallback";

    private static readonly string[] KoreanFontFamilyNames =
    {
        "Apple SD Gothic Neo",
        "AppleGothic",
        "Malgun Gothic",
        "Noto Sans CJK KR",
        "Noto Sans KR"
    };

    private static TMP_FontAsset _koreanFallbackFontAsset;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        TMP_FontAsset koreanFontAsset = GetKoreanFallbackFontAsset();
        if (koreanFontAsset == null)
        {
            Debug.LogError("Korean TMP fallback font was not found on this device.");
            return;
        }

        AddGlobalFallback(koreanFontAsset);
        AddDefaultFontFallback(koreanFontAsset);
    }

    private static TMP_FontAsset GetKoreanFallbackFontAsset()
    {
        if (_koreanFallbackFontAsset != null)
        {
            return _koreanFallbackFontAsset;
        }

        for (int i = 0; i < KoreanFontFamilyNames.Length; i++)
        {
            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(KoreanFontFamilyNames[i], FontStyleName, PointSize);
            if (fontAsset == null)
            {
                continue;
            }

            fontAsset.name = FallbackFontAssetName;
            fontAsset.isMultiAtlasTexturesEnabled = true;
            _koreanFallbackFontAsset = fontAsset;
            return _koreanFallbackFontAsset;
        }

        return null;
    }

    private static void AddGlobalFallback(TMP_FontAsset fontAsset)
    {
        if (TMP_Settings.fallbackFontAssets == null)
        {
            TMP_Settings.fallbackFontAssets = new List<TMP_FontAsset>();
        }

        AddFontAsset(TMP_Settings.fallbackFontAssets, fontAsset);
    }

    private static void AddDefaultFontFallback(TMP_FontAsset fontAsset)
    {
        TMP_FontAsset defaultFontAsset = TMP_Settings.defaultFontAsset;
        if (defaultFontAsset == null)
        {
            return;
        }

        if (defaultFontAsset.fallbackFontAssetTable == null)
        {
            defaultFontAsset.fallbackFontAssetTable = new List<TMP_FontAsset>();
        }

        AddFontAsset(defaultFontAsset.fallbackFontAssetTable, fontAsset);
    }

    private static void AddFontAsset(List<TMP_FontAsset> fontAssets, TMP_FontAsset fontAsset)
    {
        for (int i = 0; i < fontAssets.Count; i++)
        {
            if (fontAssets[i] == fontAsset)
            {
                return;
            }
        }

        fontAssets.Add(fontAsset);
    }
}
