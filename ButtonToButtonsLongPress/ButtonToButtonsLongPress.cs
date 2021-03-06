﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using HidWizards.UCR.Core.Attributes;
using HidWizards.UCR.Core.Models;
using HidWizards.UCR.Core.Models.Binding;
using Timer = System.Timers.Timer;

namespace ButtonToButtonsLongPress
{
    [Plugin("Button to Buttons (Long Press)", Group = "Button", Description = "Remap one button to two buttons\nOne is fired on short press, the other on long press")]
    [PluginInput(DeviceBindingCategory.Momentary, "Button")]
    [PluginOutput(DeviceBindingCategory.Momentary, "Button (Short)")]
    [PluginOutput(DeviceBindingCategory.Momentary, "Button (Long)")]
    public class ButtonToButtonsLongPress : Plugin
    {
        [PluginGui("Timeout in ms")]
        public int Timeout { get; set; }

        [PluginGui("Short Press Duration in ms")]
        public int ShortPressDuration { get; set; }

        private bool _longHeld;
        private readonly Timer _longPressTimer;

        public ButtonToButtonsLongPress()
        {
            Timeout = 200;
            ShortPressDuration = 50;

            _longPressTimer = new Timer();
            _longPressTimer.Elapsed += LongPressTimerElapsed;

        }

        private void LongPressTimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Press Long
            WriteOutput(1, 1);
            SetLongPressTimerState(false);
            _longHeld = true;
        }

        public override void Update(params short[] values)
        {
            var value = values[0];
            var pressed = value == 1;

            if (pressed)
            {
                SetLongPressTimerState(true);
            }
            else
            {
                SetLongPressTimerState(false);
                if (_longHeld)
                {
                    // Release Long
                    WriteOutput(1, 0);
                    _longHeld = false;
                }
                else
                {
                    // Send Short press and release
                    WriteOutput(0, 1);
                    Thread.Sleep(ShortPressDuration);
                    WriteOutput(0, 0);
                }
            }
        }

        public void SetLongPressTimerState(bool state)
        {
            if (state)
            {
                if (_longPressTimer.Enabled)
                {
                    _longPressTimer.Stop();
                }
                _longPressTimer.Interval = Timeout;
                _longPressTimer.Start();
            }
            else if (_longPressTimer.Enabled)
            {
                _longPressTimer.Stop();
            }
        }

        public override void OnDeactivate()
        {
            SetLongPressTimerState(false);
        }
    }
}
