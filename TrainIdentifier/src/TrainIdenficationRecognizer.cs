using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace SebastianHaeni.ThermoClassification.TrainIdentifier
{
    public class TrainIdenficationRecognizer
    {
        private readonly Dictionary<char, Image<Gray, byte>> _characterMap = new Dictionary<char, Image<Gray, byte>>();

        private readonly char[][] _characterIndexes =
        {
            new[] {'!', '"', '#', '$', '%', '&', '\'', '(', ')', '*'},
            new[] {'+', ',', '-', '.', '/', '0', '1', '2', '3', '4'},
            new[] {'5', '6', '7', '8', '9', ':', ';', '<', '=', '>'},
            new[] {'?', '@', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H'},
            new[] {'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R'},
            new[] {'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '[', '\\'},
            new[] {']', '^', '_', '`', 'a', 'b', 'c', 'd', 'e', 'f'},
            new[] {'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p'},
            new[] {'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'}
        };

        private (Image<Gray, byte> template, char character)[] _numbers;

        private Image<Gray, byte> _locatorTemplate;

        private Rectangle _templateRoi = new Rectangle(22, 22, 28, 40);

        public TrainIdenficationRecognizer()
        {
            BuildMap();
            BuildNumbers();
            BuildLocatorTemplate();
        }

        public string GetIdentification(Image<Bgr, byte> image)
        {
            var scene = PrepareScene(image);

            var (startBlackOnWhite, scaleBlackOnWhite, probabilityBlackOnWhite) =
                GetStartLocation(scene, "black-on-white");
            Console.WriteLine($"Probability with black on white: {probabilityBlackOnWhite}");

            // inverse
            _locatorTemplate = _locatorTemplate.Not();
            var (startWhiteOnBlack, scaleWhiteOnBlack, probabilityWhiteOnBlack) =
                GetStartLocation(scene, "white-on-black");
            Console.WriteLine($"Probability with white on black: {probabilityWhiteOnBlack}");

            if (probabilityWhiteOnBlack < .4 && probabilityBlackOnWhite < .4)
            {
                Console.WriteLine("Starting point not found. Is there a \"CH-SBB\" text on it?");
                Environment.Exit(1);
            }

            if (probabilityBlackOnWhite > probabilityWhiteOnBlack)
            {
                scene = scene.Not();
            }

            var (start, scale) = probabilityBlackOnWhite > probabilityWhiteOnBlack
                ? (startBlackOnWhite, scaleBlackOnWhite)
                : (startWhiteOnBlack, scaleWhiteOnBlack);

            Console.WriteLine(probabilityBlackOnWhite > probabilityWhiteOnBlack
                ? "Text is black on white background"
                : "Text is white on black background");

            var croppedNumber = GetCroppedNumber(scene, start, scale);
            croppedNumber.Save("cropped.jpg");

            var estimates = EstimateNumber(croppedNumber);
            var number = new Guesstimator(ChecksumCalculator.IsValidNumber).Guesstimate(estimates);

            return number;
        }

        private (Point start, double scale, double probability) GetStartLocation(Image<Gray, byte> scene, string desc)
        {
            var best = 0.0;
            var bestScale = 0.0;
            var matchLocation = Point.Empty;

            for (var scale = .7; scale > .2; scale -= .005)
            {
                // down scale template
                var resizedTemplate = Resize(_locatorTemplate, scale);

                // find best match
                var (probability, location) = MatchTemplate(scene, resizedTemplate);

                if (probability < best)
                {
                    continue;
                }

                best = probability;
                bestScale = scale;

                matchLocation = location;
            }

            // TODO remove soon
            var debug = scene.Convert<Bgr, byte>();
            debug.Draw(new Rectangle(matchLocation, new Size(10, 10)), new Bgr(0, 0, 255), 5);
            debug.Draw($"P: {best}, S: {bestScale}", new Point(10, 30), FontFace.HersheyPlain, 2, new Bgr(0, 0, 255),
                3);
            var resizedDebug = Resize(_locatorTemplate, bestScale).Convert<Bgr, byte>();
            debug.ROI = new Rectangle(matchLocation.X, matchLocation.Y, resizedDebug.Width, resizedDebug.Height);
            resizedDebug.CopyTo(debug);
            debug.ROI = Rectangle.Empty;

            // copy template to scene
            debug.Save($"{desc}.png");

            return (matchLocation, bestScale, best);
        }

        private static (double probability, Point location) MatchTemplate(IInputArray scene, IInputArray template)
        {
            var result = new Mat();
            CvInvoke.MatchTemplate(scene, template, result, TemplateMatchingType.SqdiffNormed);

            var minVal = double.MinValue;
            var maxVal = double.MaxValue;
            var minLoc = new Point();
            var maxLoc = new Point();

            CvInvoke.MinMaxLoc(result, ref minVal, ref maxVal, ref minLoc, ref maxLoc);

            // For SQDIFF and SQDIFF_NORMED, the best matches are lower values. 
            // For all the other methods, the higher the better.

            return (1 - minVal, minLoc);
        }

        private Image<Gray, byte> Resize(CvArray<byte> template, double scale)
        {
            var width = Convert.ToInt32(template.Width * scale);
            var height = Convert.ToInt32(template.Height * scale);
            var resizedTemplate = new Image<Gray, byte>(new Size(width, height));

            CvInvoke.Resize(template, resizedTemplate, resizedTemplate.Size, 0D, 0D, Inter.Cubic);

            return resizedTemplate;
        }

        private Image<Gray, byte> GetCroppedNumber(Image<Gray, byte> scene, Point start, double scale)
        {
            var margin = Convert.ToInt32(start.X - _locatorTemplate.Width * scale * 5.3);
            var verticalPadding = Convert.ToInt32(_locatorTemplate.Height * scale * .1);
            var height = Convert.ToInt32(_locatorTemplate.Height * scale * 1.1);
            var rightCrop = Convert.ToInt32(_locatorTemplate.Width * scale * .9);

            scene.ROI = new Rectangle(
                margin,
                start.Y - verticalPadding,
                start.X - margin - rightCrop,
                height + verticalPadding);

            return scene.Copy();
        }

        private (char character, double probability)[][] EstimateNumber(Image<Gray, byte> scene)
        {
            var digits = FindDigits(scene);
            var probabilitiesPerDigit = new List<List<(char character, double probability)>>();

            foreach (var digit in digits)
            {
                var max = 0D;

                digit.Save("current-digit.png");
                var probabilities = new List<(char character, double probability)>();

                foreach (var number in _numbers)
                {
                    var numberMaxProbability = 0D;

                    // test a few variants of this number
                    foreach (var template in GetTestTemplates(number.template.Not(), digit.Size))
                    {
                        var (probability, _) = MatchTemplate(digit, template);

                        if (probability > numberMaxProbability)
                        {
                            numberMaxProbability = probability;
                        }
                    }

                    probabilities.Add((number.character, numberMaxProbability));

                    if (numberMaxProbability <= max)
                    {
                        continue;
                    }

                    max = numberMaxProbability;
                }

                /* Console.WriteLine("\n\n");
                 foreach (var probability in probabilities.OrderBy(p => p.Value))
                 {
                     Console.WriteLine($"{probability.Key}: {probability.Value}");
                 }
 
                 Console.ReadKey();*/

                probabilitiesPerDigit.Add(probabilities);
            }

            return probabilitiesPerDigit
                .Select(p => p.ToArray())
                .ToArray();
        }

        private IEnumerable<Image<Gray, byte>> GetTestTemplates(IInputArray template, Size size)
        {
            var templates = new List<Image<Gray, byte>>();

            // Trying each number template with different scales in x and y direction.
            // Trying different offsets too in x and y direction.

            for (var scaleX = .7; scaleX < 1.001; scaleX += .05)
            {
                for (var scaleY = .7; scaleY < 1.001; scaleY += .05)
                {
                    var maxOffsetX = Convert.ToInt32(size.Width - size.Width * scaleX);
                    var maxOffsetY = Convert.ToInt32(size.Height - size.Height * scaleY);

                    for (var offsetX = 0; offsetX <= Math.Max(0, maxOffsetX - 1); offsetX++)
                    {
                        for (var offsetY = 0; offsetY <= Math.Max(0, maxOffsetY - 1); offsetY++)
                        {
                            var target = new Image<Gray, byte>(size);
                            var t = new Image<Gray, byte>(
                                Convert.ToInt32(size.Width * scaleX),
                                Convert.ToInt32(size.Height * scaleY));
                            CvInvoke.Resize(template, t, t.Size, 0D, 0D, Inter.Cubic);

                            target.ROI = new Rectangle(offsetX, offsetY, t.Width, t.Height);
                            t.CopyTo(target);
                            target.ROI = Rectangle.Empty;

                            //target.Save($"{scaleX}-{scaleY}-{offsetX}-{offsetY}.png");

                            templates.Add(target);
                        }
                    }
                }
            }

            return templates;
        }

        private IEnumerable<Image<Gray, byte>> FindDigits(Image<Gray, byte> scene)
        {
            var width = scene.Width;
            var minWidth = width / 50;
            var sums = new Matrix<double>(1, width);
            var max = 0D;

            for (var x = 0; x < width; x++)
            {
                scene.ROI = new Rectangle(x, 0, 1, scene.Height);
                sums[0, x] = scene.GetSum().Intensity;
                if (sums[0, x] > max)
                {
                    max = sums[0, x];
                }
            }

            CvInvoke.Threshold(sums, sums, max / 4, 1, ThresholdType.Binary);

            var digits = new List<Image<Gray, byte>>();

            int? start = null;

            for (var i = sums.Cols - 1; i >= 0; i--)
            {
                if (Math.Abs(sums[0, i] - 1D) < 0.0001)
                {
                    if (!start.HasValue)
                    {
                        start = i;
                    }

                    continue;
                }

                if (!start.HasValue && sums[0, i] < 0.0001)
                {
                    continue;
                }

                if (start - i < minWidth)
                {
                    start = null;
                    continue;
                }

                scene.ROI = new Rectangle(i - 2, 0, Convert.ToInt32(start - i) + 4, scene.Height);
                digits.Add(scene.Copy());

                start = null;
            }

            Console.WriteLine($"Found {digits.Count} digits");

            return digits.Take(12).Reverse();
        }

        private Image<Gray, byte> PrepareScene(Image<Bgr, byte> image)
        {
            var x = Convert.ToInt32(image.Width / 5);
            var y = Convert.ToInt32(image.Height / 5 * 3);
            var width = Convert.ToInt32(image.Width / 5 * 3);
            var height = Convert.ToInt32(image.Height / 4);

            image.ROI = new Rectangle(x, y, width, height);

            var gray = image
                .Convert<Gray, byte>()
                .ThresholdAdaptive(new Gray(255), AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 15,
                    new Gray(0));

            return gray;
        }

        private void BuildMap()
        {
            var map = new Image<Gray, byte>(@"Resources\font-map-helvetica.png");
            const int tileWidth = 73;
            const int tileHeight = 86;

            for (var row = 0; row < _characterIndexes.Length; row++)
            {
                for (var col = 0; col < _characterIndexes[row].Length; col++)
                {
                    map.ROI = new Rectangle(1 + col * tileWidth, 1 + row * tileHeight, tileWidth - 1, tileHeight - 1);
                    _characterMap[_characterIndexes[row][col]] = map.Copy();
                }
            }
        }

        private void BuildNumbers()
        {
            _numbers = new[]
            {
                (_characterMap['0'], '0'),
                (_characterMap['1'], '1'),
                (_characterMap['2'], '2'),
                (_characterMap['3'], '3'),
                (_characterMap['4'], '4'),
                (_characterMap['5'], '5'),
                (_characterMap['6'], '6'),
                (_characterMap['7'], '7'),
                (_characterMap['8'], '8'),
                (_characterMap['9'], '9')
            };

            foreach (var number in _numbers)
            {
                number.template.ROI = _templateRoi;
            }
        }

        private void BuildLocatorTemplate()
        {
            var s = _characterMap['S'];
            var b = _characterMap['B'];
            const int gap = 2;

            // crop character templates
            s.ROI = _templateRoi;
            b.ROI = _templateRoi;

            // init
            var template = new Image<Gray, byte>(new Size(_templateRoi.Width * 3 + 3 * gap, _templateRoi.Height));
            template.SetValue(new Gray(255));

            // copy S
            template.ROI = new Rectangle(0, 0, _templateRoi.Width, _templateRoi.Height);
            s.CopyTo(template);

            // copy first B
            template.ROI = new Rectangle(_templateRoi.Width + gap, 0, _templateRoi.Width, _templateRoi.Height);
            b.CopyTo(template);

            // copy second B
            template.ROI = new Rectangle(2 * (_templateRoi.Width + gap), 0, template.Width, template.Height);
            b.CopyTo(template);

            template.ROI = Rectangle.Empty;

            _locatorTemplate = template;
        }
    }
}