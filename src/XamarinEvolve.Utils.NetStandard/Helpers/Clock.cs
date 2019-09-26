﻿using System;
using System.Diagnostics;

namespace XamarinEvolve.Utils
{
    /// <summary>
    /// Represents the current Clock / Calendar
    /// and allows for overriding the value of "Now" (current time) for testing
    /// </summary>
    public static class Clock
    {
        public static void Reset()
        {
            _override = null;
        }

        private static DateTime? _override;
        public static DateTime Now 
        {
            get => _override ?? DateTime.UtcNow;
			set => OverrideWith(value);
        }

		// Make sure the override never sneaks into a release build
		[Conditional("DEBUG")]
		static void OverrideWith(DateTime value)
		{
			_override = value;
		}
    }
}
