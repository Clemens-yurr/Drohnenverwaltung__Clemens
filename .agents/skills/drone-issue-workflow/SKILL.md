---
name: drone-issue-workflow
description: Bearbeitet genau ein GitHub-Issue im Drohnenverwaltungsprojekt mit OpenSpec, eigenem Feature-Branch, Prüfung, Commit und PR.
---

# Drone Issue Workflow

Bei einem GitHub-Issue im Drohnenverwaltungsprojekt:

1. Prüfe den aktuellen Branch und Arbeitsbaum. Wechsle nicht mit ungesicherten Änderungen den Branch.
2. Lies das Issue und die dazugehörige OpenSpec-Aufgabe. Bearbeite nur dieses Issue.
3. Arbeite in einem eigenen Branch `feature/issue-<nummer>-<kurzer-name>`, der auf dem aktuellen `origin/main` basiert. Erstelle keine zweite Projektkopie.
4. Halte dich an die Projektregeln in `AGENTS.md`. Ändere keine unabhängigen Dateien oder Aufgaben.
5. Führe die im Issue geforderten Builds und Tests aus. Melde fehlende SDKs und fehlgeschlagene Prüfungen offen; behaupte keinen Erfolg, wenn Befehle fehlschlagen.
6. Prüfe vor dem Commit `git status` und den Diff. Nimm nur die beabsichtigten Quelldateien und Projektdateien auf; verwende nicht unbesehen `git add .` oder `git add -A`.
7. Erstelle einen aussagekräftigen Commit, pushe den Feature-Branch und erstelle einen PR nach `main`, der das Issue korrekt referenziert.
8. Mische keine anderen Issues in denselben Branch oder PR. Merge den PR nur, wenn der Benutzer das in seiner aktuellen Aufgabe verlangt.
