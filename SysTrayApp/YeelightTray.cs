﻿using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Net;

namespace YeelightTray
{
    public class YeelightTray : Form
    {

        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;
        bool OnOff = true;

        private DevicesDiscovery m_DevicesDiscovery;
        private DeviceIO m_DeviceIO;
        Device device;
        static Timer myTimer = new Timer();


        public YeelightTray()
        {

            m_DeviceIO = new DeviceIO();

            m_DevicesDiscovery = new DevicesDiscovery();
            m_DevicesDiscovery.StartListening();

            //Send Discovery Message
            m_DevicesDiscovery.SendDiscoveryMessage();

            device = m_DevicesDiscovery.GetDiscoveredDevices()[m_DevicesDiscovery.GetDiscoveredDevices().FindIndex(a => a.Id == "0x00000000036da392")];

            if (m_DeviceIO.Connect(device) == true)
            {
                //Apply current device values to controls
                OnOff = device.State;
            }

            // Create a simple tray menu with only one item.
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", OnExit);

            // Create a tray icon. In this example we use a
            // standard system icon for simplicity, but you
            // can of course use your own custom icon too.
            trayIcon = new NotifyIcon();
            trayIcon.Text = "Yeelight";
            //trayIcon.Icon = new Icon(SystemIcons.Application, 40, 40);
            trayIcon.Icon = new Icon(Properties.Resources.yeelight_win_on, 40, 40);
            //trayIcon.Icon = new Icon((device.State) ? Properties.Resources.yeelight_win_on : Properties.Resources.yeelight_win_off, 40, 40);
            trayIcon.MouseClick += new MouseEventHandler(OnMouseClick);

            // Add menu to tray icon and show it.
            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;

            SunTime();

            /* Adds the event and the event handler for the method that will 
          process the timer event to the timer. */
            myTimer.Tick += new EventHandler(TimerEventProcessor);

            // Sets the timer interval to 5 seconds.
            myTimer.Interval = 120000;

            //Session Switch Event
            Microsoft.Win32.SystemEvents.SessionSwitch += new Microsoft.Win32.SessionSwitchEventHandler(SystemEvents_SessionSwitch);
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                //MessageBox.Show(e.Button.ToString());
                if (device.State)
                {
                    trayIcon.Icon = Properties.Resources.yeelight_win_off;
                    m_DeviceIO.Toggle();
                    OnOff = false;
                }
                else
                {
                    trayIcon.Icon = Properties.Resources.yeelight_win_on;
                    m_DeviceIO.Toggle();
                    OnOff = true;
                }
                
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.

            base.OnLoad(e);
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void Dispose(bool isDisposing)
        {
            trayIcon.Visible = false;
            while (trayIcon.Visible != false)
            {
                
            }
            if (isDisposing)
            {
                // Release the icon resource.
                trayIcon.Dispose();
            }

            base.Dispose(isDisposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // SysTrayApp
            // 
            this.ClientSize = new System.Drawing.Size(120, 0);
            this.Name = "Yeelight";
            this.ResumeLayout(false);

        }

        void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                if (device.State) myTimer.Start();
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                if (OnOff)
                {
                    trayIcon.Icon = Properties.Resources.yeelight_win_on;
                    m_DeviceIO.Toggle();
                }
            }
        }
        // This is the method to run when the timer is raised.
        private void TimerEventProcessor(Object myObject,
                                                EventArgs myEventArgs)
        {
            myTimer.Stop();

            trayIcon.Icon = Properties.Resources.yeelight_win_off;
            m_DeviceIO.Toggle();

        }

        private void SunTime()
        {
            DateTime civil_twilight_end = DateTime.Now.AddMinutes(1);
            string Today = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString();
            var json = new WebClient().DownloadString("https://api.sunrise-sunset.org/json?lat=48.584167&lng=-17.833611&date=" + Today);
            int cte = json.IndexOf("civil_twilight_end");
            string scte = json.Substring(cte + 21, 10);
            civil_twilight_end = DateTime.Parse(scte);

            if (civil_twilight_end < DateTime.Now & !OnOff)
            {
                trayIcon.Icon = Properties.Resources.yeelight_win_on;
                m_DeviceIO.Toggle();
                OnOff = true;
            }

        }
    }
}