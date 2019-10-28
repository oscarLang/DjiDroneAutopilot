# DjiDroneAutopilot
The aim of this project is to create an autopilot for DJI Mavic Pro. The autopilot has the ability to record a flight path and after a successful flight path has been recorded the user will be able to tell the application to perform a autonomous flight of this flightpath.

## Prerequisites
* Windows 10
* An internet connection(for connecting to the dji-sdk)
* DJI Mavic 2 Pro or similar
* Micro USB-cable

## Connecting to the drone
1. Install [DJI Assistant 2 For Mavic](https://www.dji.com/se/downloads/softwares/assistant-dji-2-for-mavic). During installation, make sure to install all the drivers.
2. Connect the Drone Controller into the computer with an USB-cable
3. Run the DjiDroneAutopilot project and start the controller.
4. Start the drone

The drone should now be connected to the software and you can start flying.

If you run into problems, please take a look at this [guide](https://developer.dji.com/windows-sdk/documentation/connection/Mavic2.html) from the official DJI Windows SDK documentation.

## Flying
The autopilot uses the DJI Windows SDK to save the flight path in Waypoints. This is a built in system in the SDK which uses GPS-navigation to navigate. This means that it is recommended to fly outside and in open areas without obstacles that can block the GPS-connection.

Start by connecting the software to the Drone. If succesfull, there should be a text indicating this in the upper right corner. You should now be able to takeoff. Once the aircraft is in the air, press the button "Start Recording flight path. 
When pleased with the flight path, press "Stop Record flight path". 

You should now be able to upload the flight path to the aircraft, and then just press "Start mission". The aircraft will now perform an autonomus flight following the prerecorded flight path.
