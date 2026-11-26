// Emeraude Construction — une touche remet la pièce à poser dans sa position par défaut : droite sur tous les
// axes et accroche automatique.
//
// En mode construction, Q et E font défiler les points d'accroche de la pièce (Player.m_manualSnapPoint,
// -1 = automatique) et la molette la fait tourner (Player.m_placeRotation, en crans de 22,5°). Le jeu garde ces
// choix tant qu'on pose la même pièce. Le mod PreciseRotation du profil remplace la rotation vanilla par sa propre
// rotation sur trois axes (champ statique privé « rotator »), que sa touche ne remet à zéro qu'axe par axe.
using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace EmeraudeConstruction
{
    [BepInPlugin(Guid, Name, Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string Guid = "emeraude.construction";
        public const string Name = "Emeraude Construction";
        public const string Version = "1.0.0";

        internal static ConfigEntry<KeyboardShortcut> ResetKey;
        internal static ManualLogSource Log;

        void Awake()
        {
            Log = Logger;
            ResetKey = Config.Bind("Placement", "ResetKey", new KeyboardShortcut(KeyCode.Backspace),
                "While placing a piece, puts it back in its default position: straight on every axis, automatic snapping.");
            new Harmony(Guid).PatchAll(typeof(Plugin).Assembly);
            Logger.LogInfo($"{Name} {Version} loaded (default position key: {ResetKey.Value})");
        }
    }

    [HarmonyPatch(typeof(Player), "UpdatePlacementGhost")]
    static class ResetPlacementPatch
    {
        const int AutomaticSnap = -1;

        // Seulement l'appel de LateUpdate (flashGuardStone faux) : celui de TryPlacePiece a lieu pendant
        // UpdatePlacement, où PreciseRotation compterait la remise à zéro de m_placeRotation comme une rotation.
        static void Prefix(Player __instance, bool flashGuardStone, GameObject ___m_placementGhost,
            ref int ___m_manualSnapPoint, ref int ___m_placeRotation)
        {
            if (flashGuardStone || __instance != Player.m_localPlayer || ___m_placementGhost == null) return;
            if (KeysUsedElsewhere() || !Plugin.ResetKey.Value.IsDown()) return;

            ___m_manualSnapPoint = AutomaticSnap;
            ___m_placeRotation = 0;
            PreciseRotationReset.Apply();
            __instance.Message(MessageHud.MessageType.Center, DefaultPositionText());
        }

        // Mêmes garde-fous que le jeu pour Q/E, plus la saisie d’un texte (panneau, nom de portail).
        static bool KeysUsedElsewhere() =>
            Hud.IsPieceSelectionVisible() || Minimap.instance.m_mode == Minimap.MapMode.Large ||
            Console.IsVisible() || Chat.instance.HasFocus() || TextInput.IsVisible();

        static string DefaultPositionText() =>
            Localization.instance.GetSelectedLanguage() == "French" ? "Position par défaut" : "Default position";
    }

    // Remet à zéro les trois axes de PreciseRotation et revient à l'axe vertical, par réflexion : le mod n'expose rien.
    // Si son champ change de nom, on le dit une fois dans le journal et seule la rotation vanilla est remise.
    static class PreciseRotationReset
    {
        const string PluginGuid = "com.github.johndowson.PreciseRotation";
        static FieldInfo rotatorField;
        static bool resolved;

        internal static void Apply()
        {
            if (!Resolve()) return;
            object rotator = rotatorField.GetValue(null);
            var type = rotator.GetType();
            foreach (var axis in new[] { "x", "y", "z" }) type.GetField(axis).SetValue(rotator, 0f);
            var currentAxis = type.GetField("CurrentAxis");
            currentAxis.SetValue(rotator, Enum.Parse(currentAxis.FieldType, "Y"));
            rotatorField.SetValue(null, rotator);
        }

        static bool Resolve()
        {
            if (resolved) return rotatorField != null;
            resolved = true;
            if (!BepInEx.Bootstrap.Chainloader.PluginInfos.TryGetValue(PluginGuid, out var info)) return false;
            var field = info.Instance.GetType().GetField("rotator", BindingFlags.Static | BindingFlags.NonPublic);
            var type = field?.FieldType;
            bool usable = type != null && type.IsValueType && type.GetField("CurrentAxis")?.FieldType.IsEnum == true
                && type.GetField("x") != null && type.GetField("y") != null && type.GetField("z") != null;
            if (usable) rotatorField = field;
            else Plugin.Log.LogWarning("PreciseRotation found but its rotation could not be read: only vanilla rotation is reset.");
            return usable;
        }
    }
}
