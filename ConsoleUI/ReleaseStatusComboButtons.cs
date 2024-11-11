using System;
using System.Linq;

using CKAN.Extensions;
using CKAN.ConsoleUI.Toolkit;

namespace CKAN.ConsoleUI {

    /// <summary>
    /// Combo buttons for choosing release statuses
    /// </summary>
    public class ReleaseStatusComboButtons : ConsoleRadioButtons<ReleaseStatus?>
    {
        /// <param name="l">X coordinate of left edge</param>
        /// <param name="t">Y coordinate of top edge</param>
        /// <param name="header">Label to show above the list</param>
        /// <param name="nullValueString">Allow null values if non-null and represent with this string</param>
        /// <param name="value">Initially selected value</param>
        public ReleaseStatusComboButtons(int l, int t,
                                         string         header,
                                         string?        nullValueString,
                                         ReleaseStatus? value)
            : base(l, t,
                   l + UIWidth + nonNullOptions.Max(rs => baseRenderer(rs)?.Length ?? 0) - 1,
                   t + nonNullOptions.Length + (nullValueString != null ? 1 : 0),
                   header,
                   (nullValueString != null ? nonNullOptions.Prepend(null)
                                            : nonNullOptions)
                       .ToArray(),
                   value)
        {
            this.nullValueString = nullValueString;
        }

        /// <inheritdoc/>
        protected override string Renderer(ReleaseStatus? rs)
            => baseRenderer(rs) ?? nullValueString ?? "";

        private static string? baseRenderer(ReleaseStatus? rs)
            => rs.HasValue ? $"{rs.LocalizeName()} - {rs.LocalizeDescription()}"
                           : null;

        private readonly string? nullValueString;

        private static readonly ReleaseStatus?[] nonNullOptions =
            Enum.GetValues(typeof(ReleaseStatus))
                .OfType<ReleaseStatus>()
                .OrderBy(relStat => (int)relStat)
                .OfType<ReleaseStatus?>()
                .ToArray();
    }
}
