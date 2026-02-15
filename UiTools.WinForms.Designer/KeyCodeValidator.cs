using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace UiTools.WinForms.Designer
{
    internal class KeyCodeValidator
    {
        private readonly KeyCodeRange[] keyCodeRanges;

        public KeyCodeValidator(params KeyCodeRange[] keyCodeRanges)
        {
            this.keyCodeRanges = keyCodeRanges;
        }

        static KeyCodeValidator()
        {
            var ranges = new List<KeyCodeRange> {
                new KeyCodeRange(Keys.D0, Keys.D9),
                new KeyCodeRange(Keys.NumPad0, Keys.NumPad9),
                new KeyCodeRange(Keys.Back),
                new KeyCodeRange(Keys.Delete),
                new KeyCodeRange(Keys.Left),
                new KeyCodeRange(Keys.Right)
            };
            if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == ".")
            {
                ranges.Add(new KeyCodeRange(Keys.OemPeriod)); // '.' under '>'
                ranges.Add(new KeyCodeRange(Keys.Decimal)); // '.' on NumPad (when pressed in any TextBox - always produces '.', whatever CurrentCulture.NumberFormat is)
            }
            else if (CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == ",")
                ranges.Add(new KeyCodeRange(Keys.Oemcomma)); // ',' under '<'
            DecimalInputValidator = new KeyCodeValidator(ranges.ToArray());
        }

        public bool Validate(Keys keyCode)
        {
            return keyCodeRanges.Any(range => range.Validate(keyCode));
        }

        public static readonly KeyCodeValidator IntegerInputValidator = new KeyCodeValidator(new[] {
            new KeyCodeRange(Keys.D0, Keys.D9),
            new KeyCodeRange(Keys.NumPad0, Keys.NumPad9),
            new KeyCodeRange(Keys.Back),
            new KeyCodeRange(Keys.Delete),
            new KeyCodeRange(Keys.Left),
            new KeyCodeRange(Keys.Right)
        });

        public static readonly KeyCodeValidator DecimalInputValidator; // is set in ctor

        public static readonly KeyCodeValidator DateInputValidator = new KeyCodeValidator(new[] {
            new KeyCodeRange(Keys.D0, Keys.D9),
            new KeyCodeRange(Keys.NumPad0, Keys.NumPad9),
            new KeyCodeRange(Keys.OemPeriod),
            new KeyCodeRange(Keys.Decimal),
            new KeyCodeRange(Keys.Back),
            new KeyCodeRange(Keys.Delete),
            new KeyCodeRange(Keys.Left),
            new KeyCodeRange(Keys.Right)
        });
    }

    internal class KeyCodeRange
    {
        private readonly Keys keyCodeFrom;
        private readonly Keys keyCodeTo;
        private readonly bool includingFrom;
        private readonly bool includingTo;

        public KeyCodeRange(Keys keyCodeFrom, Keys keyCodeTo, bool includingFrom = true, bool includingTo = true)
        {
            this.keyCodeFrom = keyCodeFrom;
            this.keyCodeTo = keyCodeTo;
            this.includingFrom = includingFrom;
            this.includingTo = includingTo;
        }

        public KeyCodeRange(Keys keyCode)
        {
            keyCodeFrom = keyCode;
            keyCodeTo = keyCode;
            includingFrom = true;
            includingTo = true;
        }

        public bool Validate(Keys keyCode)
        {
            bool condition1 = includingFrom
                ? keyCode >= keyCodeFrom
                : keyCode > keyCodeFrom;
            bool condition2 = includingTo
                ? keyCode <= keyCodeTo
                : keyCode < keyCodeTo;
            return condition1 && condition2;
        }
    }
}
