using System;
using System.Collections.Generic;
using System.Linq;

namespace SebastianHaeni.ThermoClassification.TrainIdentifier
{
    public class Guesstimator
    {
        private readonly Func<string, bool> _validator;

        public Guesstimator(Func<string, bool> validator)
        {
            _validator = validator;
        }

        public string Guesstimate(IEnumerable<(char character, double probability)[]> input)
        {
            var estimates = SortEstimates(input);
            var number = BuildString(estimates);

            var p = 1;

            while (!_validator.Invoke(number))
            {
                if (p > 100)
                {
                    // abort
                    return null;
                }

                var minDistance = double.PositiveInfinity;
                var minDistanceIndex = 0;

                (char character, double probability)[] target = null;

                foreach (var estimate in estimates)
                {
                    if (estimate.Length <= 1)
                    {
                        // no distance if there's only one element
                        continue;
                    }

                    var minLocalDistance = double.PositiveInfinity;
                    var minLocalDistanceIndex = 0;

                    for (var i = 0; i < estimate.Length - 1; i++)
                    {
                        var distance = estimate[i].probability - estimate[i + 1].probability + i;

                        if (distance >= minLocalDistance)
                        {
                            continue;
                        }

                        minLocalDistanceIndex = i + 1;
                        minLocalDistance = distance;
                    }

                    if (minLocalDistance >= minDistance)
                    {
                        // no minimum distance encountered
                        continue;
                    }

                    minDistance = minLocalDistance;
                    minDistanceIndex = minLocalDistanceIndex;
                    target = estimate;
                }

                if (target == null)
                {
                    // nothing to swap
                    return null;
                }

                target[minDistanceIndex].probability += p++ * 2;

                // reorder
                estimates = SortEstimates(estimates);

                number = BuildString(estimates);
                Console.WriteLine($"Guessing: {number}");
            }

            return number;
        }

        private static (char character, double probability)[][] SortEstimates(
            IEnumerable<(char character, double probability)[]> estimates)
        {
            return estimates
                .Select(estimate => estimate.OrderByDescending(e => e.probability).ToArray())
                .ToArray();
        }

        private static string BuildString(IEnumerable<(char character, double probability)[]> estimates)
        {
            var characters = estimates
                .Select(estimate => char.ToString(estimate.First().character));

            return string.Join("", characters);
        }
    }
}
