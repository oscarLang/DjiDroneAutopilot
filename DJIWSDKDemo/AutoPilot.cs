using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DJI.WindowsSDK;
using Windows.UI.Popups;

namespace DJIWSDKDemo
{
    class AutoPilot
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private double _timeSinceLastWayPoint;
        private bool _loaded;
        private bool _uploaded;
        private bool _started;

        public AutoPilot()
        {
            this._loaded = false;
            this._started = false;
            this._uploaded = false;
        }

        public bool Uploaded { get => _uploaded; }
        public bool Loaded { get => _loaded; }
        public bool Started { get => _started; }
        public double TimeSinceLastWayPoint
        {
            set
            {
                _timeSinceLastWayPoint = value;
            }
            get
            {
                return _timeSinceLastWayPoint;
            }
        }

        //returns a new waypoint
        public Waypoint CreateWaypoint(double lat, double lon, double alt = 10, double spd = 0, int hdng = 0)
        {
            Waypoint newWaypoint = new Waypoint()
            {
                location = new LocationCoordinate2D() { latitude = lat, longitude = lon },
                altitude = alt,
                gimbalPitch = -30,
                heading = hdng,
                speed = spd,
                turnMode = WaypointTurnMode.CLOCKWISE,
                actionRepeatTimes = 1,
                actionTimeoutInSeconds = 60,
                cornerRadiusInMeters = 0.2,
                shootPhotoTimeInterval = -1,
                shootPhotoDistanceInterval = -1,
                waypointActions = new List<WaypointAction>()
            };
            return newWaypoint;
        }

        private bool canCreateWaypoint(Velocity3D velocity)
        {
            bool canCreate = false;
            double north = velocity.x;
            double east = velocity.y;
            double speed = Math.Sqrt((east * east) + (north * north));
            if (speed * TimeSinceLastWayPoint > 0.5)
            {
                canCreate = true;
            }
            return canCreate;
        }

        private void OnPropertyChanged(string value)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(value));
            }
        }
        private WaypointMission _waypointMission;
        public WaypointMission WaypointMission
        {
            get
            {
                return _waypointMission;
            }
            set
            {
                _waypointMission = value;
            }
        }
        //creates and inits a new waypointmission
        public void InitWaypointMission(List<Waypoint> waypoints)
        {
            System.Diagnostics.Debug.WriteLine(waypoints.Count());

            WaypointMission wpMission = new WaypointMission()
            {
                waypoints = new List<Waypoint>(waypoints),
                waypointCount = waypoints.Count(),
                autoFlightSpeed = 10,
                flightPathMode = WaypointMissionFlightPathMode.NORMAL,
                repeatTimes = 0,
                gotoFirstWaypointMode = WaypointMissionGotoFirstWaypointMode.SAFELY,
                maxFlightSpeed = 10,
                finishedAction = WaypointMissionFinishedAction.GO_HOME,
                headingMode = WaypointMissionHeadingMode.AUTO,
                exitMissionOnRCSignalLostEnabled = false,
                gimbalPitchRotationEnabled = false,
              
            };
            WaypointMission = wpMission;
        }
        //loads the waypointmission into the computer memory.
        public async void LoadWaypointMission()
        {
            //loads waypoint into computer memory
            System.Diagnostics.Debug.WriteLine(WaypointMission);
            System.Diagnostics.Debug.WriteLine(WaypointMission.waypointCount);

            SDKError errGroundstn = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).SetGroundStationModeEnabledAsync(new BoolMsg { value = true });
            SDKError err = DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).LoadMission(WaypointMission);
            var message = new MessageDialog("Loaded mission: " + err.ToString());
            if (err.Equals(SDKError.NO_ERROR))
            {
                this._loaded = true;
            }
            await message.ShowAsync();

        }
        //Uploads the waypointmission to the drone.
        public async void UploadWaypointMission()
        {
            //can only be uploaded if the wpmission has been set.
            SDKError err = await DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).UploadMission();
            var message = new MessageDialog("Uploaded mission to aircraft: " + err.ToString());
            await message.ShowAsync();
            if (err.Equals(SDKError.NO_ERROR))
            {
                this._uploaded = true;
            }
        }

        public async void StartWaypointMission()
        {
            //Start the waypointmission if there is one uploaded to the drone
            var isFlying = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GetIsFlyingAsync();
            if (isFlying.value.Value.value == true)
            {
                SDKError err = await DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).StartMission();
                var message = new MessageDialog("Mission started, " + err.ToString());
                await message.ShowAsync();
                if (err.Equals(SDKError.NO_ERROR))
                {
                    this._started = true;
                }
            }
            else
            {
                var msg = new MessageDialog("Aircraft must be flying to start waypointmission");
                await msg.ShowAsync();
            }
            
        }
        public async void StopWaypointMission()
        {
            //Stops/aborts the waypointmission
            SDKError err = await DJISDKManager.Instance.WaypointMissionManager.GetWaypointMissionHandler(0).StopMission();
            var message = new MessageDialog("Mission stopped, " + err.ToString());
            await message.ShowAsync();
        }
    }
}
