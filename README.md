# SWEN 4 Tourplanner

## Technische Schritte
Aufgrund unserer anfänglichen Unsicherheit bezüglich des MVVM-Patterns haben wir mit der Erstellung der View und des ViewModels begonnen, um die grundlegenden Prinzipien dieses Patterns zu erfüllen. Dabei ist das "MainViewModel" immer umfangreicher geworden, und wir vermuten, dass es eigentlich aufgeteilt werden sollte.

Anschließend haben wir die View und das ViewModel mittels Bindings verknüpft, die Models in die entsprechende Ordnerstruktur eingefügt und das Entity Framework eingebunden. Danach haben wir die Verbindung zur Datenbank hergestellt. Im Zuge dessen haben wir die Datei "launchSettings.json" als Projekteigenschaft erstellt, um Variablen wie den Datenbank-Connection-String, API-Schlüssel oder den Pfad zum Ordner mit den Tiles zu hinterlegen.

Als nächstes haben wir einfache Dialoge zum Anlegen und Ändern von Touren und Tourlogs erstellt.

Daraufhin haben wir den TourService und TourLogService implementiert, um eine Schichtung (Layering) zu erreichen und die Übersichtlichkeit des Codes zu erhöhen. Im Anschluss daran haben wir die Import- und Exportfunktionalitäten umgesetzt, da diese auf den frisch ausgelagerten Funktionen basieren.

Schließlich haben wir die Suche implementiert.

## Feedback
Haben wir das MVVM-Pattern korrekt implementiert oder zumindest so, dass der Punkt "Required" als erfüllt gilt?
Muss die Suche in Postgres implementiert werden? (In den Anforderungen wird dies nicht spezifiziert.)