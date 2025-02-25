using System;
using UnityEditor;
using UnityEngine.Serialization;

namespace UnityEngine.Rendering.Universal
{
    /// <summary>
    /// Universal Render Pipeline's Global Settings.
    /// Global settings are unique per Render Pipeline type. In URP, Global Settings contain:
    /// - light layer names
    /// </summary>
    [URPHelpURL("urp-global-settings")]
    [DisplayInfo(name = "URP Global Settings Asset", order = CoreUtils.Sections.section4 + 2)]
    partial class UniversalRenderPipelineGlobalSettings : RenderPipelineGlobalSettings<UniversalRenderPipelineGlobalSettings, UniversalRenderPipeline>
    {
        #region Version system

        private const int k_LastVersion = 3;

#pragma warning disable CS0414
        [SerializeField][FormerlySerializedAs("k_AssetVersion")]
        int m_AssetVersion = k_LastVersion;
#pragma warning restore CS0414

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
#if UNITY_EDITOR
            if (m_AssetVersion != k_LastVersion)
            {
                EditorApplication.delayCall += () => UpgradeAsset(this.GetInstanceID());
            }
#endif
        }

#if UNITY_EDITOR
        static void UpgradeAsset(int assetInstanceID)
        {
            if (EditorUtility.InstanceIDToObject(assetInstanceID) is not UniversalRenderPipelineGlobalSettings asset)
                    return;

            int assetVersionBeforeUpgrade = asset.m_AssetVersion;

            if (asset.m_AssetVersion < 2)
            {
#pragma warning disable 618 // Obsolete warning
                // Renamed supportRuntimeDebugDisplay => stripDebugVariants, which results in inverted logic
                asset.m_StripDebugVariants = !asset.supportRuntimeDebugDisplay;
                asset.m_AssetVersion = 2;
#pragma warning restore 618 // Obsolete warning

                // For old test projects lets keep post processing stripping enabled, as huge chance they did not used runtime profile creating
#if UNITY_INCLUDE_TESTS
                asset.m_StripUnusedPostProcessingVariants = true;
#endif
            }

            if (asset.m_AssetVersion < 3)
            {
                int index = 0;
                asset.m_RenderingLayerNames = new string[8];
#pragma warning disable 618 // Obsolete warning
                asset.m_RenderingLayerNames[index++] = asset.lightLayerName0;
                asset.m_RenderingLayerNames[index++] = asset.lightLayerName1;
                asset.m_RenderingLayerNames[index++] = asset.lightLayerName2;
                asset.m_RenderingLayerNames[index++] = asset.lightLayerName3;
                asset.m_RenderingLayerNames[index++] = asset.lightLayerName4;
                asset.m_RenderingLayerNames[index++] = asset.lightLayerName5;
                asset.m_RenderingLayerNames[index++] = asset.lightLayerName6;
                asset.m_RenderingLayerNames[index++] = asset.lightLayerName7;
#pragma warning restore 618 // Obsolete warning
                asset.m_AssetVersion = 3;
                asset.UpdateRenderingLayerNames();
            }

            // If the asset version has changed, means that a migration step has been executed
            if (assetVersionBeforeUpgrade != asset.m_AssetVersion)
            EditorUtility.SetDirty(asset);
        }

#endif
        #endregion

        /// <summary>Default name when creating an URP Global Settings asset.</summary>
        public const string defaultAssetName = "UniversalRenderPipelineGlobalSettings";

#if UNITY_EDITOR
        internal static string defaultPath => $"Assets/{defaultAssetName}.asset";

        //Making sure there is at least one UniversalRenderPipelineGlobalSettings instance in the project
        internal static UniversalRenderPipelineGlobalSettings Ensure(bool canCreateNewAsset = true)
        {
            UniversalRenderPipelineGlobalSettings currentInstance = GraphicsSettings.
                GetSettingsForRenderPipeline<UniversalRenderPipeline>() as UniversalRenderPipelineGlobalSettings;

            if (RenderPipelineGlobalSettingsUtils.TryEnsure<UniversalRenderPipelineGlobalSettings, UniversalRenderPipeline>(ref currentInstance, defaultPath, canCreateNewAsset))
            {
                if (currentInstance != null && currentInstance.m_AssetVersion != k_LastVersion)
                    UpgradeAsset(currentInstance.GetInstanceID());

                return currentInstance;
            }

            return null;
        }

        public override void Initialize(RenderPipelineGlobalSettings source = null)
            {
            if (source is UniversalRenderPipelineGlobalSettings globalSettingsSource)
                Array.Copy(globalSettingsSource.m_RenderingLayerNames, m_RenderingLayerNames, globalSettingsSource.m_RenderingLayerNames.Length);
        }

#endif

        void Reset()
        {
            UpdateRenderingLayerNames();
        }

        [SerializeField]
        string[] m_RenderingLayerNames = new string[] { "Default" };
        string[] renderingLayerNames
        {
            get
            {
                if (m_RenderingLayerNames == null)
                    UpdateRenderingLayerNames();
                return m_RenderingLayerNames;
            }
        }
        [System.NonSerialized]
        string[] m_PrefixedRenderingLayerNames;
        string[] prefixedRenderingLayerNames
        {
            get
            {
                if (m_PrefixedRenderingLayerNames == null)
                    UpdateRenderingLayerNames();
                return m_PrefixedRenderingLayerNames;
            }
        }
        /// <summary>Names used for display of rendering layer masks.</summary>
        public string[] renderingLayerMaskNames => renderingLayerNames;
        /// <summary>Names used for display of rendering layer masks with a prefix.</summary>
        public string[] prefixedRenderingLayerMaskNames => prefixedRenderingLayerNames;

        [SerializeField]
        uint m_ValidRenderingLayers;
        /// <summary>Valid rendering layers that can be used by graphics. </summary>
        public uint validRenderingLayers => m_ValidRenderingLayers;

        /// <summary>Regenerate Rendering Layer names and their prefixed versions.</summary>
        internal void UpdateRenderingLayerNames()
        {
            // Update prefixed
            if (m_PrefixedRenderingLayerNames == null)
                m_PrefixedRenderingLayerNames = new string[32];
            for (int i = 0; i < m_PrefixedRenderingLayerNames.Length; ++i)
            {
                uint renderingLayer = (uint)(1 << i);

                m_ValidRenderingLayers = i < m_RenderingLayerNames.Length ? (m_ValidRenderingLayers | renderingLayer) : (m_ValidRenderingLayers & ~renderingLayer);
                m_PrefixedRenderingLayerNames[i] = i < m_RenderingLayerNames.Length ? m_RenderingLayerNames[i] : $"Unused Layer {i}";
            }

            // Update decals
            DecalProjector.UpdateAllDecalProperties();
        }

        /// <summary>
        /// Names used for display of light layers with Layer's index as prefix.
        /// For example: "0: Light Layer Default"
        /// </summary>
        [Obsolete("This is obsolete, please use prefixedRenderingLayerMaskNames instead.", true)]
        public string[] prefixedLightLayerNames => new string[0];


        #region Light Layer Names [3D]

        /// <summary>Name for light layer 0.</summary>
        [Obsolete("This is obsolete, please use renderingLayerMaskNames instead.", false)]
        public string lightLayerName0;
        /// <summary>Name for light layer 1.</summary>
        [Obsolete("This is obsolete, please use renderingLayerMaskNames instead.", false)]
        public string lightLayerName1;
        /// <summary>Name for light layer 2.</summary>
        [Obsolete("This is obsolete, please use renderingLayerMaskNames instead.", false)]
        public string lightLayerName2;
        /// <summary>Name for light layer 3.</summary>
        [Obsolete("This is obsolete, please use renderingLayerMaskNames instead.", false)]
        public string lightLayerName3;
        /// <summary>Name for light layer 4.</summary>
        [Obsolete("This is obsolete, please use renderingLayerMaskNames instead.", false)]
        public string lightLayerName4;
        /// <summary>Name for light layer 5.</summary>
        [Obsolete("This is obsolete, please use renderingLayerMaskNames instead.", false)]
        public string lightLayerName5;
        /// <summary>Name for light layer 6.</summary>
        [Obsolete("This is obsolete, please use renderingLayerMaskNames instead.", false)]
        public string lightLayerName6;
        /// <summary>Name for light layer 7.</summary>
        [Obsolete("This is obsolete, please use renderingLayerNames instead.", false)]
        public string lightLayerName7;

        /// <summary>
        /// Names used for display of light layers.
        /// </summary>
        [Obsolete("This is obsolete, please use renderingLayerMaskNames instead.", false)]
        public string[] lightLayerNames => new string[0];

        internal void ResetRenderingLayerNames()
        {
            m_RenderingLayerNames = new string[] { "Default"};
        }

        #endregion
    }
}
