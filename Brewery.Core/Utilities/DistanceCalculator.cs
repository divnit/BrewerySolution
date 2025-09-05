namespace Brewery.Core.Utilities
{
    public static class DistanceCalculator
    {
        private const double EarthRadiusKm = 6371.0;

        public static double HaversineKm(double lat1, double lon1, double lat2, double lon2)
        {
            double dLat = ToRad(lat2 - lat1);
            double dLon = ToRad(lon2 - lon1);
            double a = Math.Pow(Math.Sin(dLat / 2), 2) +
                       Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                       Math.Pow(Math.Sin(dLon / 2), 2);
            double c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
            return EarthRadiusKm * c;
        }

        private static double ToRad(double deg) => deg * Math.PI / 180.0;
    }
}
