// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("iT282M/gEnCgcme4RsdIldne50zNTkBPf81ORU3NTk5PxecnLqTlWuVjPgFi73rrr25OvgD6ahHb42GTHJCUNq2vsA/KndrrmFBx43QbMpq5Y5yA3JmRFcv7RmbJhnbhrZ/k+FY2Kyd9XCIA+au4b+KiY+vABvZay+uHH52xR/W9doVbt3g8VKR4IB61GsZKgq3HDQR17ikeeNdY+e/F9RrPmG7iS5QlxElYVZLT5EEYAkKeZ3suqWFbhUTXtkr5VOKYIYdnJsDnJJqnAp4PfOqVD6DXU0YUDapQ1n/NTm1/QklGZckHybhCTk5OSk9MU7UXPQtcnvW6FXuzSG4iS2VVnuABK46msA3OgedJF6BuHo/T4O/JDaqfTfBHXc4jgE1MTk9O");
        private static int[] order = new int[] { 10,1,5,8,8,9,8,13,11,9,13,13,13,13,14 };
        private static int key = 79;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
