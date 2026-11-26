# Emeraude Construction

While building, the game keeps the rotation of the piece you are placing and the snap point chosen with Q/E until
you change them back by hand. This mod adds one key that puts the piece back in its default position.

- **Backspace** (default): rotation back to zero and snapping back to automatic. "Default position" shows on screen.
- With PreciseRotation installed, all three of its axes go back to zero and the vertical axis is selected again
  (its own key only resets the current axis).
- Change the key in `BepInEx/config/emeraude.construction.cfg`, setting `Placement.ResetKey`.

Client-side only: other players and the server do not need it.

---

**FR** — En construction, la touche **Retour arrière** remet la pièce à poser dans sa position par défaut : rotation
à zéro (les trois axes avec PreciseRotation) et accroche automatique. Touche modifiable : `Placement.ResetKey`. Mod
client, inutile sur le serveur.
