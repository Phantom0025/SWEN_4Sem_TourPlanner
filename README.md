# SWEN 4 Tourplanner

## Technische Schritte & Entscheidungen
Aufgrund unserer anfänglichen Unsicherheit bezüglich des MVVM-Patterns haben wir mit der Erstellung der View und des ViewModels begonnen, um die grundlegenden Prinzipien dieses Patterns zu erfüllen. Dabei ist das "MainViewModel" immer umfangreicher geworden, und wir vermuten, dass es eigentlich aufgeteilt werden sollte.

Anschließend haben wir die View und das ViewModel mittels Bindings verknüpft, die Models in die entsprechende Ordnerstruktur eingefügt und das Entity Framework eingebunden. Danach haben wir die Verbindung zur Datenbank hergestellt. Im Zuge dessen haben wir die Datei "launchSettings.json" als Projekteigenschaft erstellt, um Variablen wie den Datenbank-Connection-String, API-Schlüssel oder den Pfad zum Ordner mit den Tiles zu hinterlegen.

Als nächstes haben wir einfache Dialoge zum Anlegen und Ändern von Touren und Tourlogs erstellt.

Daraufhin haben wir den TourService und TourLogService implementiert, um eine Schichtung (Layering) zu erreichen und die Übersichtlichkeit des Codes zu erhöhen. Im Anschluss daran haben wir die Import- und Exportfunktionalitäten umgesetzt, da diese auf den frisch ausgelagerten Funktionen basieren.

Schließlich haben wir die Suche implementiert.

## Unit Tests

Die Unit-Tests wurden ausgewählt, um die grundlegenden Funktionalitäten des Models zu überprüfen und zu testen, da sie wichtige Bestandteile der Anwendung darstellen. Sie testen, dass die Klassen richtig initialisiert werden, korrekte Standardwerte haben und ihre Methoden die erwarteten Ergebnisse liefern. Die erfolgreiche Instanziierung von `Tour` und `TourLog` ist grundlegend für die Verwendung dieser Klassen in der Anwendung. Des Weiteren wird sichergestellt, dass die Klassen `Tour` und `TourLog` ordnungsgemäß mit Standardwerten instanziiert werden können und dass diese Werte den Erwartungen entsprechen. 

## Tracked time

Insgesamt haben wir für zusammen für das Projekt 30 Stunden gebraucht

## Wireframe

![Tourplanner_wireframe](img\WireframeTourplanner.png)

Die UX besteht hauptsächlich aus einem Popup, welches alle Grundanzeigen beinhaltet. Oben kann man in der Suchleiste nach den Touren und den TourLogs suchen, welche erstellt wurden. Bei Tours kann man auf die Buttons "+","-" und "..." drücken. Beim Drücken auf "+" entsteht ein neues Popup "TourDialog". Hier kann man alle notwendigen Informationen angeben. Danach wird der Eintrag in Tours angezeigt. Beim Klick auf "...", kann man den Tour-Eintrag editieren
Im Feld rechts neben Tours, sieht man genauere Informationen, zu dem ausgewählten Tour. Darunter zählt unteranderem die Karte, die den Weg zwischen den zwei Destinationen angibt.
Zuletzt gibt es das Feld unten rechts Tour Logs. Hier kann man auch auf die gleichen Buttons, wie bei Tours klicken. Beim Klick auf "+" entsteht das Popup "TourLogDialog" ganz rechts.  

Uns war es wichtig, eine übersichtliche, simple und verständliche UX zu erstellen, damit man sich gut auskennt und zurechtfindet.

## Git Repo Link

https://github.com/Phantom0025/SWEN_4Sem_TourPlanner

## Feedback
Haben wir das MVVM-Pattern korrekt implementiert oder zumindest so, dass der Punkt "Required" als erfüllt gilt?
Muss die Suche in Postgres implementiert werden? (In den Anforderungen wird dies nicht spezifiziert.)
Zählt unsere Aufteilung als Layer based architecture?