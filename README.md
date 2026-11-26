# Emeraude Construction — sources

Mod BepInEx (Harmony seul, client) pour Valheim : une touche remet la pièce à poser dans sa position par défaut, dans
un préfixe de `Player.UpdatePlacementGhost` (appel de `LateUpdate` seulement) : `m_manualSnapPoint` à -1 (accroche
automatique), `m_placeRotation` à 0 et, si PreciseRotation est chargé, son champ statique privé `rotator` (axes x, y, z
à 0, axe courant Y) par réflexion. Description publique : [thunderstore/README.md](thunderstore/README.md). Build et
publication comme emeraude-repos.
