using UnityEngine;

namespace RampCreation
{
    public static class Extension
    {
        public static T[] Concat<T>(this T[] first, T[] second)
        {
            T[] ret = new T[first.Length + second.Length];
            first.CopyTo(ret, 0);
            second.CopyTo(ret, first.Length);
            return ret;
        }

    }

    public enum RampType
    {
        OneSided,
        Tabletop,
        Hill,
    }

    [System.Serializable]
    public class SegmentData
    {
        public SegmentType segmentType = SegmentType.Ramp;
        public float rampRadius = 1;
        [Range(-180f, 180.0f)]
        public float rampEndAngle = 10;
        [Tooltip("The more tiles the better the curve. Choose one tile to have a straight ramp.")]
        public int rampTiles = 40;

        [Header("Flat Specs")]
        public float length = 1;
        public float angleWithHorizontal;

        public void OnValidate()
        {
            if (rampRadius <= 0)
            {
                rampRadius = 0.1f;
            }
            if (rampTiles < 1)
            {
                rampTiles = 1;
            }
            if (length <= 0)
            {
                length = 1;
            }
        }

        // A class constructor for ramps
        public SegmentData(float rampRadius, float rampEndAngle, int rampTiles)
        {
            this.segmentType = SegmentType.Ramp;
            this.rampRadius = rampRadius;
            this.rampEndAngle = rampEndAngle;
            this.rampTiles = rampTiles;
        }

        // A class constructor for flats
        public SegmentData(float length, float angleWithHorizontal)
        {
            this.segmentType = SegmentType.Flat;
            this.length = length;
            this.angleWithHorizontal = angleWithHorizontal;
        }
    }

    public enum SegmentType
    {
        Ramp,
        Flat,
    }

}