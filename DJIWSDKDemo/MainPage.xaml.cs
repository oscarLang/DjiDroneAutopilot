using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Geolocation;
using DJI.WindowsSDK;
using System.ComponentModel;
using Windows.UI.Popups;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DJIWSDKDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private AutoPilot autoPilot;
        private List<Waypoint> waypoints;
        public event PropertyChangedEventHandler PropertyChanged;
        private bool AirCraftConnected;


        public MainPage()
        {
            this.InitializeComponent();
            DJISDKManager.Instance.SDKRegistrationStateChanged += Instance_SDKRegistrationEvent;
            this.autoPilot = new AutoPilot();
            this.waypoints = new List<Waypoint>();
            this.AirCraftConnected = false;
            this.resetRecordingVisibility();

            StartLanding.Visibility = Visibility.Collapsed;

            //Replace with your registered App Key. Make sure your App Key matched your application's package name on DJI developer center.
            DJISDKManager.Instance.RegisterApp("054c9f4be8787e42c7e5e12a");
        }
        

        private void resetRecordingVisibility()
        {
            StartDebug.Visibility = Visibility.Visible;
            StopRecording.Visibility = Visibility.Collapsed;
            StartUpload.Visibility = Visibility.Collapsed;
            StartMission.Visibility = Visibility.Collapsed;
        }
        private async void StartRecording_Click(object sender, RoutedEventArgs e)
        {
            if (DJISDKManager.Instance.ComponentManager != null && this.AirCraftConnected)
            {
                System.Diagnostics.Debug.WriteLine(DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0));
                DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).AircraftLocationChanged += Instance_AircraftLocationChanged;
                DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).AttitudeChanged += Instance_AttitudeChanged;

                var attitude = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GetAttitudeAsync();
                LastWayPointAttitude = attitude.value.Value;
                LastWaypointTime = System.DateTime.Now;
                StartDebug.Visibility = Visibility.Collapsed;
                StopRecording.Visibility = Visibility.Visible;
            }
            else
            {
                var message = new MessageDialog("The aircraft has not been connected yet");
                await message.ShowAsync();
                System.Diagnostics.Debug.WriteLine("The Application has not been connected yet");
            }       
        }
        private async void StopRecording_Click(object sender, RoutedEventArgs e)
        {
            DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).AttitudeChanged -= Instance_AttitudeChanged;
            this.autoPilot.InitWaypointMission(this.waypoints);
            this.autoPilot.LoadWaypointMission();

            if (this.autoPilot.Loaded)
            {
                StartDebug.Visibility = Visibility.Visible;
                StopRecording.Visibility = Visibility.Collapsed;
                StartUpload.Visibility = Visibility.Visible;
            }
        }

        private BasicGeoposition simualtionLocation = new BasicGeoposition() { Latitude = 22.6308, Longitude = 113.812 };
        private async void StartSimulator_Click(object sender, RoutedEventArgs e)
        {

            SimulatorInitializationSettings settings = new SimulatorInitializationSettings
            {
                latitude = simualtionLocation.Latitude,
                longitude = simualtionLocation.Longitude,
                satelliteCount = 15
            };
            var err = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).StartSimulatorAsync(settings);
            System.Diagnostics.Debug.WriteLine(err.ToString());

        }

        private async void StartTakeOff_Click(object sender, RoutedEventArgs e)
        {
            var err = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).StartTakeoffAsync();
            System.Diagnostics.Debug.WriteLine(err.ToString());
    
            if (err.Equals(SDKError.NO_ERROR))
            {
                var message = new MessageDialog("Taking off");
                await message.ShowAsync();

                StartTakeOff.Visibility = Visibility.Collapsed;
                StartLanding.Visibility = Visibility.Visible;
            }
            else
            {
                var message = new MessageDialog("Could not take off, errror: " + err.ToString());
                await message.ShowAsync();

            }
        }

        private async void StartLanding_Click(object sender, RoutedEventArgs e)
        {
            var err = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).StartAutoLandingAsync();
            if (err.Equals(SDKError.NO_ERROR))
            {
                this.autoPilot.StopWaypointMission();
                var message = new MessageDialog("Starting landing of aircraft");
                await message.ShowAsync();


                System.Diagnostics.Debug.WriteLine(err.ToString());

                StartLanding.Visibility = Visibility.Collapsed;
                StartTakeOff.Visibility = Visibility.Visible;
            }
            else
            {
                var message = new MessageDialog("Could not land, errror: " + err.ToString());
                await message.ShowAsync();
            }
        }

        private void Upload_Click(object sender, RoutedEventArgs e)
        {
            this.autoPilot.UploadWaypointMission();
            if (this.autoPilot.Uploaded)
            {
                StartUpload.Visibility = Visibility.Collapsed;
                StartMission.Visibility = Visibility.Visible;
            }
        }
        private void StartMission_Click(object sender, RoutedEventArgs e)
        {
            this.autoPilot.StartWaypointMission();
            StartMission.Visibility = Visibility.Collapsed;
            if (this.autoPilot.Started)
            {
                StartDebug.Visibility = Visibility.Visible;
                StartMission.Visibility = Visibility.Collapsed;
            }

        }
        private async void Instance_AircraftLocationChanged(object sender, LocationCoordinate2D? value)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                if (value.HasValue)
                {
                    AircraftLocation = value.Value;
                    OutputLong.Text = value.Value.longitude.ToString();
                    OutputLat.Text = value.Value.latitude.ToString();
                }
            });
        }

        private async void Instance_AttitudeChanged(object sender, Attitude? value)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                if (value.HasValue)
                {
                    double yaw = value.Value.yaw;
                    double pitch = value.Value.pitch;
                    double roll = value.Value.roll;

                    AircraftAttitude = value.Value;
                    OutputPitch.Text = pitch.ToString();
                    OutputYaw.Text = yaw.ToString();
                    OutputRoll.Text = roll.ToString();

                    if (satisfactoryChange(yaw, LastWayPointAttitude.yaw) || satisfactoryChange(pitch, LastWayPointAttitude.pitch) || satisfactoryChange(roll, LastWayPointAttitude.roll))
                    {
                        LastWayPointAttitude = value.Value;
                        LastWaypointTime = System.DateTime.Now;
                        var altitude = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GetAltitudeAsync();
                        var velocity3D = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GetVelocityAsync();

                        var location = await DJISDKManager.Instance.ComponentManager.GetFlightControllerHandler(0, 0).GetAircraftLocationAsync();

                        Waypoint newWaypoint = this.autoPilot.CreateWaypoint(location.value.Value.latitude, location.value.Value.longitude, altitude.value.Value.value);
                        this.waypoints.Add(newWaypoint);
                        System.Diagnostics.Debug.WriteLine(this.waypoints.Count());
                    }
                }
                
            });
        }

        private System.DateTime _lastWaypointTime;
        public System.DateTime LastWaypointTime
        {
            get
            {
                return _lastWaypointTime;
            }

            set
            {
                _lastWaypointTime = value;
                //OnPropertyChanged(nameof(LastWaypointTime));
            }
        }

        private bool satisfactoryChange(double oldValue, double newValue)
        {
            //måste nog kolla tiden också 
            double changeNeeded = 20;
            double changed = oldValue - newValue;
            System.DateTime now = System.DateTime.Now;
            TimeSpan diff = now - LastWaypointTime;

            double diffSec = diff.TotalMilliseconds;

            bool satisfactory = false;

            if (changed < 0)
            {
                changed = changed * -1;
            }

            if (changed >= changeNeeded && diffSec > 1000) {
                satisfactory = true;
            }
            return satisfactory;
        }

        private async void Instance_ExecutionStateChanged(object sender, WaypointMissionExecutionState? value)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                if (value.HasValue)
                {
                    ExecutionState = value.Value;
                    if (value.Value.isExecutionFinish)
                    {
                        this.resetRecordingVisibility();
                    }
                }
            });
        }

        private WaypointMissionExecutionState _executionState = new WaypointMissionExecutionState();

        public WaypointMissionExecutionState ExecutionState
        {
            get
            {
                return _executionState;
            }
            set
            {
                _executionState = value;
                OnPropertyChanged(nameof(ExecutionState));
            }
        }

        private async void Instance_StateChanged(object sender, WaypointMissionState? value)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                if (value.HasValue)
                {
                    MissionState = value.Value;
                    WaypointState.Text = value.Value.ToString();

                }
            });
        }

        private WaypointMissionState _missionState = new WaypointMissionState();
        public WaypointMissionState MissionState
        {
            get
            {
                return _missionState;
            }

            set
            {
                _missionState = value;
                OnPropertyChanged(nameof(MissionState));
            }
        }

        private LocationCoordinate2D _aircraftLocation = new LocationCoordinate2D() { latitude = 0, longitude = 0 };
        public LocationCoordinate2D AircraftLocation
        {
            get
            {
                return _aircraftLocation;
            }
            
            set
            {
                _aircraftLocation = value;
                OnPropertyChanged(nameof(AircraftLocation));
            }
        }

        private Attitude _aircraftAttitude = new Attitude { pitch = 0, roll = 0, yaw = 0};
        public Attitude AircraftAttitude
        {
            get
            {
                return _aircraftAttitude;
            }
            set
            {
                _aircraftAttitude = value;
                OnPropertyChanged(nameof(AircraftAttitude));
            }
        }

        private Attitude _lastWaypointAttitude = new Attitude { pitch = 0, roll = 0, yaw = 0 };
        public Attitude LastWayPointAttitude
        {
            get
            {
                return _lastWaypointAttitude;
            }
            set
            {
                _lastWaypointAttitude = value;
                //OnPropertyChanged(nameof(_lastWaypointAttitude));
            }
        }

        private void OnPropertyChanged(string value)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(value));
            }
        }

        private async void Instance_SDKRegistrationEvent(SDKRegistrationState state, SDKError resultCode)
        {
            if (resultCode == SDKError.NO_ERROR)
            {
                System.Diagnostics.Debug.WriteLine("Register app successfully.");

                //The product connection state will be updated when it changes here.
                DJISDKManager.Instance.ComponentManager.GetProductHandler(0).ProductTypeChanged += async delegate (object sender, ProductTypeMsg? value)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        if (value != null && value?.value != ProductType.UNRECOGNIZED)
                        {
                            System.Diagnostics.Debug.WriteLine("The Aircraft is connected now.");
                            IsConnected.Text = "The Aircraft is connected now.";
                            this.AirCraftConnected = true;
                         }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("The Aircraft is disconnected now.");
                            IsConnected.Text = "The Aircraft is disconnected now.";
                            this.AirCraftConnected = false;

                        }
                    });
                };
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Register SDK failed, the error is: ");
                System.Diagnostics.Debug.WriteLine(resultCode.ToString());
            }
        }

        
    }
}
