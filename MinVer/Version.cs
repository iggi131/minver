namespace MinVer
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using static System.Math;

#pragma warning disable CA1036 // Override methods on comparable types
    public class Version : IComparable<Version>
#pragma warning restore CA1036 // Override methods on comparable types
    {
        private readonly int major;
        private readonly int minor;
        private readonly int patch;
        private readonly List<string> preReleaseIdentifiers;

        public Version() => this.preReleaseIdentifiers = new List<string> { "alpha", "0" };

        public Version(int major, int minor, int patch, IEnumerable<string> preReleaseIdentifiers)
        {
            this.major = major;
            this.minor = minor;
            this.patch = patch;
            this.preReleaseIdentifiers = preReleaseIdentifiers?.ToList() ?? new List<string>();
        }

        public override string ToString() =>
            $"{this.major}.{this.minor}.{this.patch}{(this.preReleaseIdentifiers.Count == 0 ? "" : $"-{string.Join(".", this.preReleaseIdentifiers)}")}";

        public int CompareTo(Version other)
        {
            var major = this.major.CompareTo(other.major);
            if (major != 0)
            {
                return major;
            }

            var minor = this.minor.CompareTo(other.minor);
            if (minor != 0)
            {
                return minor;
            }

            var patch = this.patch.CompareTo(other.patch);
            if (patch != 0)
            {
                return patch;
            }

            if (this.preReleaseIdentifiers.Count > 0 && other.preReleaseIdentifiers.Count == 0)
            {
                return -1;
            }

            if (this.preReleaseIdentifiers.Count == 0 && other.preReleaseIdentifiers.Count > 0)
            {
                return 1;
            }

            var maxCount = Max(this.preReleaseIdentifiers.Count, other.preReleaseIdentifiers.Count);
            for (var index = 0; index < maxCount; ++index)
            {
                if (this.preReleaseIdentifiers.Count == index && other.preReleaseIdentifiers.Count > index)
                {
                    return -1;
                }

                if (this.preReleaseIdentifiers.Count > index && other.preReleaseIdentifiers.Count == index)
                {
                    return 1;
                }

                if (int.TryParse(this.preReleaseIdentifiers[index], out var thisNumber) && int.TryParse(other.preReleaseIdentifiers[index], out var otherNumber))
                {
                    var number = thisNumber.CompareTo(otherNumber);
                    if (number != 0)
                    {
                        return number;
                    }
                }
                else
                {
                    var text = string.CompareOrdinal(this.preReleaseIdentifiers[index], other.preReleaseIdentifiers[index]);
                    if (text != 0)
                    {
                        return text;
                    }
                }
            }

            return 0;
        }

        public Version AddHeight(int height) =>
            height == 0
                ? new Version(this.major, this.minor, this.patch, this.preReleaseIdentifiers)
                : this.preReleaseIdentifiers.Count == 0
                    ? new Version(this.major, this.minor + 1, 0, new[] { "alpha", "0", height.ToString(CultureInfo.InvariantCulture) })
                    : new Version(this.major, this.minor, this.patch, this.preReleaseIdentifiers.Concat(new[] { height.ToString(CultureInfo.InvariantCulture) }));

        public static Version ParseOrDefault(string text)
        {
            if (text == default)
            {
                return null;
            }

            var numbersAndPreRelease = text.Split(new[] { '-' }, 2);
            var numbers = numbersAndPreRelease[0].Split('.');

            return
                numbers.Length == 3 &&
                    int.TryParse(numbers[0], out var major) &&
                    int.TryParse(numbers[1], out var minor) &&
                    int.TryParse(numbers[2], out var patch)
                ? new Version(major, minor, patch, numbersAndPreRelease.Length == 2 ? numbersAndPreRelease[1].Split('.') : null)
                : null;
        }
    }
}