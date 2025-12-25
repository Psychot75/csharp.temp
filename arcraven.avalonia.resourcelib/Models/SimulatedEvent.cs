using System.Numerics;

namespace Arcraven.Avalonia.ResourcesLib.Models;

/// <summary>
/// A helper class that acts as a Factory to generate various event types 
/// for simulation and UI testing purposes.
/// </summary>
public class SimulatedEvent : ContinuousEvent
{

    /// <summary>
    /// Factory to generate random events of any type (Geo, RC, Continuous, Generic).
    /// </summary>
    public static class Factory
    {
        private static readonly Random Rnd = new();

        public static IEnumerable<Event> CreateRandomEvents(int count)
        {
            var list = new List<Event>();
            for (int i = 0; i < count; i++)
            {
                list.Add(CreateRandom());
            }
            return list;
        }

        private static Event CreateRandom()
        {
            var type = Rnd.Next(0, 3); // 0: Geo, 1: RC, 2: Continuous
            var severity = (Severity)Rnd.Next(1, 5);
            var now = DateTimeOffset.UtcNow;

            return type switch
            {
                0 => new GeoSpatialEvent(
                    name: $"TRK-{Rnd.Next(100, 999)}",
                    label: "UAV detected in sector",
                    severity: severity,
                    position: new Vector3(
                        (float)(45.0 + Rnd.NextDouble()), 
                        (float)(-75.0 - Rnd.NextDouble()), 
                        (float)Rnd.Next(100, 5000)),
                    startingTime: now.AddMinutes(-Rnd.Next(1, 60)),
                    description: "Unidentified aerial phenomenon entered restricted zone."
                ),
                1 => new RCEvent(
                    name: $"RBT-{Rnd.Next(10, 99)}",
                    label: "Remote Link Interruption",
                    severity: severity,
                    isControllable: Rnd.Next(0, 2) == 1,
                    startingTime: now.AddMinutes(-Rnd.Next(5, 120)),
                    description: "Keep-alive heartbeat missed. Manual override recommended."
                ),
                _ => new ContinuousEvent(
                    name: $"PROC-{Rnd.Next(1000, 9999)}",
                    label: "Hydraulic Pressure Variance",
                    severity: severity,
                    duration: TimeSpan.FromMinutes(Rnd.Next(10, 100)),
                    value: (decimal)(Rnd.NextDouble() * 100),
                    isOngoing: true,
                    startingTime: now.AddHours(-Rnd.Next(1, 10)),
                    description: "Main pump pressure fluctuation detected."
                )
            };
        }
    }
}