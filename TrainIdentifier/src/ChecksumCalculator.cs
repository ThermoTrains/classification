namespace SebastianHaeni.ThermoClassification.TrainIdentifier
{
    public static class ChecksumCalculator
    {
        public static int TrainNumberCheckSum(string number)
        {
            var sum = 0;
            var multiplier = 2;

            for (var i = number.Length - 1; i != -1; i--)
            {
                var c = number[i];

                if (c >= '0' && c <= '9')
                {
                    var zsum = ushort.Parse("" + c) * multiplier;
                    sum += zsum % 10 + zsum / 10;
                }

                if (c >= '0' && c <= '9')
                {
                    multiplier = 3 - multiplier;
                }
            }

            var checkDigit = (10 - sum % 10) % 10;

            return checkDigit;
        }

        public static bool IsValidNumber(string number)
        {
            if (number.Length <= 0)
            {
                return false;
            }

            var numberWithoutCheckDigit = number.Substring(0, number.Length - 1);
            var checkDigit = number.Substring(number.Length - 1);

            return TrainNumberCheckSum(numberWithoutCheckDigit) == int.Parse(checkDigit);
        }
    }
}