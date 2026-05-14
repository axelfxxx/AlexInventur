using InventurApp.Models;

namespace InventurApp.Services
{
    public static class FieldMappingService
    {
        public const string CustomPrefix = "Custom:";
        private const string CustomDisplayPrefix = "Zusatzfeld: ";

        public static readonly string[] CanonicalFields =
        {
            "Artikelnummer",
            "Bezeichnung",
            "Lagerort",
            "SollMenge"
        };

        public static bool IsCustomTarget(string? target) =>
            !string.IsNullOrWhiteSpace(target) && target.StartsWith(CustomPrefix, StringComparison.OrdinalIgnoreCase);

        public static string GetCustomFieldName(string target) =>
            target.StartsWith(CustomPrefix, StringComparison.OrdinalIgnoreCase)
                ? target[CustomPrefix.Length..].Trim()
                : target.Trim();

        public static string ToCustomTarget(string fieldName) => $"{CustomPrefix}{fieldName.Trim()}";

        public static string ToCustomDisplayName(string fieldName) => $"{CustomDisplayPrefix}{fieldName.Trim()}";

        public static string GetDisplayName(AppSettings settings, string canonical)
        {
            settings.EnsureFieldDefaults();
            return settings.GetFieldName(canonical);
        }

        public static List<string> GetTargetDisplayNames(AppSettings settings, IEnumerable<string>? csvColumns = null)
        {
            settings.EnsureFieldDefaults();
            var targets = CanonicalFields.Select(f => settings.GetFieldName(f)).ToList();

            foreach (var field in settings.CustomImportFields)
                targets.Add(ToCustomDisplayName(field));

            if (csvColumns != null)
            {
                foreach (var column in csvColumns.Where(c => !string.IsNullOrWhiteSpace(c)))
                {
                    var canonical = ToCanonical(settings, column);
                    if (canonical == null)
                    {
                        var custom = ToCustomDisplayName(column);
                        if (!targets.Contains(custom, StringComparer.OrdinalIgnoreCase))
                            targets.Add(custom);
                    }
                }
            }

            return targets.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }

        public static string? ToCanonical(AppSettings settings, string? displayOrAlias)
        {
            if (string.IsNullOrWhiteSpace(displayOrAlias))
                return null;

            settings.EnsureFieldDefaults();
            var raw = displayOrAlias.Trim();

            if (raw.StartsWith(CustomPrefix, StringComparison.OrdinalIgnoreCase))
                return ToCustomTarget(GetCustomFieldName(raw));

            if (raw.StartsWith(CustomDisplayPrefix, StringComparison.OrdinalIgnoreCase))
                return ToCustomTarget(raw[CustomDisplayPrefix.Length..].Trim());

            var value = Normalize(raw);

            foreach (var field in CanonicalFields)
            {
                if (Normalize(field) == value || Normalize(settings.GetFieldName(field)) == value)
                    return field;

                if (settings.FieldImportAliases.TryGetValue(field, out var aliases) &&
                    aliases.Any(a => Normalize(a) == value))
                    return field;
            }

            foreach (var customField in settings.CustomImportFields)
            {
                if (Normalize(customField) == value)
                    return ToCustomTarget(customField);
            }

            return null;
        }

        public static string GuessTarget(AppSettings settings, string csvHeader)
        {
            var canonical = ToCanonical(settings, csvHeader);
            if (canonical == null)
                return ToCustomDisplayName(csvHeader);

            return IsCustomTarget(canonical)
                ? ToCustomDisplayName(GetCustomFieldName(canonical))
                : settings.GetFieldName(canonical);
        }

        public static bool AddAlias(AppSettings settings, string canonicalField, string alias)
        {
            if (string.IsNullOrWhiteSpace(alias) || !CanonicalFields.Contains(canonicalField))
                return false;

            settings.EnsureFieldDefaults();
            settings.FieldImportAliases.TryAdd(canonicalField, new List<string>());

            var normalizedAlias = Normalize(alias);
            var changed = false;

            foreach (var field in CanonicalFields)
            {
                if (!settings.FieldImportAliases.TryGetValue(field, out var aliases))
                    continue;

                var removed = aliases.RemoveAll(existing => Normalize(existing) == normalizedAlias);
                changed |= removed > 0;
            }

            var isBuiltInName = Normalize(settings.GetFieldName(canonicalField)) == normalizedAlias
                || Normalize(canonicalField) == normalizedAlias;

            if (!isBuiltInName)
            {
                settings.FieldImportAliases[canonicalField].Add(alias.Trim());
                changed = true;
            }

            return changed;
        }

        public static bool AddCustomField(AppSettings settings, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                return false;

            settings.EnsureFieldDefaults();
            var clean = fieldName.Trim();
            if (CanonicalFields.Any(f => Normalize(f) == Normalize(clean) || Normalize(settings.GetFieldName(f)) == Normalize(clean)))
                return false;

            if (settings.CustomImportFields.Any(f => Normalize(f) == Normalize(clean)))
                return false;

            settings.CustomImportFields.Add(clean);
            settings.CustomImportFields = settings.CustomImportFields
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(f => f)
                .ToList();
            return true;
        }

        private static string Normalize(string? value) =>
            new string((value ?? string.Empty)
                .Trim()
                .ToLowerInvariant()
                .Where(ch => !char.IsWhiteSpace(ch) && ch != '-' && ch != '_' && ch != '.' && ch != ':' && ch != '/')
                .ToArray());
    }
}
