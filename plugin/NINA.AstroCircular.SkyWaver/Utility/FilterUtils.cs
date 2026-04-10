using NINA.Core.Model.Equipment;
using NINA.Profile.Interfaces;
using System;

namespace NINA.AstroCircular.SkyWaver.Utility {

    /// <summary>
    /// Looks up filter information from the NINA profile so that the correct
    /// filter wheel slot index is used instead of a hardcoded value.
    /// </summary>
    public static class FilterUtils {

        /// <summary>
        /// Resolves a filter name to the full FilterInfo from the user's NINA profile,
        /// which contains the correct slot index for the physical filter wheel.
        /// Falls back to a name-only FilterInfo with position -1 if the filter
        /// is not found in the profile (lets NINA attempt a name-based match).
        /// </summary>
        public static FilterInfo LookupFilterInfo(string filterName, IProfileService profileService) {
            var profileFilters = profileService?.ActiveProfile?.FilterWheelSettings?.FilterWheelFilters;
            if (profileFilters != null) {
                foreach (var f in profileFilters) {
                    if (f.Name.Equals(filterName, StringComparison.OrdinalIgnoreCase)) {
                        return f;
                    }
                }
            }
            // Filter not found in profile — return with position -1 so NINA
            // does not silently select slot 0
            return new FilterInfo(filterName, 0, (short)-1);
        }
    }
}
