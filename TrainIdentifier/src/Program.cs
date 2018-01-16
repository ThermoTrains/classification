using System;
using Emgu.CV;
using Emgu.CV.Structure;

namespace SebastianHaeni.ThermoClassification.TrainIdentifier
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var identifier = new TrainIdenficationRecognizer();

            Match("508526731064", identifier);
            Match("508536331111", identifier);
            Match("508586331110", identifier);
            Match("918544500088", identifier);
            Match("948505000372", identifier);
        }

        private static void Match(string name, TrainIdenficationRecognizer identifier)
        {
            Console.WriteLine($"\n///////////////////\nMatching: {name}\n");

            var result = identifier.GetIdentification(new Image<Bgr, byte>($@"..\..\..\Test\Resources\{name}.jpg"));

            Console.WriteLine($"\nExpected: {name}\nActual:   {result}");
            Console.WriteLine($"\nMatch: {name.Equals(result)}\n");
        }
    }
}
