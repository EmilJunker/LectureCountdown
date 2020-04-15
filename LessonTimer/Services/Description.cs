﻿using System;

namespace LessonTimer.Services
{
    public class Description
    {
        public string CountdownDescription { get; private set; }
        public double CountdownLength { get; private set; }

        public Description()
        {
            Reset();
        }

        public void Reset()
        {
            CountdownDescription = null;
            CountdownLength = 0;
        }

        public void Set(string description)
        {
            Reset();
            CountdownDescription = description;
        }

        public void Set(double length)
        {
            Reset();
            CountdownLength = length;
        }

        public static string GetDescriptionString(string description, double length)
        {
            if (description is null)
            {
                return GetDescriptionString(length);
            }
            return description;
        }

        public static string GetDescriptionString(double length)
        {
            return Settings.GetCountdownDescription().Replace("#", length.ToString());
        }
    }
}