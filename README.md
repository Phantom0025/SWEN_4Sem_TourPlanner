# SWEN 4 Tourplanner 

## Technical Steps & Decisions
Due to our initial uncertainty regarding the MVVM pattern, we started by creating the View and the ViewModel to meet the basic principles of this pattern. In the process, the "MainViewModel" became increasingly complex, and we suspect that it should actually be split up.

Subsequently, we linked the View and the ViewModel using bindings, inserted the models into the appropriate folder structure, and integrated the Entity Framework. After that, we established the connection to the database. In the course of this, we created the "launchSettings.json" file as a project property to store variables such as the database connection string, API keys, or the path to the folder with the tiles.

Next, we created simple dialogs for creating and modifying tours and tour logs.

Then, we implemented the TourService and TourLogService to achieve layering and increase code clarity. Following this, we implemented the import and export functionalities, as these are based on the newly outsourced functions.

Finally, we implemented the search feature.

## Unit Tests
The unit tests were selected to verify and test the basic functionalities of the models, as they are important components of the application. They test that the classes are properly initialized, have correct default values, and that their methods deliver the expected results. The successful instantiation of Tour and TourLog is fundamental for the use of these classes in the application. Furthermore, it ensures that the Tour and TourLog classes can be instantiated properly with default values and that these values meet expectations.

## Tracked Time
In total, we spent 40 hours together on the project.

## Wireframe
![WireframeTourplanner](https://github.com/Phantom0025/SWEN_4Sem_TourPlanner/assets/73280704/abb110e0-0ef6-44e5-bd3b-0a2d73cadd9c)

The UX mainly consists of a popup that includes all the basic displays. At the top, you can search for tours and tour logs that have been created using the search bar. For tours, you can press the "+", "-", and "..." buttons. Pressing "+" brings up a new popup "TourDialog". Here, you can enter all the necessary information. Then the entry will be displayed in Tours. By clicking on "...", you can edit the tour entry. In the field to the right of Tours, you can see more detailed information about the selected tour, including the map showing the route between the two destinations. Finally, there is the field at the bottom right for Tour Logs. Here, you can also click the same buttons as for Tours. Clicking "+" brings up the "TourLogDialog" popup on the far right.

It was important to us to create a clear, simple, and understandable UX so that users can easily familiarize themselves and navigate through the application.

## Mandatory Unique Feature
Language Filter for Tour comments: try typing "badword1" :). This filter can be customized in the code to block additional words.

## Library Decisions
As our OR Mapper, we chose Microsoft's Entity Framework because it's a well-established option with a supportive community and numerous Stack Overflow posts to help us troubleshoot errors.
For logging, we opted for NLog since we couldn't get log4net to work properly.
We continued using nUnit for unit tests, as we had in our previous project.
For PDF generation, we selected iText7 because it was the top result in my search engine.

#### Honorable Mentions:
- Npgsql for the PostgreSQL database in conjunction with the ORM.
- Newtonsoft.Json for de/serializing classes in the export functionality.


## Git Repo Link
https://github.com/Phantom0025/SWEN_4Sem_TourPlanner

## Feedback
Have we implemented the MVVM pattern correctly or at least in a way that meets the "Required" criterion? Does the search need to be implemented in Postgres? (This is not specified in the requirements.) Does our structure count as a layer-based architecture?
