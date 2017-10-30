# Session Success Analyzer - Azure Functions Demo
![Session Success Analyzer](https://github.com/joshdcar/session-success-gauge-azure-functions/blob/master/session_success_analyzer_app.png "Session Success Analyzer")
The Session Success Analyzer is just a fun project I use to demo some of the capabilities of Azure Functions. The application attempts to analyze your session attendees facial expressions to gauge their response based on emotional analysis of their faces done by Azure Cognitive Services.  

Some of the concepts that it covers include:
- Working with Http and Queue Triggers
- Working with input and output bindings
- Examples of bindings that facilitate easier unit testing
- Dynamic Binding Parameters (appsettings, new-guid, etc)
- Making use of Bindings to save liens of code (e.g. working with Tables, Blobs, Queues through Binding Inputs/outputs
- Visual Studio Azure Function Development Experience

## Do you want to actually use this for your session?
I plan to build on the demo nature of this project but I do not intend to support it as a standalone app. I also intend to build on the front end app to help cover some common scenerios (e.g. changing which camera, how often photos are submitted, etc) Saying that if you would like to use it a few suggestions:

- Make sure your audience knows you are taking photos. I've used this on two occasions and the audience recognized the fun\utility of it but some might now.
- Use a high quality external camera.The built in web cameras don't typically have the resolution required combined with the small faces of a wide angle to provide good analysis.  I've had the best success elevating the camera a bit (I'm a photographer so I used a tripod)

## Running the project locally 

The repository consists of two projects:
- Session-Gauge-Web 
- Session-Gauge-Functions

## Session-Gauge-Web
This is an Angular (4) Single Page Application (SPA). The application is a simple application that is responsible for gathering the photos via your web camera (or attached camera) and submitting those photos to the Azure Function HTTP endpoint for processing.  The web application also displays the results of the analysis also retrieved from an Azure Function. 

To run locally:
- Install [Angular CLI](https://cli.angular.io/) 
- Open the folder in your favorite editor (Visual Studio Code for example)
- Update the environment.ts file with the correct Azure Function endpoint you would like to target. The default is for local Azure Functions development.
- Run the following commands:

For production release:
```sh
$ npm install
$ ng serve (Angular CLI command to run on a local web server)
```

## Session-Gauge-Functions
Ensure that you're running Visual Studio 2017 and you have the Azure Functions Extensions for Visual Studio installed. Review [requirements](https://docs.microsoft.com/en-us/azure/azure-functions/functions-develop-vs)

Additional optional requirements for running locally:
- [Azure Storage Emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator) 

This Function App consists of two functions:
- SessionAttendeePhotos - HttpTriggers (Get/Post) for submitting photos and retrieving results
- SessionAttendeePhotoQueue - Queue Trigger submits photos for analysis
- 
Update the local.settings.json with the correct Storage Connection Strings and Cognitive Services URL and Key. 

NOTE: The only functionality that cannot be run locally is the request to the Azure Cognitive Services Emotion API. All other functionality can run 100% locally.
