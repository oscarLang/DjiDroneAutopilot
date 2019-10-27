# DjiDroneAutopilot
The aim of this project is to create an autopilot for DJI Mavic Pro. The autopilot has the ability to record a flight path and after a successful flight path has been recorded the user will be able to tell the application to perform a autonomous flight of this flightpath.

## Prerequisites
* Windows 10
* DJI Mavic 2 Pro or similar

## Installing
todo

## Flying
The autopilot uses the DJI Windows SDK to save the flight path in Waypoints. This is a built in system in the SDK which uses GPS-navigation to navigate. This means that it is recommended to fly outside and in open areas without obstacles that can block the GPS-connection.

Start by connecting the software to the Drone. If succesfull, there should be a text indicating this in the upper right corner. You should now be able to takeoff. Once the aircraft is in the air, press the button "Start Recording flight path. 
When pleased with the flight path, press "Stop Record flight path". 

You should now be able to upload the flight path to the aircraft, and then just press "Start mission". The aircraft will now perform an autonomus flight following the prerecorded flight path.
