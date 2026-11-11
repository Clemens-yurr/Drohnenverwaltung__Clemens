---
name: dotnet-pr-artifacts
description: Prüft .NET-Änderungen auf Build-Ausgaben in bin/ und obj/ und hält solche Dateien aus Feature-PRs heraus.
---

# .NET PR-Artefakte

Vor dem Commit eines .NET-Issues:

- Prüfe `git status --short` und den PR-Diff auf Dateien unter `bin/` und `obj/`.
- Führe Builds und Tests vor der Bereinigung aus und notiere, ob sie erfolgreich waren.
- Beachte: `.gitignore` verhindert neue ungetrackte Build-Ausgaben, entfernt aber keine bereits versionierten Dateien aus Git.
- Stelle generierte Dateien nicht pauschal wieder her oder lösche sie. Vergleiche betroffene Pfade mit `origin/main` und entferne nur Build-Artefakte, die durch den aktuellen Feature-Branch zusätzlich in den Diff gekommen sind.
- Nimm keine Build-Ausgaben in einen Commit auf. Verwende gezielte Pfade beim Stagen statt blind `git add -A`.
- Prüfe nach der Bereinigung erneut `git status --short` und den PR-Diff. Der PR soll die erforderlichen Quell-, Test- und Projektdateien enthalten, keine neu hinzugefügten Build-Ausgaben.
- Wenn die Bereinigung Projektdateien oder Testergebnisse unsicher macht, stoppe und erkläre den konkreten Konflikt, statt Dateien pauschal zu löschen.
